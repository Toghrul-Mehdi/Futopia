namespace Futopia.UserService.Domain.Entities.Common;
public class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; } 
    public string? UpdatedBy { get; set; } 

}
