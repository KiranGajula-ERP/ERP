using AuditFlow.Web.Data;
using AuditFlow.Web.Models;
using AuditFlow.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProjectScopeService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
var connectionString = builder.Configuration.GetConnectionString("AuditFlow")
    ?? "Host=localhost;Port=5432;Database=auditflow;Username=auditflow;Password=ChangeThisBeforeUse";
builder.Services.AddDbContext<AuditFlowDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.Cookie.Name = "AuditFlow.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProjectAdmin", p => p.RequireRole(AppRoles.SystemAdministrator, AppRoles.Management));
    options.AddPolicy("InventoryPost", p => p.RequireRole(AppRoles.SystemAdministrator, AppRoles.Management, AppRoles.PurchaseTeam, AppRoles.StoreInCharge));
    options.AddPolicy("Purchase", p => p.RequireRole(AppRoles.SystemAdministrator, AppRoles.Management, AppRoles.PurchaseTeam));
    options.AddPolicy("Billing", p => p.RequireRole(AppRoles.SystemAdministrator, AppRoles.Management, AppRoles.BillingEngineer));
});
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (app.Configuration.GetValue<bool>("Web:EnableHttpsRedirection")) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Initial beta bootstrap. Once the database exists, schema changes must be handled with migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuditFlowDbContext>();
    var attempts = 0;
    while (true)
    {
        try { await db.Database.EnsureCreatedAsync(); break; }
        catch when (++attempts < 20) { await Task.Delay(TimeSpan.FromSeconds(2)); }
    }
    await BootstrapData.InitializeAsync(scope.ServiceProvider);
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", application = "AuditFlow", utc = DateTime.UtcNow }));
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
