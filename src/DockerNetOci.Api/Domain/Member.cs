namespace DockerNetOci.Api.Domain;

public class Member
{
    public int MemberId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? CustomField1 { get; set; }
    public string? CustomField2 { get; set; }
}
