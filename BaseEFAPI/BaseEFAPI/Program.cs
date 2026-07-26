using BaseEFAPI.MVCS.Services.Context;
using BaseEFAPI.MVCS.Services.Registration.Interfaces;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString) == true)
{
    Console.WriteLine("Connection string 'DefaultConnection' not found!");
    Environment.Exit(0);
}

// TODO: ADD DB CONTEXT TO THE CONTAINER
// REGISTER THE REGISTRATION API DBCONTEXT WITH THE CONTAINER
builder.Services.AddDbContext<RegistrationDbContext>(options =>
options.UseSqlServer(
    connectionString: connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    })
);


// LEARN MORE ABOUT CONFIGURING SWAGGER/OPENAPI AT HTTPS://AKA.MS/ASPNETCORE/SWASHBUCKLE
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ADD CONTROLLERS TO THE CONTAINER
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// TODO: ADD SERVICES TO THE CONTAINER
builder.Services.AddScoped<IRegistrationService, RegistrationService>()
    .AddTransient<IUserRepository, UserRepository>();

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
