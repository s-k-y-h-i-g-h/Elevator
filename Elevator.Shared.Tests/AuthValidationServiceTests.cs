using Elevator.Shared.Models;
using Elevator.Shared.Services;

namespace Elevator.Shared.Tests;

public class AuthValidationServiceTests
{
    private readonly AuthValidationService _validationService;

    public AuthValidationServiceTests()
    {
        _validationService = new AuthValidationService();
    }

    #region Email Validation Tests

    [Fact]
    public void ValidateEmail_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var validEmail = "test@example.com";

        // Act
        var result = _validationService.ValidateEmail(validEmail);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ValidateEmail_WithEmptyOrWhitespaceEmail_ReturnsRequiredError(string? email)
    {
        // Act
        var result = _validationService.ValidateEmail(email!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Email and password are required", result.ErrorMessage);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    [InlineData("test@.com")]
    [InlineData("test@example.")]
    [InlineData("test@example")]
    public void ValidateEmail_WithInvalidEmailFormat_ReturnsFormatError(string invalidEmail)
    {
        // Act
        var result = _validationService.ValidateEmail(invalidEmail);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Please enter a valid email address", result.ErrorMessage);
    }

    [Theory]
    [InlineData("user@domain.co.uk")]
    [InlineData("test.email@example.org")]
    [InlineData("user+tag@example.com")]
    [InlineData("user_name@example-domain.com")]
    [InlineData("123@example.com")]
    public void ValidateEmail_WithVariousValidFormats_ReturnsSuccess(string validEmail)
    {
        // Act
        var result = _validationService.ValidateEmail(validEmail);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public void ValidatePassword_WithValidPassword_ReturnsSuccess()
    {
        // Arrange
        var validPassword = "Password123";

        // Act
        var result = _validationService.ValidatePassword(validPassword);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ValidatePassword_WithEmptyOrWhitespacePassword_ReturnsRequiredError(string? password)
    {
        // Act
        var result = _validationService.ValidatePassword(password!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Email and password are required", result.ErrorMessage);
    }

    [Theory]
    [InlineData("1234567")] // 7 characters
    [InlineData("Pass123")] // 7 characters
    [InlineData("a")] // 1 character
    [InlineData("Ab1")] // 3 characters
    public void ValidatePassword_WithShortPassword_ReturnsLengthError(string shortPassword)
    {
        // Act
        var result = _validationService.ValidatePassword(shortPassword);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Password must be at least 8 characters long", result.ErrorMessage);
    }

    [Theory]
    [InlineData("password123")] // No uppercase
    [InlineData("PASSWORD123")] // No lowercase
    [InlineData("PasswordABC")] // No number
    [InlineData("password")] // No uppercase, no number
    [InlineData("PASSWORD")] // No lowercase, no number
    [InlineData("12345678")] // No letters
    public void ValidatePassword_WithoutRequiredComplexity_ReturnsComplexityError(string weakPassword)
    {
        // Act
        var result = _validationService.ValidatePassword(weakPassword);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Password must contain at least one uppercase letter, one lowercase letter, and one number", result.ErrorMessage);
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("MySecure123")]
    [InlineData("Test1234")]
    [InlineData("ComplexPass9")]
    [InlineData("Aa1bcdef")]
    public void ValidatePassword_WithValidComplexity_ReturnsSuccess(string validPassword)
    {
        // Act
        var result = _validationService.ValidatePassword(validPassword);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    #endregion

    #region Login Request Validation Tests

    [Fact]
    public void ValidateLoginRequest_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act
        var result = _validationService.ValidateLoginRequest(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void ValidateLoginRequest_WithNullRequest_ReturnsError()
    {
        // Act
        var result = _validationService.ValidateLoginRequest(null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid request", result.ErrorMessage);
    }

    [Fact]
    public void ValidateLoginRequest_WithInvalidEmail_ReturnsEmailError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Password = "Password123"
        };

        // Act
        var result = _validationService.ValidateLoginRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Please enter a valid email address", result.ErrorMessage);
    }

    [Fact]
    public void ValidateLoginRequest_WithEmptyPassword_ReturnsRequiredError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var result = _validationService.ValidateLoginRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Email and password are required", result.ErrorMessage);
    }

    #endregion

    #region Register Request Validation Tests

    [Fact]
    public void ValidateRegisterRequest_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        // Act
        var result = _validationService.ValidateRegisterRequest(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void ValidateRegisterRequest_WithNullRequest_ReturnsError()
    {
        // Act
        var result = _validationService.ValidateRegisterRequest(null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid request", result.ErrorMessage);
    }

    [Fact]
    public void ValidateRegisterRequest_WithInvalidEmail_ReturnsEmailError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        // Act
        var result = _validationService.ValidateRegisterRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Please enter a valid email address", result.ErrorMessage);
    }

    [Fact]
    public void ValidateRegisterRequest_WithWeakPassword_ReturnsPasswordError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "weak",
            ConfirmPassword = "weak"
        };

        // Act
        var result = _validationService.ValidateRegisterRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Password must be at least 8 characters long", result.ErrorMessage);
    }

    [Fact]
    public void ValidateRegisterRequest_WithMismatchedPasswords_ReturnsMatchError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "DifferentPassword123"
        };

        // Act
        var result = _validationService.ValidateRegisterRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Passwords do not match", result.ErrorMessage);
    }

    #endregion
}
