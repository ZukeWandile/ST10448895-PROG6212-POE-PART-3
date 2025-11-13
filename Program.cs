using ST10448895_CMCS_PROG.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MySQL EF Core configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36)), // or your MySQL version
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));


builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSession();
app.UseRouting();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.Run();
