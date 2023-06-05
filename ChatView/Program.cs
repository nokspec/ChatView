using ChatView.Data;
using ChatView.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ChatViewContextConnection") ?? throw new InvalidOperationException("Connection string 'ChatViewContextConnection' not found.");
var connectionStringTest = builder.Configuration.GetConnectionString("AcceptanceDbConnection") ?? throw new InvalidOperationException("Connection string 'AcceptanceDbConnection' not found.");

//builder.Services.AddDbContext<ChatViewContext>(options => options.UseSqlServer(connectionString));
//builder.Services.AddDbContext<ChatViewContext>(options => options.UseSqlServer(connectionStringTest));
builder.Services.AddDbContext<ChatViewContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:AcceptanceDbConnection"]));

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
    options.Secure = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
	context.Response.Headers.Add("Content-Security-Policy", "media-src 'self' data: mp4: *;");
	context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<ChatViewHub>("/chatviewhub");

app.Run();
