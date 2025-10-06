using Chirp.Application.Interfaces;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChirpDbContext>(options => options.UseSqlite(connectionString ?? "Data Source=Chirp.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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

app.MapRazorPages();

app.Run();

public partial class Program { } // Needed for WebApplicationFactory<T>
