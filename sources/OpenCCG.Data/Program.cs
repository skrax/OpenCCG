using Microsoft.EntityFrameworkCore;
using OpenCCG.Data;
using OpenCCG.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
var connectionString = connectionStrings.GetValue<string>("Database");


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapSwagger();

app.MigrateDatabase();

app.Run();
