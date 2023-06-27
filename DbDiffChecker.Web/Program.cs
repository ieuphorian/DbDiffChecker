using DbDiffChecker.Service.DbDesign;
using DbDiffChecker.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.
    AddJsonFile("appsettings.json", false, true).
    AddJsonFile("appsettings.Development.json", true, true);
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IDbDesignService, DbDesignService>();

builder.Services.AddScoped<IsAppInstalledActionFilter>();

builder.Services.AddMvcCore(f =>
{
    f.Filters.Add(typeof(IsAppInstalledActionFilter));
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

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();