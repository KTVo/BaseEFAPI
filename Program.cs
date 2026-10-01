using BaseEFAPI.MVCS.Services.Authentication.Implementations;
using BaseEFAPI.MVCS.Services.Authentication.Interfaces;
using BaseEFAPI.MVCS.Services.Context;
using BaseEFAPI.MVCS.Services.Registration.Interfaces;
using BaseEFAPI.MVCS.Services.SignIn.Implementations;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ADD SERVICES TO THE CONTAINER
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddLogging(builder => builder.AddConsole());

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString) == true)
{
    Console.WriteLine("Connection string 'DefaultConnection' not found!");
    Environment.Exit(0);
}

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
builder.Services
    .AddScoped<IRegistrationService, RegistrationService>()
    .AddScoped<ISignInService, SignInService>()
    .AddTransient<IJwtTokenService, JwtTokenService>()
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
