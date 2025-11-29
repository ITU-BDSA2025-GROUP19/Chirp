using Chirp.Application.Interfaces;
using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<CheepRepository>();
builder.Services.AddScoped<AuthorRepository>();
builder.Services.AddScoped<FollowRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IFollowService, FollowService>();



string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Application DbContext (existing)
builder.Services.AddDbContext<ChirpDbContext>(options => options.UseSqlite(connectionString ?? "Data Source=Chirp.db"));

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddSignInManager()
    .AddEntityFrameworkStores<ChirpDbContext>();

// GitHub OAuth 
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme)  
    .AddCookie(IdentityConstants.ExternalScheme)    
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["authentication:github:clientId"];
        o.ClientSecret = builder.Configuration["authentication:github:clientSecret"];
        o.CallbackPath = "/signin-github";

        o.Events.OnCreatingTicket = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
            var db = context.HttpContext.RequestServices.GetRequiredService<ChirpDbContext>();

            var email = context.Identity?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = context.Identity?.Name ?? email ?? "GitHubUser";

            if (string.IsNullOrWhiteSpace(email))
                return;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user);
            }

            // Link or create Author
            var author = db.Authors.FirstOrDefault(a => a.Email == email);
            if (author == null)
            {
                author = new Author
                {
                    Name = name,
                    Email = email,
                    ApplicationUserId = user.Id
                };
                db.Authors.Add(author);
            }
            else if (string.IsNullOrEmpty(author.ApplicationUserId))
            {
                author.ApplicationUserId = user.Id;
                db.Authors.Update(author);
            }

            await db.SaveChangesAsync();

            // Directly sign in the user
            await signInManager.SignInAsync(user, isPersistent: false);
        };
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddSession();

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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (builder.Environment.IsEnvironment("Testing"))
    {
        context.Database.EnsureCreated();
    }
    else
    {
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        DbInitializer.SeedDatabase(context);

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
        
        /* Create required users and test users */
        CreateIfMissing("ropf@itu.dk", "LetM31n!", "Helge").GetAwaiter().GetResult();
        CreateIfMissing("adho@itu.dk", "M32Want_Access", "Adrian").GetAwaiter().GetResult();
        CreateIfMissing("anna@itu.dk", "Password123", "Anna").GetAwaiter().GetResult();
        CreateIfMissing("bella@itu.dk", "Password123", "Bella").GetAwaiter().GetResult();
        CreateIfMissing("cheryl@itu.dk", "Password123", "Cheryl").GetAwaiter().GetResult();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapGet("/login/github", async context =>
{
    await context.ChallengeAsync("GitHub", new AuthenticationProperties
    {
        RedirectUri = "/"
    });
});

app.MapRazorPages();

app.Run();

public partial class Program { } // Needed for WebApplicationFactory<T>