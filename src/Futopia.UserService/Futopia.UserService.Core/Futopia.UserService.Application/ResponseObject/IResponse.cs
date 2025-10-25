using Futopia.UserService.Application.ResponceObject.Enums;

namespace Futopia.UserService.Application.ResponceObject;
public interface IResponse
{
    string Message { get; set; }
    ResponseStatusCode ResponseStatusCode { get; set; }
    IEnumerable<CustomValidationError> ValidationErrors { get; set; }
    IEnumerable<CustomError> Errors { get; set; }
}
