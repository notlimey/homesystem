using API.Data;
using API.Extensions;
using API.Interfaces.Devices;
using API.Services.Devices;
using API.Services.Job;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.InitializeJobs();

builder.Services.AddSingleton<JobManager>();
builder.Services.AddHostedService<JobManager>(sp => sp.GetRequiredService<JobManager>());

builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });
});


// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // Listen on port 80 on all network interfaces
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder
                .AllowAnyOrigin()   
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddDbContext<HomeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin"); // Use CORS
app.MapControllers();

app.Run();