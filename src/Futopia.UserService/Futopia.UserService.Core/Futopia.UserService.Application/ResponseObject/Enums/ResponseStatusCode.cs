using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;
namespace Futopia.UserService.Application.ResponceObject.Enums;
[JsonConverter(typeof(StringEnumConverter))]
public enum ResponseStatusCode
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Error = 4,
    Forbidden = 5,
    Unauthorized = 6,
    Conflict = 7,
    Created = 8,
    NoContent = 9,
    EmailNotConfirmed = 10
}
