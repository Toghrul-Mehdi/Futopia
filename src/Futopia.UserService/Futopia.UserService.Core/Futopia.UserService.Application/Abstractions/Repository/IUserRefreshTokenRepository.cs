using Futopia.UserService.Application.Abstractions.Repository.Generic;
using Futopia.UserService.Domain.Entities;

namespace Futopia.UserService.Application.Abstractions.Repository;

public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken>
{
    // Token ilə refresh token-i tapmaq
    Task<UserRefreshToken?> GetByTokenAsync(string refreshToken);    
}
