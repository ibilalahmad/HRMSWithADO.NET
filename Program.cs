using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); // 5 minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registers configuration so it can be injected anywhere in the app
//builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSingleton<DatabaseHelper>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


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
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
     pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
