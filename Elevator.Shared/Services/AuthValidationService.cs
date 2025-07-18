using System.Text.RegularExpressions;
using Elevator.Shared.Models;

namespace Elevator.Shared.Services;

/// <summary>
/// Service for validating authentication-related data
/// </summary>
public class AuthValidationService : IAuthValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Validates an email address format
    /// </summary>
    public ValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationResult.Error("Email and password are required");
        
        if (!IsValidEmailFormat(email))
            return ValidationResult.Error("Please enter a valid email address");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates password strength and requirements
    /// </summary>
    public ValidationResult ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return ValidationResult.Error("Email and password are required");
        
        if (password.Length < 8)
            return ValidationResult.Error("Password must be at least 8 characters long");
        
        if (!HasRequiredPasswordComplexity(password))
            return ValidationResult.Error("Password must contain at least one uppercase letter, one lowercase letter, and one number");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a complete login request
    /// </summary>
    public ValidationResult ValidateLoginRequest(LoginRequest request)
    {
        if (request == null)
            return ValidationResult.Error("Invalid request");
            
        var emailValidation = ValidateEmail(request.Email);
        if (!emailValidation.IsValid)
            return emailValidation;
            
        if (string.IsNullOrWhiteSpace(request.Password))
            return ValidationResult.Error("Email and password are required");
            
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a complete registration request
    /// </summary>
    public ValidationResult ValidateRegisterRequest(RegisterRequest request)
    {
        if (request == null)
            return ValidationResult.Error("Invalid request");
            
        var emailValidation = ValidateEmail(request.Email);
        if (!emailValidation.IsValid)
            return emailValidation;
            
        var passwordValidation = ValidatePassword(request.Password);
        if (!passwordValidation.IsValid)
            return passwordValidation;
            
        if (request.Password != request.ConfirmPassword)
            return ValidationResult.Error("Passwords do not match");
            
        return ValidationResult.Success();
    }
    
    private static bool IsValidEmailFormat(string email)
    {
        return EmailRegex.IsMatch(email);
    }
    
    private static bool HasRequiredPasswordComplexity(string password)
    {
        return password.Any(char.IsUpper) && 
               password.Any(char.IsLower) && 
               password.Any(char.IsDigit);
    }
}