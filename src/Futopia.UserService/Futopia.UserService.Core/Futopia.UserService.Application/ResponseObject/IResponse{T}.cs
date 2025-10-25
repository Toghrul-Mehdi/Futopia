namespace Futopia.UserService.Application.ResponceObject;
public interface IResponse<T> : IResponse
{
    T Data { get; set; }
}
