using DockerNetOci.Api.Data;
using DockerNetOci.Api.Domain;
using DockerNetOci.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace DockerNetOci.UnitTests.Services;

public class MemberServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly MemberService _sut;

    public MemberServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _sut = new MemberService(_dbContext);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMembers()
    {
        _dbContext.Members.AddRange(
            new Member { MemberId = 1, Source = "Source1" },
            new Member { MemberId = 2, Source = "Source2" });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyCollection()
    {
        var result = await _sut.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsMember()
    {
        _dbContext.Members.Add(new Member { MemberId = 1, Source = "TestSource" });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Source.Should().Be("TestSource");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_AddsMemberAndReturnsIt()
    {
        var member = new Member
        {
            Source = "NewSource",
            CustomField1 = "Field1",
            CustomField2 = "Field2"
        };

        var result = await _sut.CreateAsync(member);

        result.Source.Should().Be("NewSource");
        result.CustomField1.Should().Be("Field1");
        result.CustomField2.Should().Be("Field2");

        var persisted = await _dbContext.Members.FindAsync(result.MemberId);
        persisted.Should().NotBeNull();
    }
}
