using Microsoft.AspNetCore.WebSockets;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOcelot();

builder.Configuration.AddJsonFile("ocelot.json", false, true);

var app = builder.Build();

app.UseRouting();

app.MapControllers();

app.UseWebSockets();

app.UseOcelot().Wait();

app.Run();