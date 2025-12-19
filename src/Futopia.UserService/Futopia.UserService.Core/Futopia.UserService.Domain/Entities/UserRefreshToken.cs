using Futopia.UserService.Domain.Entities.Common;

namespace Futopia.UserService.Domain.Entities;
public class UserRefreshToken : BaseEntity
{
    public string UserId { get; set; }
    public AppUser User { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
