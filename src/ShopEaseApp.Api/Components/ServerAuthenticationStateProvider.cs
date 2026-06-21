using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace ShopEaseApp.Api.Components;

/// <summary>
/// Bridges the incoming HTTP request's <see cref="HttpContext.User"/> into the
/// Blazor Server circuit. Blazor Server runs in-process, so the auth cookie
/// authenticated by the JWT bearer pipeline is already materialized on
/// <c>HttpContext.User</c> at render time. When no <c>HttpContext</c> is
/// available (e.g. a SignalR post-back after initial render), an anonymous
/// principal is returned so <c>&lt;AuthorizeView&gt;</c> degrades gracefully.
/// </summary>
public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _http;

    public ServerAuthenticationStateProvider(IHttpContextAccessor http) => _http = http;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }

    /// <summary>
    /// Re-reads the current <c>HttpContext.User</c> and notifies the Blazor
    /// circuit that the authentication state changed (e.g. after login/logout).
    /// </summary>
    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
