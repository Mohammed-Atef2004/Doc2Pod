using API.Hubs;
using API.Hubs.RealTime;
using Application;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using EducationalPlatform.Infrastructure.Services.Token;
using Hangfire;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Presistence.Data;
using Infrastructure.Services;
using Infrastructure.Services.PythonService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApi.Middlewares;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Configuration.AddUserSecrets<Program>();

            // ==========================
            // 1. Controllers & Swagger
            // ==========================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate:
                    "[MY-APP] [{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate:
                    "[MY-APP] [{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Doc2Pod API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Please enter your JWT token like this: Bearer {your_token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });


            });

            builder.Services.AddLogging();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ==========================
            // 2. Database & Core Services
            // ==========================
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddHttpClient<IPythonRagService, PythonRagService>(client =>
            {
                var baseUrl = builder.Configuration["PythonService:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl!);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            });

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            // MediatR Setup
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

            // ==========================
            // 3. Identity Setup
            // ==========================
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // ==========================
            // 4. Token Blacklist (MemoryCache)
            // ==========================
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

            // ==========================
            // 5. JWT Authentication
            // ==========================
            var jwtSection = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JwtOptions>(jwtSection);
            var jwtOptions = jwtSection.Get<JwtOptions>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/podcastHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            builder.Services.AddAuthorization();


            //// ==========================
            //// 5. Hangfire
            //// ==========================
            ///
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
            });


            //// ==========================
            //// 6. SignalR
            //// ==========================
            ///
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IPodcastNotificationService).Assembly));
            builder.Services.AddScoped<IPodcastNotificationService, PodcastNotificationService>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SignalRPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .WithOrigins("http://localhost:4200") // رابط مشروع الأنجولار
                          .AllowCredentials(); // دي أهم واحدة عشان SignalR يشتغل
                });
            });
            var app = builder.Build();
            app.UseCors("AllowAngular");

            app.UseCors("SignalRPolicy");
            // ==========================
            // 7. Middleware Pipeline
            // ==========================
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.UseDeveloperExceptionPage();
            }

            app.MapHub<PodcastHub>("/podcastHub");

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseMiddleware<BlacklistMiddleware>();

            app.UseAuthorization();

            app.UseHangfireDashboard();

            app.MapControllers();

            app.Run();
        }
    }
}