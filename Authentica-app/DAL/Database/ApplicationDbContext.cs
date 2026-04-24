using Authentica_app.BLL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authentica_app.DAL.Database
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vault> Vaults { get; set; }
        public DbSet<VaultItem> VaultItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Vault>()
                .HasOne<IdentityUser>(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VaultItem>()
                .HasOne<Vault>(i => i.Vault)
                .WithMany(v => v.VaultItems)
                .HasForeignKey(i => i.VaultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
