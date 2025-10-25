using Futopia.UserService.Application.ResponceObject.Enums;

namespace Futopia.UserService.Application.ResponceObject;
public class Response<T> : Response, IResponse<T>
{
    public T Data { get; set; }
    public Response(ResponseStatusCode responseType, T data) : base(responseType)
    {
        Data = data;
    }
    public Response(Response response, T data) : base(response)
    {
        Data = data;
    }
    public Response(ResponseStatusCode responseType, string message) : base(responseType, message)
    {
    }
    public Response(ResponseStatusCode responseType, T data, List<CustomValidationError> errors) : base(responseType)
    {
        Data = data;
        ValidationErrors = errors;
    }
}
