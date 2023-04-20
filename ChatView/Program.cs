using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChatView.Data;
using ChatView.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ChatViewContextConnection") ?? throw new InvalidOperationException("Connection string 'ChatViewContextConnection' not found.");
var connectionStringTest = builder.Configuration.GetConnectionString("TestDbContext") ?? throw new InvalidOperationException("Connection string 'TestDbContext' not found.");

//builder.Services.AddDbContext<ChatViewContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ChatViewContext>(options => options.UseSqlServer(connectionStringTest));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ChatViewContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 100; // 100 MB
});


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.Configure<CookieAuthenticationOptions>(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Domain = "yourdomain.com";
    options.Cookie.SameSite = SameSiteMode.Strict;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<ChatViewHub>("/chatviewhub");

app.Run();
