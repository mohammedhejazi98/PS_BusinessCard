using Microsoft.EntityFrameworkCore;

using PS_BusinessCard.Data;
using PS_BusinessCard.IService;
using PS_BusinessCard.Repositories;
using PS_BusinessCard.Services;

using System.Reflection; // Required for XML comments

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Define Swagger documentation for the API
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Business Card API",
        Version = "v1",
        Description = "API for managing business cards, including QR code, Excel, and XML functionalities."
    });

    // Include XML comments (Enable this after you enable XML documentation in the project settings)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath); // Reads the XML comments for Swagger documentation
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

#region Register our context

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Register our repositories and services

builder.Services.AddScoped<IBusinessCardRepository, BusinessCardRepository>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IXmlService, XmlService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

#endregion

var app = builder.Build();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Business Card API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at the root URL
        c.InjectStylesheet("/css/swagger.css");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
