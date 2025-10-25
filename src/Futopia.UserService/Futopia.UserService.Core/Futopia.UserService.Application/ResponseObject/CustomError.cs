namespace Futopia.UserService.Application.ResponceObject;
public class CustomError
{
    public string Code { get; set; }
    public string Description { get; set; }
    public CustomError(string code, string description)
    {
        Code = code;
        Description = description;
    }
    public CustomError()
    {
    }
}
