using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Chirp.Domain.Entities;

namespace Chirp.Infrastructure.Data;

public class ChirpDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Cheep> Cheeps { get; set; } = null!;

    public ChirpDbContext(DbContextOptions<ChirpDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Cheeps)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
