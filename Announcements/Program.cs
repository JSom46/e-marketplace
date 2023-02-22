using Announcements.Configuration;
using Announcements.DataAccess;
using Authentication.Utils;
using DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<JwtBearerConfiguration>(builder.Configuration.GetSection("JWT"));

builder.Services.AddJwtAuthentication(builder.Configuration.GetSection("JWT").Get<JwtBearerConfiguration>());

builder.Services.AddSingleton<IDataAccess, SqlDataAccess>();

builder.Services.AddSingleton<IAnnouncementsDataAccess, AnnouncementsDataAccess>();

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
