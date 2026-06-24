using Bunit;
using ShopEaseApp.Api.Components.DesignSystem;

namespace ShopEaseApp.Blazor.Tests;

public class LogoutTests
{
    [Fact]
    public void Logout_NavigatesToCookieClearEndpoint()
    {
        using var ctx = new TestContext();

        ctx.RenderComponent<LogoutPage>();

        var nav = (Microsoft.AspNetCore.Components.NavigationManager)
            ctx.Services.GetService(typeof(Microsoft.AspNetCore.Components.NavigationManager))!;

        Assert.StartsWith("http://localhost/auth/clear-cookie", nav.Uri);
    }
}
