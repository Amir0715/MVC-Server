using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MVCS.Infrastructure.Identity.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasMany(x => x.Projects)
            .WithMany(x => x.Users);

        builder.Metadata.FindNavigation(nameof(ApplicationUser.Projects))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(ProjectTenant.Users))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}