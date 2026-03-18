using DockerNetOci.Api.Data;
using DockerNetOci.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace DockerNetOci.Api.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _dbContext;

    public MemberService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Member>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Members.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Members.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MemberId == id, cancellationToken);
    }

    public async Task<Member> CreateAsync(Member member, CancellationToken cancellationToken = default)
    {
        _dbContext.Members.Add(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return member;
    }
}
