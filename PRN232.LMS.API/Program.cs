using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;

namespace PRN232.LMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
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
            });

            builder.Services.AddDbContext<PRN232.LMS.Repositories.Entities.Prn232Lab1Context>(
                opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            // Đăng ký Dependency Injection cho Repository và Service
            builder.Services.AddScoped(typeof(PRN232.LMS.Repositories.Interfaces.IGenericRepository<>), typeof(PRN232.LMS.Repositories.Repositories.GenericRepository<>));
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.IStudentService, PRN232.LMS.Services.Business.StudentService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.ICourseService, PRN232.LMS.Services.Business.CourseService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.ISemesterService, PRN232.LMS.Services.Business.SemesterService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.ISubjectService, PRN232.LMS.Services.Business.SubjectService>();
            builder.Services.AddScoped<PRN232.LMS.Services.Interfaces.IEnrollmentService, PRN232.LMS.Services.Business.EnrollmentService>();

            var app = builder.Build();

            await EnsureDatabaseSeededAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Configuration.GetValue<bool>("DisableHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();

            app.UseMiddleware<PRN232.LMS.API.Middleware.ExceptionHandlingMiddleware>();

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

                    if (File.Exists(seedFilePath) && !await context.Semesters.AnyAsync())
                    {
                        foreach (var batch in ReadSqlBatches(seedFilePath))
                        {
                            await context.Database.ExecuteSqlRawAsync(batch);
                        }
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
