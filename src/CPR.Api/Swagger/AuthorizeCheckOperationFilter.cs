using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace CPR.Api.Swagger;

/// <summary>
/// Adds the JWT security requirement to Swagger operations that have the [Authorize] attribute.
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    /// <summary>
    /// Apply the operation filter: when an operation or its declaring controller has [Authorize], add the Bearer security requirement.
    /// </summary>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
        var hasAuthorize = actionDescriptor?.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true
                           || actionDescriptor?.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

        if (!hasAuthorize) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } };
        operation.Security.Add(new OpenApiSecurityRequirement { [scheme] = new string[] { } });
    }
}
