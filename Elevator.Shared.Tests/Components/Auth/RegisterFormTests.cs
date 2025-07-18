using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Elevator.Shared.Components.Auth;
using Elevator.Shared.Models;
using Elevator.Shared.Services;

namespace Elevator.Shared.Tests.Components.Auth;

/// <summary>
/// Unit tests for RegisterForm component
/// Tests form rendering, validation, error display, and user interaction
/// Requirements: 7.1, 7.2, 7.4
/// </summary>
public class RegisterFormTests : TestContext
{
    private readonly Mock<IAuthValidationService> _mockValidationService;

    public RegisterFormTests()
    {
        _mockValidationService = new Mock<IAuthValidationService>();
        Services.AddSingleton(_mockValidationService.Object);
    }

    [Fact]
    public void RegisterForm_ShouldRenderCorrectly()
    {
        // Act
        var component = RenderComponent<RegisterForm>();

        // Assert
        Assert.NotNull(component.Find("form"));
        Assert.NotNull(component.Find("input[id='email']"));
        Assert.NotNull(component.Find("input[id='password']"));
        Assert.NotNull(component.Find("input[id='confirmPassword']"));
        Assert.NotNull(component.Find("button[type='submit']"));
        
        // Check labels
        var emailLabel = component.Find("label[for='email']");
        Assert.Equal("Email", emailLabel.TextContent);
        
        var passwordLabel = component.Find("label[for='password']");
        Assert.Equal("Password", passwordLabel.TextContent);
        
        var confirmPasswordLabel = component.Find("label[for='confirmPassword']");
        Assert.Equal("Confirm Password", confirmPasswordLabel.TextContent);
        
        // Check button text
        var submitButton = component.Find("button[type='submit']");
        Assert.Contains("Create Account", submitButton.TextContent);
    }

