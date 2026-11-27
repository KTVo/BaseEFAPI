using BaseEFAPI.MVCS.Services.Context;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder => builder.AddConsole());

builder.Services.AddDbContext<NexusDbContext>(options =>
{
    string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection before using database services.");

    string provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
    switch (provider.ToLowerInvariant())
    {
        case "sqlserver": options.UseSqlServer(connection); break;
        case "postgresql": options.UseNpgsql(connection); break;
        default: throw new InvalidOperationException("Database:Provider must be SqlServer or PostgreSql.");
    }
});
builder.Services.AddScoped<NexusRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost")
              .AllowAnyHeader()
              .AllowAnyMethod());
});


// LEARN MORE ABOUT CONFIGURING SWAGGER/OPENAPI AT HTTPS://AKA.MS/ASPNETCORE/SWASHBUCKLE
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ADD CONTROLLERS TO THE CONTAINER
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("Development environment detected. Enabling CORS for all origins.");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
                       bldr => bldr.AllowAnyMethod()
                                   .AllowAnyHeader()
                                   .AllowAnyOrigin());
    });

}
else
{
    builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowAppServices",
                                 bldr => bldr
                                 .AllowAnyMethod()
                                 .AllowAnyHeader()
                                 .WithOrigins());
   });
}

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // ENABLE MIDDLEWARE TO SERVE GENERATED SWAGGER AS JSON ENDPOINT
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        // options.RoutePrefix = string.Empty;
    });

    

    // ENABLE CORS POLICY
    app.UseCors("AllowAll");
}
else
{   
    app.UseCors("AllowAppServices");
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
