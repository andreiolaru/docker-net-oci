using System.Net;
using System.Net.Http.Json;
using DockerNetOci.Api.Domain;
using DockerNetOci.IntegrationTests.Collections;
using DockerNetOci.IntegrationTests.Fixtures;
using FluentAssertions;

namespace DockerNetOci.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class MemberEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MemberEndpointTests(OracleContainerFixture oracle)
    {
        _factory = new CustomWebApplicationFactory(oracle.ConnectionString);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/members");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.Content.ReadFromJsonAsync<List<Member>>();
        members.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_CreatesMember_ReturnsCreated()
    {
        var newMember = new { Source = "IntegrationTest", CustomField1 = "Val1", CustomField2 = "Val2" };

        var response = await _client.PostAsJsonAsync("/api/members", newMember);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Member>();
        created.Should().NotBeNull();
        created!.MemberId.Should().BeGreaterThan(0);
        created.Source.Should().Be("IntegrationTest");
        created.CustomField1.Should().Be("Val1");
        created.CustomField2.Should().Be("Val2");
    }

    [Fact]
    public async Task GetById_WithExistingMember_ReturnsMember()
    {
        var newMember = new { Source = "GetByIdTest" };
        var postResponse = await _client.PostAsJsonAsync("/api/members", newMember);
        var created = await postResponse.Content.ReadFromJsonAsync<Member>();

        var response = await _client.GetAsync($"/api/members/{created!.MemberId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var member = await response.Content.ReadFromJsonAsync<Member>();
        member.Should().NotBeNull();
        member!.Source.Should().Be("GetByIdTest");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/members/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
