using DockerNetOci.Api.Domain;
using DockerNetOci.Api.Services;

namespace DockerNetOci.Api.Endpoints;

public record CreateMemberRequest(string Source, string? CustomField1 = null, string? CustomField2 = null);

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMemberService memberService, CancellationToken ct) =>
        {
            var members = await memberService.GetAllAsync(ct);
            return Results.Ok(members);
        });

        group.MapGet("/{id:int}", async (int id, IMemberService memberService, CancellationToken ct) =>
        {
            var member = await memberService.GetByIdAsync(id, ct);
            return member is not null ? Results.Ok(member) : Results.NotFound();
        });

        group.MapPost("/", async (CreateMemberRequest request, IMemberService memberService, CancellationToken ct) =>
        {
            var member = new Member
            {
                Source = request.Source,
                CustomField1 = request.CustomField1,
                CustomField2 = request.CustomField2
            };

            var created = await memberService.CreateAsync(member, ct);
            return Results.Created($"/api/members/{created.MemberId}", created);
        });

        return group;
    }
}
