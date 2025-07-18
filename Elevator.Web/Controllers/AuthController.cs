using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Elevator.Shared.Models;
using Elevator.Shared.Services;
using Elevator.Web.Data.Models;
using Elevator.Web.Services;

namespace Elevator.Web.Controllers;

/// <summary>
/// API controller for authentication operations including registration, login, and logout
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtService;
    private readonly IAuthValidationService _validationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtService,
        IAuthValidationService validationService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _validationService = validationService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">Registration request containing email, password, and confirm password</param>
    /// <returns>Authentication response with success status and message</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Handle null request
            if (request == null)
            {
                return BadRequest(CreateErrorResponse(
                    "Invalid request data",
                    400,
                    new Dictionary<string, string[]> { { "request", new[] { "Request body is required" } } }
                ));
            }

            // Validate the request
            var validationResult = _validationService.ValidateRegisterRequest(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(CreateErrorResponse(
                    validationResult.ErrorMessage,
                    400,
                    new Dictionary<string, string[]> { { "validation", new[] { validationResult.ErrorMessage } } }
                ));
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(CreateErrorResponse(
                    "Email address is already registered",
                    409,
                    new Dictionary<string, string[]> { { "email", new[] { "Email address is already registered" } } }
                ));
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} registered successfully", request.Email);
                
                return Created($"/api/auth/user/{user.Id}", new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful. Please log in to continue."
                });
            }

            // Handle Identity errors with detailed error mapping
            var errorDict = new Dictionary<string, string[]>();
            foreach (var error in result.Errors)
            {
                var key = error.Code.ToLowerInvariant() switch
                {
                    var code when code.Contains("password") => "password",
                    var code when code.Contains("email") => "email",
                    var code when code.Contains("username") => "username",
                    _ => "general"
                };
                
                if (errorDict.ContainsKey(key))
                {
                    errorDict[key] = errorDict[key].Concat(new[] { error.Description }).ToArray();
                }
                else
                {
                    errorDict[key] = new[] { error.Description };
                }
            }

            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errorMessage);
            
            return BadRequest(CreateErrorResponse(errorMessage, 400, errorDict));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument during registration for {Email}", request?.Email ?? "unknown");
            return BadRequest(CreateErrorResponse("Invalid request data", 400));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during registration for {Email}", request?.Email ?? "unknown");
            return StatusCode(500, CreateErrorResponse("Service temporarily unavailable", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", request?.Email ?? "unknown");
            return StatusCode(500, CreateErrorResponse("An unexpected error occurred during registration. Please try again.", 500));
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>Authentication response with JWT token on success</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Handle null request
            if (request == null)
            {
                return BadRequest(CreateErrorResponse(
                    "Invalid request data",
                    400,
                    new Dictionary<string, string[]> { { "request", new[] { "Request body is required" } } }
                ));
            }

            // Validate the request
            var validationResult = _validationService.ValidateLoginRequest(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(CreateErrorResponse(
                    validationResult.ErrorMessage,
                    400,
                    new Dictionary<string, string[]> { { "validation", new[] { validationResult.ErrorMessage } } }
                ));
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(CreateErrorResponse(
                    "Invalid email or password",
                    401,
                    new Dictionary<string, string[]> { { "credentials", new[] { "Invalid email or password" } } }
                ));
            }

            // Attempt to sign in
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                // Update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var expiresAt = _jwtService.GetExpirationFromToken(token) ?? DateTime.UtcNow.AddHours(1);

                _logger.LogInformation("User {Email} logged in successfully", request.Email);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Token = token,
                    Message = "Login successful",
                    ExpiresAt = expiresAt
                });
            }

            // Handle specific sign-in results
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account locked out for {Email}", request.Email);
                return StatusCode(423, CreateErrorResponse(
                    "Account is temporarily locked due to multiple failed login attempts",
                    423,
                    new Dictionary<string, string[]> { { "lockout", new[] { "Account is temporarily locked" } } }
                ));
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login not allowed for {Email}", request.Email);
                return StatusCode(403, CreateErrorResponse(
                    "Login is not allowed for this account",
                    403,
                    new Dictionary<string, string[]> { { "account", new[] { "Account requires verification" } } }
                ));
            }

            // Handle failed login (generic response for security)
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return Unauthorized(CreateErrorResponse(
                "Invalid email or password",
                401,
                new Dictionary<string, string[]> { { "credentials", new[] { "Invalid email or password" } } }
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument during login for {Email}", request?.Email ?? "unknown");
            return BadRequest(CreateErrorResponse("Invalid request data", 400));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during login for {Email}", request?.Email ?? "unknown");
            return StatusCode(500, CreateErrorResponse("Authentication service temporarily unavailable", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", request?.Email ?? "unknown");
            return StatusCode(500, CreateErrorResponse("An unexpected error occurred during login. Please try again.", 500));
        }
    }

    /// <summary>
    /// Logs out the current user by clearing authentication state
    /// </summary>
    /// <returns>Success response indicating logout completion</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        try
        {
            // Get the current user's email for logging
            var userEmail = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? "Unknown";
            
            // Verify user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(CreateErrorResponse(
                    "User is not authenticated",
                    401,
                    new Dictionary<string, string[]> { { "authentication", new[] { "User must be logged in to logout" } } }
                ));
            }
            
            // Sign out the user (clears server-side authentication state)
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("User {Email} logged out successfully", userEmail);
            
            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Logout successful"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during logout");
            return StatusCode(500, CreateErrorResponse("Authentication service temporarily unavailable", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout");
            return StatusCode(500, CreateErrorResponse("An unexpected error occurred during logout. Please try again.", 500));
        }
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    private AuthResponse CreateErrorResponse(string message, int statusCode, Dictionary<string, string[]>? errors = null)
    {
        return new AuthResponse
        {
            Success = false,
            Message = message
        };
    }
}