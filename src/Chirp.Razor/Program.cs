using Chirp.Application.Interfaces;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string? identityConnection = builder.Configuration.GetConnectionString("IdentityConnection");

// Application DbContext (existing)
builder.Services.AddDbContext<ChirpDbContext>(options => options.UseSqlite(connectionString ?? "Data Source=Chirp.db"));

// Add Identity (minimal, keep reasonable defaults)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ChirpDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    if(!builder.Environment.IsEnvironment("Testing")) //If testing, dont seed data and dont add migrations
    // We manually "seed" data in tests for now
    {
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        DbInitializer.SeedDatabase(context);
    }

    // Identity DB migrate and seed two users (Helge and Adrian)
    context.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    async Task CreateIfMissing(string email, string password)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user {email}: {errors}");
            }
        }
    }

    // seed users synchronously to keep existing startup flow
    CreateIfMissing("ropf@itu.dk", "LetM31n!").GetAwaiter().GetResult();
    CreateIfMissing("adho@itu.dk", "    ").GetAwaiter().GetResult();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

/* using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

    Console.WriteLine("EF Core connected successfully!");
    Console.WriteLine($"Authors table count: {db.Authors.Count()}");
    Console.WriteLine($"Cheeps table count: {db.Cheeps.Count()}");
} */ //Test to check EF Core connection, keep for now, ill remove myself later when needed

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program { } // Needed for WebApplicationFactory<T>
