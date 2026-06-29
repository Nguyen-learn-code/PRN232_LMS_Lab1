using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace PRN232.LMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
            })
            .AddXmlSerializerFormatters()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

            // Configure API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }

                // Add Swagger JWT Authorization Button
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? "AntigravitySuperSecretDefaultKey1234567890!!";
            var issuer = jwtSettings["Issuer"] ?? "LmsServer";
            var audience = jwtSettings["Audience"] ?? "LmsClient";

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
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddDbContext<PRN232.LMS.Repositories.Entities.Prn232Lab1Context>(
                opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            // Register Dependency Injection for Repositories and Services
            builder.Services.AddScoped(typeof(PRN232.LMS.Repositories.Interfaces.IGenericRepository<>), typeof(PRN232.LMS.Repositories.Repositories.GenericRepository<>));
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.IStudentService, PRN232.LMS.Services.Business.StudentService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.ICourseService, PRN232.LMS.Services.Business.CourseService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.ISemesterService, PRN232.LMS.Services.Business.SemesterService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.ISubjectService, PRN232.LMS.Services.Business.SubjectService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.IEnrollmentService, PRN232.LMS.Services.Business.EnrollmentService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.IAuthService, PRN232.LMS.Services.Business.AuthService>();

            // Register FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(PRN232.LMS.Services.Models.StudentModels.CreateStudentRequest).Assembly);
            builder.Services.AddFluentValidationAutoValidation();

            var app = builder.Build();

            await EnsureDatabaseSeededAsync(app);

            // Configure HTTP pipeline
            app.UseMiddleware<PRN232.LMS.API.Middleware.ExceptionHandlingMiddleware>();
            app.UseMiddleware<PRN232.LMS.API.Middleware.LoggingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var descriptions = app.DescribeApiVersions();
                    foreach (var description in descriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
            }

            if (!app.Configuration.GetValue<bool>("DisableHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static async Task EnsureDatabaseSeededAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PRN232.LMS.Repositories.Entities.Prn232Lab1Context>();
            var seedFilePath = Path.Combine(AppContext.BaseDirectory, "dbseed.sql");

            for (var attempt = 1; attempt <= 10; attempt++)
            {
                try
                {
                    await context.Database.EnsureCreatedAsync();

                    // Create User and RefreshToken tables if they don't exist
                    await context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='User' and xtype='U')
                        BEGIN
                            CREATE TABLE [User] (
                                UserId INT IDENTITY(1,1) PRIMARY KEY,
                                Username VARCHAR(50) NOT NULL UNIQUE,
                                PasswordHash VARCHAR(255) NOT NULL,
                                Role VARCHAR(20) NOT NULL
                            );
                        END
                    ");

                    await context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RefreshToken' and xtype='U')
                        BEGIN
                            CREATE TABLE RefreshToken (
                                TokenId INT IDENTITY(1,1) PRIMARY KEY,
                                Token VARCHAR(255) NOT NULL,
                                UserId INT NOT NULL,
                                ExpiryDate DATETIME NOT NULL,
                                IsRevoked BIT NOT NULL DEFAULT 0,
                                CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                                CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE
                            );
                        END
                    ");

                    if (File.Exists(seedFilePath) && !await context.Semesters.AnyAsync())
                    {
                        foreach (var batch in ReadSqlBatches(seedFilePath))
                        {
                            await context.Database.ExecuteSqlRawAsync(batch);
                        }
                    }

                    // Seed default users if empty
                    if (!await context.Users.AnyAsync())
                    {
                        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                        var studentPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

                        context.Users.AddRange(
                            new PRN232.LMS.Repositories.Entities.User
                            {
                                Username = "admin",
                                PasswordHash = adminPasswordHash,
                                Role = "Admin"
                            },
                            new PRN232.LMS.Repositories.Entities.User
                            {
                                Username = "student",
                                PasswordHash = studentPasswordHash,
                                Role = "Student"
                            }
                        );
                        await context.SaveChangesAsync();
                    }

                    return;
                }
                catch when (attempt < 10)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }
        }

        private static IEnumerable<string> ReadSqlBatches(string filePath)
        {
            var batch = new StringBuilder();

            foreach (var line in File.ReadLines(filePath))
            {
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    var sql = batch.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(sql))
                    {
                        yield return sql;
                    }

                    batch.Clear();
                    continue;
                }

                batch.AppendLine(line);
            }

            var finalBatch = batch.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(finalBatch))
            {
                yield return finalBatch;
            }
        }
    }
}
