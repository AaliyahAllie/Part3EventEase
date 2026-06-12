using Part3EventEase.Data;
using Part3EventEase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<EventEaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession();
builder.Services.AddScoped<BlobService>();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection:blobServiceUri"]!).WithName("StorageConnection");
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnection:queueServiceUri"]!).WithName("StorageConnection");
    clientBuilder.AddTableServiceClient(builder.Configuration["StorageConnection:tableServiceUri"]!).WithName("StorageConnection");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
