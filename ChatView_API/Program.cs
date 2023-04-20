using ChatView_API.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//var connectionString = builder.Configuration.GetConnectionString("ChatViewDbContext");
var connectionStringTest = builder.Configuration.GetConnectionString("ChatViewDbContext");

//builder.Services.AddDbContext<ChatViewDbContext>(x => x.UseSqlServer(connectionString));
//builder.Services.AddDbContext<ChatViewDbContext>(x => x.UseSqlServer(connectionStringTest));
builder.Services.AddDbContext<ChatViewDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:TestDbContext"]));

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
