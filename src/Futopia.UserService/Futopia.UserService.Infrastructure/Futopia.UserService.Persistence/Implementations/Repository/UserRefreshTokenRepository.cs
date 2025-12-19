using Futopia.UserService.Application.Abstractions.Repository;
using Futopia.UserService.Domain.Entities;
using Futopia.UserService.Persistence.Context;
using Futopia.UserService.Persistence.Implementations.Repository.Generic;
using Microsoft.EntityFrameworkCore;

namespace Futopia.UserService.Persistence.Implementations.Repository;

public class UserRefreshTokenRepository : Repository<UserRefreshToken>, IUserRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public UserRefreshTokenRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<UserRefreshToken?> GetByTokenAsync(string refreshToken)
    {
        return await _context.UserRefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
    }
}
