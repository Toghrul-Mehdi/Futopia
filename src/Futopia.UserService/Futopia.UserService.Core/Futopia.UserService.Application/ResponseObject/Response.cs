using Futopia.UserService.Application.ResponceObject.Enums;

namespace Futopia.UserService.Application.ResponceObject;
public class Response : IResponse
{
    public Response(ResponseStatusCode responseType)
    {
        ResponseStatusCode = responseType;
    }

    public Response(Response response)
    {
        Message = response.Message;
        ResponseStatusCode = response.ResponseStatusCode;
        ValidationErrors = response.ValidationErrors;
        Errors = response.Errors;
    }

    public Response(ResponseStatusCode responseType, IEnumerable<CustomError> errors) : this(responseType)
    {
        Errors = errors;
    }
    public Response(ResponseStatusCode responseType, string message) : this(responseType)
    {
        Message = message;
    }
    public string Message { get; set; }
    public ResponseStatusCode ResponseStatusCode { get; set; }
    public IEnumerable<CustomValidationError> ValidationErrors { get; set; }
    public IEnumerable<CustomError> Errors { get; set; }
}
