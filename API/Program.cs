using API.Extensions;
using API.Services.Job;

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();