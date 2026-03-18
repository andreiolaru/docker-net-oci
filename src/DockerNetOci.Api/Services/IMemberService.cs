using DockerNetOci.Api.Domain;

namespace DockerNetOci.Api.Services;

public interface IMemberService
{
    Task<List<Member>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Member> CreateAsync(Member member, CancellationToken cancellationToken = default);
}
