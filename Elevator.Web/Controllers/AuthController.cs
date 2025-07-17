using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid request data"
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
            {
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var token = GenerateJwtToken(user);
                return Ok(new AuthResult
                {
                    Success = true,
                    Token = token,
                    User = MapToUserDto(user)
                });
            }

            return Unauthorized(new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new AuthResult
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid request data"
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User with this email already exists"
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return Ok(new AuthResult
                {
                    Success = true,
                    Token = token,
                    User = MapToUserDto(user)
                });
            }

            return BadRequest(new AuthResult
            {
                Success = false,
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return StatusCode(500, new AuthResult
            {
                Success = false,
                ErrorMessage = "An error occurred during registration"
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                return Unauthorized(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid token"
                });
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid token"
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                });
            }

            var newToken = GenerateJwtToken(user);
            return Ok(new AuthResult
            {
                Success = true,
                Token = newToken,
                User = MapToUserDto(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthResult
            {
                Success = false,
                ErrorMessage = "An error occurred during token refresh"
            });
        }
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var key = Encoding.ASCII.GetBytes(jwtKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim("firstName", user.FirstName ?? string.Empty),
            new Claim("lastName", user.LastName ?? string.Empty)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // Token expires in 7 days
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var key = Encoding.ASCII.GetBytes(jwtKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false // We're checking expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            if (validatedToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static UserDto MapToUserDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt ?? DateTime.UtcNow
        };
    }
}

// Request DTOs
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
}