    [Fact]
    public void RegisterForm_WithErrorMessage_ShouldDisplayError()
    {
        // Arrange
        var errorMessage = "Email address is already registered";

        // Act
        var component = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.ErrorMessage, errorMessage));

        // Assert
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal(errorMessage, errorDiv.TextContent);
    }

    [Fact]
    public void RegisterForm_WithoutErrorMessage_ShouldNotDisplayErrorDiv()
    {
        // Act
        var component = RenderComponent<RegisterForm>();

        // Assert
        var errorDivs = component.FindAll(".alert.alert-error");
        Assert.Empty(errorDivs);
    }

    [Fact]
    public void RegisterForm_WhenLoading_ShouldShowLoadingState()
    {
        // Act
        var component = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.IsLoading, true));

        // Assert
        var submitButton = component.Find("button[type='submit']");
        Assert.True(submitButton.HasAttribute("disabled"));
        Assert.Contains("Creating account...", submitButton.TextContent);
        Assert.NotNull(component.Find(".spinner"));
    }

    [Fact]
    public void RegisterForm_WhenNotLoading_ShouldShowNormalState()
    {
        // Act
        var component = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.IsLoading, false));

        // Assert
        var submitButton = component.Find("button[type='submit']");
        Assert.False(submitButton.HasAttribute("disabled"));
        Assert.Contains("Create Account", submitButton.TextContent);
        Assert.Empty(component.FindAll(".spinner"));
    }

    [Fact]
    public void RegisterForm_WithValidInput_ShouldCallOnSubmit()
    {
        // Arrange
        var submittedRequest = (RegisterRequest?)null;
        var validationResult = new ValidationResult { IsValid = true };
        
        _mockValidationService
            .Setup(x => x.ValidateRegisterRequest(It.IsAny<RegisterRequest>()))
            .Returns(validationResult);

        var component = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.OnSubmit, request => submittedRequest = request));

        // Act
        var emailInput = component.Find("input[id='email']");
        var passwordInput = component.Find("input[id='password']");
        var confirmPasswordInput = component.Find("input[id='confirmPassword']");
        var form = component.Find("form");

        emailInput.Change("test@example.com");
        passwordInput.Change("TestPassword123");
        confirmPasswordInput.Change("TestPassword123");
        form.Submit();

        // Assert
        Assert.NotNull(submittedRequest);
        Assert.Equal("test@example.com", submittedRequest.Email);
        Assert.Equal("TestPassword123", submittedRequest.Password);
        Assert.Equal("TestPassword123", submittedRequest.ConfirmPassword);
        
        _mockValidationService.Verify(x => x.ValidateRegisterRequest(It.IsAny<RegisterRequest>()), Times.Once);
    }

    [Fact]
    public void RegisterForm_WithInvalidInput_ShouldShowValidationError()
    {
        // Arrange
        var validationResult = new ValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = "Password must be at least 8 characters long" 
        };
        
        _mockValidationService
            .Setup(x => x.ValidateRegisterRequest(It.IsAny<RegisterRequest>()))
            .Returns(validationResult);

        var component = RenderComponent<RegisterForm>();

        // Act
        var form = component.Find("form");
        form.Submit();

        // Assert
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal("Password must be at least 8 characters long", errorDiv.TextContent);
        
        _mockValidationService.Verify(x => x.ValidateRegisterRequest(It.IsAny<RegisterRequest>()), Times.Once);
    }

    [Fact]
    public void RegisterForm_InputFields_ShouldHaveCorrectAttributes()
    {
        // Act
        var component = RenderComponent<RegisterForm>();

        // Assert
        var emailInput = component.Find("input[id='email']");
        Assert.Contains("form-control", emailInput.GetAttribute("class"));
        Assert.Equal("Enter your email", emailInput.GetAttribute("placeholder"));

        var passwordInput = component.Find("input[id='password']");
        Assert.Equal("password", passwordInput.GetAttribute("type"));
        Assert.Contains("form-control", passwordInput.GetAttribute("class"));
        Assert.Equal("Enter your password", passwordInput.GetAttribute("placeholder"));

        var confirmPasswordInput = component.Find("input[id='confirmPassword']");
        Assert.Equal("password", confirmPasswordInput.GetAttribute("type"));
        Assert.Contains("form-control", confirmPasswordInput.GetAttribute("class"));
        Assert.Equal("Confirm your password", confirmPasswordInput.GetAttribute("placeholder"));
    }

    [Fact]
    public void RegisterForm_ShouldBindInputValues()
    {
        // Act
        var component = RenderComponent<RegisterForm>();
        var emailInput = component.Find("input[id='email']");
        var passwordInput = component.Find("input[id='password']");
        var confirmPasswordInput = component.Find("input[id='confirmPassword']");

        // Act
        emailInput.Change("user@test.com");
        passwordInput.Change("mypassword");
        confirmPasswordInput.Change("mypassword");

        // Assert
        Assert.Equal("user@test.com", emailInput.GetAttribute("value"));
        Assert.Equal("mypassword", passwordInput.GetAttribute("value"));
        Assert.Equal("mypassword", confirmPasswordInput.GetAttribute("value"));
    }

    [Fact]
    public void RegisterForm_AfterValidationError_ShouldClearErrorOnValidSubmit()
    {
        // Arrange
        var submittedRequest = (RegisterRequest?)null;
        var invalidValidation = new ValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = "Passwords do not match" 
        };
        var validValidation = new ValidationResult { IsValid = true };

        _mockValidationService
            .SetupSequence(x => x.ValidateRegisterRequest(It.IsAny<RegisterRequest>()))
            .Returns(invalidValidation)
            .Returns(validValidation);

        var component = RenderComponent<RegisterForm>(parameters => parameters
            .Add(p => p.OnSubmit, request => submittedRequest = request));

        var form = component.Find("form");
        var emailInput = component.Find("input[id='email']");
        var passwordInput = component.Find("input[id='password']");
        var confirmPasswordInput = component.Find("input[id='confirmPassword']");

        // Act - First submit with invalid data
        form.Submit();
        
        // Assert - Error should be shown
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal("Passwords do not match", errorDiv.TextContent);

        // Act - Second submit with valid data
        emailInput.Change("test@example.com");
        passwordInput.Change("TestPassword123");
        confirmPasswordInput.Change("TestPassword123");
        form.Submit();

        // Assert - Error should be cleared and form submitted
        var errorDivs = component.FindAll(".alert.alert-error");
        Assert.Empty(errorDivs);
        Assert.NotNull(submittedRequest);
    }

    [Fact]
    public void RegisterForm_ShouldHaveThreePasswordFields()
    {
        // Act
        var component = RenderComponent<RegisterForm>();

        // Assert
        var passwordInputs = component.FindAll("input[type='password']");
        Assert.Equal(2, passwordInputs.Count); // Password and Confirm Password
    }

    [Fact]
    public void RegisterForm_WithMismatchedPasswords_ShouldShowValidationError()
    {
        // Arrange
        var validationResult = new ValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = "Passwords do not match" 
        };
        
        _mockValidationService
            .Setup(x => x.ValidateRegisterRequest(It.IsAny<RegisterRequest>()))
            .Returns(validationResult);

        var component = RenderComponent<RegisterForm>();

        // Act
        var passwordInput = component.Find("input[id='password']");
        var confirmPasswordInput = component.Find("input[id='confirmPassword']");
        var form = component.Find("form");

        passwordInput.Change("TestPassword123");
        confirmPasswordInput.Change("DifferentPassword123");
        form.Submit();

        // Assert
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal("Passwords do not match", errorDiv.TextContent);
    }
}