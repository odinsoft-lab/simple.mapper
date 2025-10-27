using Simple.AutoMapper.DependencyInjection;
using WebApiSample.Profiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure Simple.AutoMapper with DI pattern
builder.Services.AddSimpleMapper(cfg =>
{
    cfg.AddProfile<ProductProfile>();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
