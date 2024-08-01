using API.Data;
using API.Extensions;
using API.Services.Job;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.InitializeJobs();

builder.Services.AddSingleton<JobManager>();
builder.Services.AddHostedService<JobManager>(sp => sp.GetRequiredService<JobManager>());

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });
});


builder.Services.AddDbContext<HomeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();