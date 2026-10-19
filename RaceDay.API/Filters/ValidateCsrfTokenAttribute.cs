
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateCsrfTokenAttribute : Attribute,
    IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(
        AuthorizationFilterContext context)
    {
        var method = context.HttpContext.Request.Method;

        if (HttpMethods.IsGet(method) ||
            HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method) ||
            HttpMethods.IsTrace(method))
        {
            return;
        }

        var antiforgery = context.HttpContext.RequestServices
            .GetRequiredService<IAntiforgery>();

        if (!await antiforgery.IsRequestValidAsync(context.HttpContext))
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Invalid or missing CSRF token."
            });
        }
    }
}
