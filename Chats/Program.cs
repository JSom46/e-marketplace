using Chats.DataAccess;
using Chats.Hubs;
using Chats.Services;
using Configuration;
using FileManager;
using Messenger;
using Microsoft.EntityFrameworkCore;
using Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.Configure<JwtBearerConfiguration>(builder.Configuration.GetSection("JWT"));

builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddTransient<IFileManager, FsFileManager>();

builder.Services.AddJwtAuthentication(builder.Configuration.GetSection("JWT").Get<JwtBearerConfiguration>());

builder.Services.AddDbContext<ChatsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddTransient<IChatsDataAccess, ChatsDataAccess>();

builder.Services.AddHostedService<ChatDeleteMessageConsumer>();

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

app.MapHub<ChatHub>("/chathub");

app.Run();