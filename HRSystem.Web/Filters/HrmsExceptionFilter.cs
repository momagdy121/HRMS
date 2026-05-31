using HRSystem.Business.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HRSystem.Web.Filters;

public class HrmsExceptionFilter : IAsyncExceptionFilter
{
    private readonly ITempDataDictionaryFactory _tempDataFactory;

    public HrmsExceptionFilter(ITempDataDictionaryFactory tempDataFactory)
    {
        _tempDataFactory = tempDataFactory;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case BusinessRuleException businessRule:
                context.Result = HandleBusinessRule(context, businessRule.Message);
                context.ExceptionHandled = true;
                break;

            case NotFoundException notFound:
                context.Result = HandleNotFound(context, notFound);
                context.ExceptionHandled = true;
                break;

            case UnauthorizedException unauthorized:
                context.Result = HandleUnauthorized(context, unauthorized.Message);
                context.ExceptionHandled = true;
                break;
        }

        return Task.CompletedTask;
    }

    private IActionResult HandleBusinessRule(ExceptionContext context, string message)
    {
        SetErrorMessage(context, message);
        return RedirectBack(context);
    }

    private IActionResult HandleNotFound(ExceptionContext context, NotFoundException exception)
    {
        SetErrorMessage(context, exception.Message);
        return new RedirectToActionResult(exception.Action, exception.Controller, new { area = exception.Area });
    }

    private IActionResult HandleUnauthorized(ExceptionContext context, string message)
    {
        SetErrorMessage(context, message);
        return new RedirectToActionResult("Login", "Account", new { area = (string?)null });
    }

    private void SetErrorMessage(ExceptionContext context, string message)
    {
        var tempData = _tempDataFactory.GetTempData(context.HttpContext);
        tempData["Error"] = message;
    }

    private IActionResult RedirectBack(ExceptionContext context)
    {
        var referer = context.HttpContext.Request.Headers.Referer.FirstOrDefault();
        if (!string.IsNullOrEmpty(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
            && string.Equals(refererUri.Host, context.HttpContext.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            return new RedirectResult(referer);
        }

        var routeValues = context.RouteData.Values;
        var controller = routeValues.TryGetValue("controller", out var c) ? c?.ToString() : null;
        var action = routeValues.TryGetValue("action", out var a) ? a?.ToString() : null;
        var area = routeValues.TryGetValue("area", out var ar) ? ar?.ToString() : null;

        if (!string.IsNullOrEmpty(controller))
        {
            return new RedirectToActionResult(action ?? "Index", controller, new { area });
        }

        return new RedirectToActionResult("Index", "Home", null);
    }
}
