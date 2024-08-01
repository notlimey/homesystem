using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;

var bld = WebApplication.CreateBuilder();
bld.Services
   .AddFastEndpoints()
   .SwaggerDocument(); //define a swagger document

var app = bld.Build();

app.UseFastEndpoints()
   .UseSwaggerGen(); //add this
app.Run();