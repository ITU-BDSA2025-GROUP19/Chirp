using Chirp.Application.Interfaces;
using Chirp.Domain.Entities;
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
    {
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        DbInitializer.SeedDatabase(context);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        async Task CreateIfMissing(string email, string password, string displayName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create user {email}: {errors}");
                }
            }

            // Link or create Author
            var author = context.Authors.FirstOrDefault(a => a.Email == email);
            if (author != null)
            {
                // link existing author to IdentityUser
                author.ApplicationUserId = user.Id;
                if (string.IsNullOrEmpty(author.Name))
                    author.Name = displayName;
                context.Update(author);
            }
            else
            {
                // create a new author
                author = new Author
                {
                    Name = displayName,
                    Email = email,
                    ApplicationUserId = user.Id
                };
                context.Authors.Add(author);
            }

            await context.SaveChangesAsync();
        }
        
        /* Create users and link to authors */
        CreateIfMissing("ropf@itu.dk", "LetM31n!", "Helge").GetAwaiter().GetResult();
        CreateIfMissing("adho@itu.dk", "M32Want_Access", "Adrian").GetAwaiter().GetResult();
    }
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