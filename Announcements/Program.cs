using Announcements.DataAccess;
using Configuration.Models;
using Configuration.Utils;
using DataAccess;
using FileManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<JwtBearerConfiguration>(builder.Configuration.GetSection("JWT"));

builder.Services.AddJwtAuthentication(builder.Configuration.GetSection("JWT").Get<JwtBearerConfiguration>());

builder.Services.AddTransient<IDataAccess, SqlDataAccess>();

builder.Services.AddTransient<IFileManager, FsFileManager>();

builder.Services.AddTransient<IAnnouncementsDataAccess, AnnouncementsDataAccess>();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();