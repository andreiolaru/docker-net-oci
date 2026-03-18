using DockerNetOci.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DockerNetOci.Api.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("MEMBERS");

        builder.HasKey(m => m.MemberId);

        builder.Property(m => m.MemberId)
            .HasColumnName("MEMBER_ID")
            .UseIdentityColumn();

        builder.Property(m => m.Source)
            .HasColumnName("SOURCE")
            .HasColumnType("VARCHAR2(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.CustomField1)
            .HasColumnName("CUSTOM_FIELD_1")
            .HasColumnType("VARCHAR2(100)")
            .HasMaxLength(100);

        builder.Property(m => m.CustomField2)
            .HasColumnName("CUSTOM_FIELD_2")
            .HasColumnType("VARCHAR2(100)")
            .HasMaxLength(100);
    }
}
