using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WebApplication1.Model;

public class SessionTimeoutMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTimeoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        var userId = context.Session.GetString("UserId");
        var sessionToken = context.Session.GetString("SessionToken");

        if (userId != null && sessionToken != null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null && user.SessionToken != sessionToken)
            {
                // Invalidate the session if the session token does not match
                context.Session.Clear();
                context.Response.Redirect("/Login");
                return;
            }
        }

        await _next(context);
    }
}