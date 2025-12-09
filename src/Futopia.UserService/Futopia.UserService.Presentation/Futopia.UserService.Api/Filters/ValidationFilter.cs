using Futopia.UserService.Application.ResponceObject;
using Futopia.UserService.Application.ResponceObject.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var validationErrors = context.ModelState
                .Where(ms => ms.Value.Errors.Any())
                .SelectMany(kvp => kvp.Value.Errors.Select(error => new CustomValidationError
                {
                    PropertyName = kvp.Key,
                    ErrorMessage = error.ErrorMessage
                }))
                .ToList();
            var response = new Response(ResponseStatusCode.ValidationError)
            {
                Message = "Validation failed.",
                ValidationErrors = validationErrors
            };

            context.Result = new JsonResult(response)
            {
                StatusCode = 400
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
