using ContactMVP.Data;
using ContactMVP.Models;
using ContactMVP.Services;
using ContactMVP.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
// UseNpgsql must replace default Sqlserver
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Custom Services 
// must use AddScoped to add interfaces
// When we say "builder.Services.AddScoped<IInterface, Class>();
// " we're saying "when someone asks for IInterface, provide them this Class"
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.AddScoped<IContactMVPService, ContactMVPService>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
