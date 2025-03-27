using FileManagerAPI.Services;
using FileStorageApi.Context;
using FileStorageApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Gestión de archivos", Version = "v1" });

});


// Leer la configuración de URLs desde appsettings.json
var urls = builder.Configuration["Urls"];

builder.WebHost.ConfigureKestrel(options =>
{
    if (!string.IsNullOrEmpty(urls))
    {
        options.ListenAnyIP(int.Parse(urls.Split(':')[2])); // Configura el puerto
    }
});

// Agregar conexión a la base de datos
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("filemanager_db")));

builder.Services.AddScoped<FileRecordService>();
builder.Services.AddScoped<SupportedFileService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
