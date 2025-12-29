using Microsoft.AspNetCore.Mvc;
using NewsPortal.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // (optional) Make sure model binding errors show nicely
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddSingleton<INewsRepository, SqliteNewsRepository>();
builder.Services.AddSingleton<DbInitializer>();

var app = builder.Build();

// Ensure SQLite DB exists + seeded
using (var scope = app.Services.CreateScope())
{
    var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    init.Initialize();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "category",
    pattern: "category/{slug}",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "news",
    pattern: "news/{id:int}",
    defaults: new { controller = "News", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
