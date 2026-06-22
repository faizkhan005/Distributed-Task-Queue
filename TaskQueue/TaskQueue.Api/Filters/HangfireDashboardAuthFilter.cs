using Hangfire.Dashboard;
using System.Text;

namespace TaskQueue.Api.Filters;

/// <summary>
/// Protects the Hangfire dashboard with HTTP Basic Auth.
/// Credentials are read from environment variables:
///   HANGFIRE_DASHBOARD_USER  (default: admin)
///   HANGFIRE_DASHBOARD_PASS  (default: admin — change in production)
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    private static readonly string ExpectedUser =
        Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_USER") ?? "admin";

    private static readonly string ExpectedPass =
        Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_PASS") ?? "admin";

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var header = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        try
        {
            var encoded = header["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':', 2);

            if (parts.Length == 2 && parts[0] == ExpectedUser && parts[1] == ExpectedPass)
                return true;
        }
        catch
        {
            // malformed header — fall through to challenge
        }

        Challenge(httpContext);
        return false;
    }

    private static void Challenge(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}

