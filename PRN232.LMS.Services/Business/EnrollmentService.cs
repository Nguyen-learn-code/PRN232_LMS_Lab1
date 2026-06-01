using PRN232.LMS.Services.Models.EnrollmentModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Services.Business;

public class EnrollmentService : IEnrollmentService
{
    private readonly IGenericRepository<Enrollment> _enrollmentRepo;
    private readonly IGenericRepository<Student> _studentRepo;
    private readonly IGenericRepository<Course> _courseRepo;

    public EnrollmentService(
        IGenericRepository<Enrollment> enrollmentRepo,
        IGenericRepository<Student> studentRepo,
        IGenericRepository<Course> courseRepo)
    {
        _enrollmentRepo = enrollmentRepo;
        _studentRepo = studentRepo;
        _courseRepo = courseRepo;
    }

    public async Task<EnrollmentResponseModel?> GetEnrollmentByIdAsync(int id)
    {
        var enrollment = await _enrollmentRepo.GetByIdWithIncludesAsync(
            e => e.EnrollmentId == id,
            "Student", "Course");

        if (enrollment == null)
        {
            return null;
        }

        return MapEnrollment(enrollment);
    }

    public async Task<IEnumerable<EnrollmentResponseModel>?> GetEnrollmentsByCourseAsync(int courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);

        if (course == null)
        {
            return null;
        }

        return course.Enrollments.Select(MapEnrollment).ToList();
    }

    public async Task<IEnumerable<EnrollmentResponseModel>?> GetEnrollmentsByStudentAsync(int studentId)
    {
        var student = await _studentRepo.GetByIdAsync(studentId);

        if (student == null)
        {
            return null;
        }

        return student.Enrollments.Select(MapEnrollment).ToList();
    }

    public async Task<PagedResult<object>> GetEnrollmentsAsync(QueryParameters parameters)
    {
        var result = await _enrollmentRepo.GetListAsync(
            parameters,
            !string.IsNullOrWhiteSpace(parameters.Search)
                ? e => e.Status.Contains(parameters.Search)
                : null);

        if (!string.IsNullOrWhiteSpace(parameters.Fields))
        {
            return result;
        }

        var mappedItems = result.Items.Cast<Enrollment>().Select(MapEnrollment).Cast<object>().ToList();

        return new PagedResult<object>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            Items = mappedItems
        };
    }

    public async Task<EnrollmentResponseModel> CreateEnrollmentAsync(EnrollmentCreateModel model)
    {
        await EnsureStudentAndCourseExistAsync(model.StudentId, model.CourseId);

        // Query maximum EnrollmentId dynamically since DB has no Identity column
        var enrollments = await _enrollmentRepo.GetAllAsync();
        var nextId = enrollments.Any() ? enrollments.Max(e => e.EnrollmentId) + 1 : 1;

        var enrollment = new Enrollment
        {
            EnrollmentId = nextId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        };

        await _enrollmentRepo.AddAsync(enrollment);
        return (await GetEnrollmentByIdAsync(enrollment.EnrollmentId))!;
    }

    public async Task<EnrollmentResponseModel?> UpdateEnrollmentAsync(int id, EnrollmentUpdateModel model)
    {
        var enrollment = await _enrollmentRepo.GetByIdAsync(id);

        if (enrollment == null)
        {
            throw new ArgumentException($"Enrollment with ID {id} does not exist.");
        }

        await EnsureStudentAndCourseExistAsync(model.StudentId, model.CourseId);

        enrollment.StudentId = model.StudentId;
        enrollment.CourseId = model.CourseId;
        enrollment.EnrollDate = model.EnrollDate;
        enrollment.Status = model.Status;

        await _enrollmentRepo.UpdateAsync(enrollment);
        return await GetEnrollmentByIdAsync(id);
    }

    public async Task<bool> DeleteEnrollmentAsync(int id)
    {
        var enrollment = await _enrollmentRepo.GetByIdAsync(id);

        if (enrollment == null)
        {
            return false;
        }

        await _enrollmentRepo.DeleteAsync(id);
        return true;
    }

    private async Task EnsureStudentAndCourseExistAsync(int studentId, int courseId)
    {
        if (await _studentRepo.GetByIdAsync(studentId) == null)
        {
            throw new ArgumentException($"Student with ID {studentId} does not exist.");
        }

        if (await _courseRepo.GetByIdAsync(courseId) == null)
        {
            throw new ArgumentException($"Course with ID {courseId} does not exist.");
        }
    }

    private static EnrollmentResponseModel MapEnrollment(Enrollment enrollment)
    {
        // Map to Business Model first
        var businessModel = new EnrollmentBusinessModel
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };

        // Map Business Model to Response Model
        return new EnrollmentResponseModel
        {
            EnrollmentId = businessModel.EnrollmentId,
            StudentId = businessModel.StudentId,
            StudentName = enrollment.Student?.FullName,
            CourseId = businessModel.CourseId,
            CourseName = enrollment.Course?.CourseName,
            EnrollDate = businessModel.EnrollDate,
            Status = businessModel.Status
        };
    }
}
