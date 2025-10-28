using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Data;

public class ChirpIdentityDbContext : IdentityDbContext
{
    public ChirpIdentityDbContext(DbContextOptions<ChirpIdentityDbContext> options)
        : base(options)
    {
    }
}
