using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Services.Models.StudentModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Services.Business;

public class StudentService : IStudentService
{
    private readonly IGenericRepository<Student> _studentRepo;

    public StudentService(IGenericRepository<Student> studentRepo)
    {
        _studentRepo = studentRepo;
    }

    public async Task<StudentResponseModel?> GetStudentByIdAsync(int id)
    {
        // 1. Fetch related data
        var student = await _studentRepo.GetByIdWithIncludesAsync(
            s => s.StudentId == id, 
            "Enrollments", "Enrollments.Course"); // Include Course via nested path

        if (student == null)
            return null;

        // 2. Map Entity to Business Model
        var businessModel = new StudentBusinessModel
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth
        };

        // 3. Map Business Model to Response Model
        return new StudentResponseModel
        {
            StudentId = businessModel.StudentId,
            FullName = businessModel.FullName,
            Email = businessModel.Email,
            DateOfBirth = businessModel.DateOfBirth,
            //Enrollments = student.Enrollments.Select(e => new StudentEnrollmentModel
            //{
            //    EnrollmentId = e.EnrollmentId,
            //    CourseId = e.CourseId,
            //    CourseName = e.Course?.CourseName,
            //    Status = e.Status,
            //    EnrollDate = e.EnrollDate
            //}).ToList()
        };
    }

    public async Task<PagedResult<object>> GetStudentsAsync(QueryParameters parameters)
    {
        var result = await _studentRepo.GetListAsync(
            parameters,
            !string.IsNullOrWhiteSpace(parameters.Search)
                ? s => s.FullName.Contains(parameters.Search) || s.Email.Contains(parameters.Search)
                : null);

        if (!string.IsNullOrWhiteSpace(parameters.Fields))
        {
            return result;
        }

        var mappedItems = result.Items.Cast<Student>().Select(MapStudent).Cast<object>().ToList();

        return new PagedResult<object>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            Items = mappedItems
        };
    }

    public async Task<StudentResponseModel> CreateStudentAsync(CreateStudentRequest model)
    {
        // Query maximum StudentId dynamically since DB has no Identity column
        var students = await _studentRepo.GetAllAsync();
        var nextId = students.Any() ? students.Max(s => s.StudentId) + 1 : 1;

        var student = new Student
        {
            StudentId = nextId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };

        await _studentRepo.AddAsync(student);
        return (await GetStudentByIdAsync(student.StudentId))!;
    }

    public async Task<StudentResponseModel?> UpdateStudentAsync(int id, UpdateStudentRequest model)
    {
        var student = await _studentRepo.GetByIdAsync(id);

        if (student == null)
        {
            throw new ArgumentException($"Student with ID {id} does not exist.");
        }

        student.FullName = model.FullName;
        student.Email = model.Email;
        student.DateOfBirth = model.DateOfBirth;

        await _studentRepo.UpdateAsync(student);
        return await GetStudentByIdAsync(id);
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _studentRepo.GetByIdAsync(id);

        if (student == null)
        {
            return false;
        }

        await _studentRepo.DeleteAsync(id);
        return true;
    }

    private static StudentResponseModel MapStudent(Student student)
    {
        // Map to Business Model first
        var businessModel = new StudentBusinessModel
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth
        };

        // Map Business Model to Response Model
        return new StudentResponseModel
        {
            StudentId = businessModel.StudentId,
            FullName = businessModel.FullName,
            Email = businessModel.Email,
            DateOfBirth = businessModel.DateOfBirth,
            Enrollments = student.Enrollments != null ? student.Enrollments.Select(e => new StudentEnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                CourseName = e.Course?.CourseName,
                Status = e.Status,
                EnrollDate = e.EnrollDate
            }).ToList() : null
        };
    }
}
