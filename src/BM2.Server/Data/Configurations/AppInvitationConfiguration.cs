using BM2.Contracts.Invitations;
using BM2.Domain.Invitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BM2.Server.Data.Configurations;

internal sealed class AppInvitationConfiguration : IEntityTypeConfiguration<AppInvitation>
{
    public void Configure(EntityTypeBuilder<AppInvitation> builder)
    {
        builder.Property(i => i.TokenHash).HasMaxLength(32).IsFixedLength();
        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.Property(i => i.Email).HasMaxLength(CreateAppInvitationRequestValidator.MaxEmail);
        builder.HasIndex(i => i.CreatedByUserId);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(i => i.CreatedByUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
