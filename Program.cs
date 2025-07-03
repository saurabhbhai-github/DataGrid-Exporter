using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PF_TRANSFER_IN__REGISTER_FORMAT.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<PFTransferRepository>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<PFTransferRepository>>();
    return new PFTransferRepository(configuration, logger);
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PFTransfer}/{action=Index}/{id?}");

app.Run();
