using Microsoft.AspNetCore.Identity;

namespace Futopia.UserService.Domain.Entities;
public class AppUser : IdentityUser
{
    public string FullName { get; set; }
}
