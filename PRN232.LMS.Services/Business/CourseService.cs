using PRN232.LMS.Services.Models.CourseModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Services.Business;

public class CourseService : ICourseService
{
    private readonly IGenericRepository<Course> _courseRepo;
    private readonly IGenericRepository<Semester> _semesterRepo;

    public CourseService(IGenericRepository<Course> courseRepo, IGenericRepository<Semester> semesterRepo)
    {
        _courseRepo = courseRepo;
        _semesterRepo = semesterRepo;
    }

    public async Task<CourseResponseModel?> GetCourseByIdAsync(int id)
    {
        var course = await _courseRepo.GetByIdWithIncludesAsync(
            c => c.CourseId == id, 
            "Semester", "Enrollments.Student");

        if (course == null)
            return null;

        // Map Entity to Business Model
        var businessModel = new CourseBusinessModel
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId
        };

        // Map Business Model to Response Model
        return new CourseResponseModel
        {
            CourseId = businessModel.CourseId,
            CourseName = businessModel.CourseName,
            SemesterId = businessModel.SemesterId,
            SemesterName = course.Semester?.SemesterName,
            //Enrollments = course.Enrollments.Select(e => new CourseEnrollmentModel
            //{
            //    EnrollmentId = e.EnrollmentId,
            //    StudentId = e.StudentId,
            //    StudentName = e.Student?.FullName,
            //    Status = e.Status,
            //    EnrollDate = e.EnrollDate
            //}).ToList()
        };
    }

    public async Task<PagedResult<object>> GetCoursesAsync(QueryParameters parameters)
    {
        var result = await _courseRepo.GetListAsync(
            parameters,
            !string.IsNullOrWhiteSpace(parameters.Search)
                ? c => c.CourseName.Contains(parameters.Search)
                : null);

        if (!string.IsNullOrWhiteSpace(parameters.Fields))
        {
            return result;
        }

        var mappedItems = result.Items.Cast<Course>().Select(MapCourse).Cast<object>().ToList();

        return new PagedResult<object>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            Items = mappedItems
        };
    }

    public async Task<CourseResponseModel> CreateCourseAsync(CourseCreateModel model)
    {
        if (await _semesterRepo.GetByIdAsync(model.SemesterId) == null)
        {
            throw new ArgumentException($"Semester with ID {model.SemesterId} does not exist.");
        }

        // Query maximum CourseId dynamically since DB has no Identity column
        var courses = await _courseRepo.GetAllAsync();
        var nextId = courses.Any() ? courses.Max(c => c.CourseId) + 1 : 1;

        var course = new Course
        {
            CourseId = nextId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId
        };

        await _courseRepo.AddAsync(course);
        return (await GetCourseByIdAsync(course.CourseId))!;
    }

    public async Task<CourseResponseModel?> UpdateCourseAsync(int id, CourseUpdateModel model)
    {
        var course = await _courseRepo.GetByIdAsync(id);

        if (course == null)
        {
            return null;
        }

        if (await _semesterRepo.GetByIdAsync(model.SemesterId) == null)
        {
            throw new ArgumentException($"Semester with ID {model.SemesterId} does not exist.");
        }

        course.CourseName = model.CourseName;
        course.SemesterId = model.SemesterId;

        await _courseRepo.UpdateAsync(course);
        return await GetCourseByIdAsync(id);
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        var course = await _courseRepo.GetByIdAsync(id);

        if (course == null)
        {
            return false;
        }

        await _courseRepo.DeleteAsync(id);
        return true;
    }

    private static CourseResponseModel MapCourse(Course course)
    {
        // Map to Business Model first
        var businessModel = new CourseBusinessModel
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId
        };

        // Map Business Model to Response Model
        return new CourseResponseModel
        {
            CourseId = businessModel.CourseId,
            CourseName = businessModel.CourseName,
            SemesterId = businessModel.SemesterId,
            SemesterName = course.Semester?.SemesterName,
            //Enrollments = course.Enrollments.Select(e => new CourseEnrollmentModel
            //{
            //    EnrollmentId = e.EnrollmentId,
            //    StudentId = e.StudentId,
            //    StudentName = e.Student?.FullName,
            //    Status = e.Status,
            //    EnrollDate = e.EnrollDate
            //}).ToList()
        };
    }
}
