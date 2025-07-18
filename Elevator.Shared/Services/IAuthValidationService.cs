using Elevator.Shared.Models;

namespace Elevator.Shared.Services;

/// <summary>
/// Interface for authentication validation services
/// </summary>
public interface IAuthValidationService
{
    /// <summary>
    /// Validates an email address format
    /// </summary>
    ValidationResult ValidateEmail(string email);
    
    /// <summary>
    /// Validates password strength and requirements
    /// </summary>
    ValidationResult ValidatePassword(string password);
    
    /// <summary>
    /// Validates a complete login request
    /// </summary>
    ValidationResult ValidateLoginRequest(LoginRequest request);
    
    /// <summary>
    /// Validates a complete registration request
    /// </summary>
    ValidationResult ValidateRegisterRequest(RegisterRequest request);
}