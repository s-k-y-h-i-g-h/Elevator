using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Elevator.Shared.Components.Auth;
using Elevator.Shared.Models;
using Elevator.Shared.Services;

namespace Elevator.Shared.Tests.Components.Auth;

/// <summary>
/// Unit tests for LoginForm component
/// Tests form rendering, validation, error display, and user interaction
/// Requirements: 7.1, 7.2, 7.4
/// </summary>
public class LoginFormTests : TestContext
{
    private readonly Mock<IAuthValidationService> _mockValidationService;

    public LoginFormTests()
    {
        _mockValidationService = new Mock<IAuthValidationService>();
        Services.AddSingleton(_mockValidationService.Object);
    }

    [Fact]
    public void LoginForm_ShouldRenderCorrectly()
    {
        // Act
        var component = RenderComponent<LoginForm>();

        // Assert
        Assert.NotNull(component.Find("form"));
        Assert.NotNull(component.Find("input[id='email']"));
        Assert.NotNull(component.Find("input[id='password']"));
        Assert.NotNull(component.Find("button[type='submit']"));
        
        // Check labels
        var emailLabel = component.Find("label[for='email']");
        Assert.Equal("Email", emailLabel.TextContent);
        
        var passwordLabel = component.Find("label[for='password']");
        Assert.Equal("Password", passwordLabel.TextContent);
        
        // Check button text
        var submitButton = component.Find("button[type='submit']");
        Assert.Contains("Sign In", submitButton.TextContent);
    }

    [Fact]
    public void LoginForm_WithErrorMessage_ShouldDisplayError()
    {
        // Arrange
        var errorMessage = "Invalid email or password";

        // Act
        var component = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.ErrorMessage, errorMessage));

        // Assert
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal(errorMessage, errorDiv.TextContent);
    }

    [Fact]
    public void LoginForm_WithoutErrorMessage_ShouldNotDisplayErrorDiv()
    {
        // Act
        var component = RenderComponent<LoginForm>();

        // Assert
        var errorDivs = component.FindAll(".alert.alert-error");
        Assert.Empty(errorDivs);
    }

    [Fact]
    public void LoginForm_WhenLoading_ShouldShowLoadingState()
    {
        // Act
        var component = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.IsLoading, true));

        // Assert
        var submitButton = component.Find("button[type='submit']");
        Assert.True(submitButton.HasAttribute("disabled"));
        Assert.Contains("Signing in...", submitButton.TextContent);
        Assert.NotNull(component.Find(".spinner"));
    }

    [Fact]
    public void LoginForm_WhenNotLoading_ShouldShowNormalState()
    {
        // Act
        var component = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.IsLoading, false));

        // Assert
        var submitButton = component.Find("button[type='submit']");
        Assert.False(submitButton.HasAttribute("disabled"));
        Assert.Contains("Sign In", submitButton.TextContent);
        Assert.Empty(component.FindAll(".spinner"));
    }

    [Fact]
    public void LoginForm_WithValidInput_ShouldCallOnSubmit()
    {
        // Arrange
        var submittedRequest = (LoginRequest?)null;
        var validationResult = new ValidationResult { IsValid = true };
        
        _mockValidationService
            .Setup(x => x.ValidateLoginRequest(It.IsAny<LoginRequest>()))
            .Returns(validationResult);

        var component = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.OnSubmit, request => submittedRequest = request));

        // Act
        var emailInput = component.Find("input[id='email']");
        var passwordInput = component.Find("input[id='password']");
        var form = component.Find("form");

        emailInput.Change("test@example.com");
        passwordInput.Change("TestPassword123");
        form.Submit();

        // Assert
        Assert.NotNull(submittedRequest);
        Assert.Equal("test@example.com", submittedRequest.Email);
        Assert.Equal("TestPassword123", submittedRequest.Password);
        
        _mockValidationService.Verify(x => x.ValidateLoginRequest(It.IsAny<LoginRequest>()), Times.Once);
    }

    [Fact]
    public void LoginForm_WithInvalidInput_ShouldShowValidationError()
    {
        // Arrange
        var validationResult = new ValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = "Email and password are required" 
        };
        
        _mockValidationService
            .Setup(x => x.ValidateLoginRequest(It.IsAny<LoginRequest>()))
            .Returns(validationResult);

        var component = RenderComponent<LoginForm>();

        // Act
        var form = component.Find("form");
        form.Submit();

        // Assert
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal("Email and password are required", errorDiv.TextContent);
        
        _mockValidationService.Verify(x => x.ValidateLoginRequest(It.IsAny<LoginRequest>()), Times.Once);
    }

    [Fact]
    public void LoginForm_InputFields_ShouldHaveCorrectAttributes()
    {
        // Act
        var component = RenderComponent<LoginForm>();

        // Assert
        var emailInput = component.Find("input[id='email']");
        Assert.Contains("form-control", emailInput.GetAttribute("class"));
        Assert.Equal("Enter your email", emailInput.GetAttribute("placeholder"));

        var passwordInput = component.Find("input[id='password']");
        Assert.Equal("password", passwordInput.GetAttribute("type"));
        Assert.Contains("form-control", passwordInput.GetAttribute("class"));
        Assert.Equal("Enter your password", passwordInput.GetAttribute("placeholder"));
    }

    [Fact]
    public void LoginForm_ShouldBindInputValues()
    {
        // Act
        var component = RenderComponent<LoginForm>();
        var emailInput = component.Find("input[id='email']");
        var passwordInput = component.Find("input[id='password']");

        // Act
        emailInput.Change("user@test.com");
        passwordInput.Change("mypassword");

        // Assert
        Assert.Equal("user@test.com", emailInput.GetAttribute("value"));
        Assert.Equal("mypassword", passwordInput.GetAttribute("value"));
    }

    [Fact]
    public void LoginForm_AfterValidationError_ShouldClearErrorOnValidSubmit()
    {
        // Arrange
        var submittedRequest = (LoginRequest?)null;
        var invalidValidation = new ValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = "Invalid input" 
        };
        var validValidation = new ValidationResult { IsValid = true };

        _mockValidationService
            .SetupSequence(x => x.ValidateLoginRequest(It.IsAny<LoginRequest>()))
            .Returns(invalidValidation)
            .Returns(validValidation);

        var component = RenderComponent<LoginForm>(parameters => parameters
            .Add(p => p.OnSubmit, request => submittedRequest = request));

        var form = component.Find("form");
        var emailInput = component.Find("input[id='email']");
        var passwordInput = component.Find("input[id='password']");

        // Act - First submit with invalid data
        form.Submit();
        
        // Assert - Error should be shown
        var errorDiv = component.Find(".alert.alert-error");
        Assert.Equal("Invalid input", errorDiv.TextContent);

        // Act - Second submit with valid data
        emailInput.Change("test@example.com");
        passwordInput.Change("TestPassword123");
        form.Submit();

        // Assert - Error should be cleared and form submitted
        var errorDivs = component.FindAll(".alert.alert-error");
        Assert.Empty(errorDivs);
        Assert.NotNull(submittedRequest);
    }
}