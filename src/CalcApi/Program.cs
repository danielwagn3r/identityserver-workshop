using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the container.
    builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration));

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = "https://localhost:5001";
            options.Audience = "urn:calcapi";
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            options.TokenValidationParameters.ValidateLifetime = true;
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("calc:double", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "calc:double");
        });

        options.AddPolicy("calc:square", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "calc:square");
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.UseAuthorization();

    app.MapControllers().RequireAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
