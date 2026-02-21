using FluentValidation;
using FluentValidation.AspNetCore;
using KindoHub.Api.Middleware;
using KindoHub.Core;
using KindoHub.Core.Interfaces;
using KindoHub.Data;
using KindoHub.Data.Repositories;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

// ========================================
// CONFIGURACIÓN DE SERILOG (PASO 1)
// ========================================
// Logger temporal para el bootstrap
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting KindoHub API...");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================
    // CONFIGURAR SERILOG DESDE APPSETTINGS (PASO 2)
    // ========================================
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    );

    // Add services to the container.

    builder.Services.AddControllers();

    // Registrar FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Pega 'Bearer TU_TOKEN_AQUI caballero'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddHttpContextAccessor();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

    // DbContext propio
    builder.Services.AddScoped<IDbConnectionFactory>(sp => new SqlConnectionFactory(builder.Configuration));
    builder.Services.AddScoped<IDbConnectionFactoryFactory, DbConnectionFactoryFactory>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IFormaPagoRepository, FormaPagoRepository>(); 
    builder.Services.AddScoped<IEstadoAsociadoRepository, EstadoAsociadoRepository>();
    builder.Services.AddScoped<IFamiliaRepository, FamiliaRepository>();
    builder.Services.AddScoped<IAnotacionRepository, AnotacionRepository>();
    builder.Services.AddScoped<ICursoRepository, CursoRepository>();
    builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, JwtTokenService>();
    builder.Services.AddScoped<IUsuarioService, UserService>();
    builder.Services.AddScoped<IFormaPagoService, FormaPagoService>();
    builder.Services.AddScoped<IEstadoAsociadoService, EstadoAsociadoService>();
    builder.Services.AddScoped<IFamiliaService, FamiliaService>();
    builder.Services.AddScoped<IAnotacionService, AnotacionService>();
    builder.Services.AddScoped<ICursoService, CursoService>();
    builder.Services.AddScoped<IAlumnoService, AlumnoService>();

    // Configuración JWT
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                NameClaimType = "sub",  // Asegura que el claim "sub" se use como nombre de usuario
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Gestion_Familias", policy => policy.RequireClaim("permission", "Gestion_Familias"));
        options.AddPolicy("Consulta_Familias", policy => policy.RequireClaim("permission", "Consulta_Familias"));
        options.AddPolicy("Gestion_Gastos", policy => policy.RequireClaim("permission", "Gestion_Gastos"));
        options.AddPolicy("Consulta_Gastos", policy => policy.RequireClaim("permission", "Consulta_Gastos"));
    });


    var app = builder.Build();

    // ========================================
    // MIDDLEWARE DE SERILOG (PASO 3)
    // ========================================
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());

            // Enriquecer con información del usuario autenticado
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                diagnosticContext.Set("Username", httpContext.User.Identity.Name);
            }
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseSerilogEnrichment();  // ← Middleware para propagar contexto a todos los logs
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("✅ KindoHub API started successfully on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ KindoHub API failed to start");
    throw;
}
finally
{
    Log.Information("🛑 Shutting down KindoHub API");
    Log.CloseAndFlush();
}
