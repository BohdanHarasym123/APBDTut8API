using Tutorial8.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ITripService, TripService>();

var app = builder.Build();

app.MapControllers();

app.Run();
