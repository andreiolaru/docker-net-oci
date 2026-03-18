using DockerNetOci.Api.Data;
using DockerNetOci.Api.Endpoints;
using DockerNetOci.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var connectionString = builder.Configuration.GetConnectionString("OracleDb")
    ?? throw new InvalidOperationException("Connection string 'OracleDb' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString));

builder.Services.AddScoped<IMemberService, MemberService>();

builder.Services.AddHealthChecks()
    .AddOracle(connectionString, name: "oracle-db");

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");

app.MapGroup("/api/members")
    .MapMemberEndpoints()
    .WithTags("Members");

app.Run();

public partial class Program { }
