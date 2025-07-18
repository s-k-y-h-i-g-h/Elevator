using Bunit;
using Microsoft.AspNetCore.Components;
using Elevator.Shared.Components.Auth;

namespace Elevator.Shared.Tests.Components.Auth;

/// <summary>
/// Unit tests for AuthLayout component
/// Tests layout rendering, content projection, and styling
/// Requirements: 7.1, 7.2, 7.4
/// </summary>
public class AuthLayoutTests : TestContext
{
    [Fact]
    public void AuthLayout_ShouldRenderBasicStructure()
    {
        // Act
        var component = RenderComponent<AuthLayout>();

        // Assert
        Assert.NotNull(component.Find(".auth-container"));
        Assert.NotNull(component.Find(".auth-card"));
        Assert.NotNull(component.Find(".auth-header"));
        Assert.NotNull(component.Find(".auth-content"));
    }

    [Fact]
    public void AuthLayout_WithTitle_ShouldDisplayTitle()
    {
        // Arrange
        var title = "Sign In";

        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.Title, title));

        // Assert
        var titleElement = component.Find(".auth-header h2");
        Assert.Equal(title, titleElement.TextContent);
    }

    [Fact]
    public void AuthLayout_WithSubtitle_ShouldDisplaySubtitle()
    {
        // Arrange
        var subtitle = "Enter your credentials to continue";

        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.Subtitle, subtitle));

        // Assert
        var subtitleElement = component.Find(".auth-subtitle");
        Assert.Equal(subtitle, subtitleElement.TextContent);
    }

    [Fact]
    public void AuthLayout_WithoutSubtitle_ShouldNotDisplaySubtitleElement()
    {
        // Act
        var component = RenderComponent<AuthLayout>();

        // Assert
        var subtitleElements = component.FindAll(".auth-subtitle");
        Assert.Empty(subtitleElements);
    }

    [Fact]
    public void AuthLayout_WithFooterText_ShouldDisplayFooter()
    {
        // Arrange
        var footerText = "Don't have an account? Sign up";

        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.FooterText, footerText));

        // Assert
        var footerElement = component.Find(".auth-footer p");
        Assert.Equal(footerText, footerElement.TextContent);
    }

    [Fact]
    public void AuthLayout_WithoutFooterText_ShouldNotDisplayFooterElement()
    {
        // Act
        var component = RenderComponent<AuthLayout>();

        // Assert
        var footerElements = component.FindAll(".auth-footer");
        Assert.Empty(footerElements);
    }

    [Fact]
    public void AuthLayout_WithChildContent_ShouldRenderChildContent()
    {
        // Arrange
        var childContent = (RenderFragment)(builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "test-content");
            builder.AddContent(2, "Test child content");
            builder.CloseElement();
        });

        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.ChildContent, childContent));

        // Assert
        var childElement = component.Find(".auth-content .test-content");
        Assert.Equal("Test child content", childElement.TextContent);
    }

    [Fact]
    public void AuthLayout_WithAllParameters_ShouldRenderAllElements()
    {
        // Arrange
        var title = "Create Account";
        var subtitle = "Join us today";
        var footerText = "Already have an account? Sign in";
        var childContent = (RenderFragment)(builder =>
        {
            builder.OpenElement(0, "form");
            builder.AddContent(1, "Registration form content");
            builder.CloseElement();
        });

        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.Title, title)
            .Add(p => p.Subtitle, subtitle)
            .Add(p => p.FooterText, footerText)
            .Add(p => p.ChildContent, childContent));

        // Assert
        var titleElement = component.Find(".auth-header h2");
        Assert.Equal(title, titleElement.TextContent);

        var subtitleElement = component.Find(".auth-subtitle");
        Assert.Equal(subtitle, subtitleElement.TextContent);

        var footerElement = component.Find(".auth-footer p");
        Assert.Equal(footerText, footerElement.TextContent);

        var formElement = component.Find(".auth-content form");
        Assert.Equal("Registration form content", formElement.TextContent);
    }

    [Fact]
    public void AuthLayout_ShouldHaveCorrectCssClasses()
    {
        // Act
        var component = RenderComponent<AuthLayout>();

        // Assert
        var container = component.Find("div");
        Assert.True(container.ClassList.Contains("auth-container"));

        var card = component.Find(".auth-card");
        Assert.True(card.ClassList.Contains("auth-card"));

        var header = component.Find(".auth-header");
        Assert.True(header.ClassList.Contains("auth-header"));

        var content = component.Find(".auth-content");
        Assert.True(content.ClassList.Contains("auth-content"));
    }

    [Fact]
    public void AuthLayout_WithEmptyTitle_ShouldStillRenderTitleElement()
    {
        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.Title, ""));

        // Assert
        var titleElement = component.Find(".auth-header h2");
        Assert.Equal("", titleElement.TextContent);
    }

    [Fact]
    public void AuthLayout_WithComplexChildContent_ShouldRenderCorrectly()
    {
        // Arrange
        var childContent = (RenderFragment)(builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "form-container");
            
            builder.OpenElement(2, "input");
            builder.AddAttribute(3, "type", "text");
            builder.AddAttribute(4, "placeholder", "Username");
            builder.CloseElement();
            
            builder.OpenElement(5, "button");
            builder.AddAttribute(6, "type", "submit");
            builder.AddContent(7, "Submit");
            builder.CloseElement();
            
            builder.CloseElement();
        });

        // Act
        var component = RenderComponent<AuthLayout>(parameters => parameters
            .Add(p => p.ChildContent, childContent));

        // Assert
        var formContainer = component.Find(".auth-content .form-container");
        Assert.NotNull(formContainer);

        var input = component.Find(".auth-content input[type='text']");
        Assert.Equal("Username", input.GetAttribute("placeholder"));

        var button = component.Find(".auth-content button[type='submit']");
        Assert.Equal("Submit", button.TextContent);
    }
}