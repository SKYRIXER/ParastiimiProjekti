using System;
using System.Collections.Generic;
using System.Security.Claims;
using db_testiä.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace db_testiä.Authentication
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private const string SessionKey = "UserSession";
        private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(
            ProtectedSessionStorage sessionStorage,
            ILogger<CustomAuthStateProvider> logger)
        {
            _sessionStorage = sessionStorage;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var storedSession = await _sessionStorage.GetAsync<UserSession>(SessionKey);
                if (!storedSession.Success || storedSession.Value is null)
                {
                    return new AuthenticationState(Anonymous);
                }

                var session = storedSession.Value;
                if (string.IsNullOrWhiteSpace(session.UserName))
                {

                    await TryDeleteSessionAsync("empty user name");
                    return new AuthenticationState(Anonymous);
                }

                var user = CreateClaimsPrincipal(session);
                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                if (IsJavaScriptInteropUnavailable(ex))
                {
                    _logger.LogDebug(ex, "Authentication state defaults to anonymous while JavaScript interop is unavailable during prerendering.");
                }
                else
                {
                    _logger.LogWarning(ex, "Failed to read the authentication session.");
                    await TryDeleteSessionAsync("read failure");
                }

                return new AuthenticationState(Anonymous);
            }
        }

        public async Task UpdateAuthenticationState(UserSession? session)
        {
            if (session is null)
            {
                await TryDeleteSessionAsync("logout");
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
                return;
            }

            await _sessionStorage.SetAsync(SessionKey, session);
            var user = CreateClaimsPrincipal(session);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(UserSession session)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, session.UserName)
            };

            if (!string.IsNullOrWhiteSpace(session.UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, session.UserId));
                claims.Add(new Claim("UserId", session.UserId));
            }

            var identity = new ClaimsIdentity(claims, nameof(CustomAuthStateProvider));
            return new ClaimsPrincipal(identity);
        }

        private async Task TryDeleteSessionAsync(string reason)
        {
            try
            {
                await _sessionStorage.DeleteAsync(SessionKey);
            }
            catch (Exception cleanupEx)
            {
                if (IsJavaScriptInteropUnavailable(cleanupEx))
                {
                    _logger.LogDebug(cleanupEx, "Skipped clearing the authentication session ({Reason}) because JavaScript interop is unavailable.", reason);
                }
                else
                {
                    _logger.LogDebug(cleanupEx, "Failed to clear the authentication session ({Reason}).", reason);
                }
            }
        }

        private static bool IsJavaScriptInteropUnavailable(Exception exception)
        {
            return exception is InvalidOperationException invalidOperationException &&
                invalidOperationException.Message.Contains("JavaScript interop calls cannot be issued", StringComparison.Ordinal);
        }
    }
}
