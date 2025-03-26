using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Services;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RabbitMQService>();
IConfiguration Configuration = builder.Configuration;
try
{
    string connectionString = Configuration.GetConnectionString("CSharpDatabase")
    ?? throw new Exception("Connection string not found");

    builder.Services.AddDbContext<DBContext>(options =>
        options.UseNpgsql(connectionString));
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

var app = builder.Build();

app.MapDefaultEndpoints();


app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
