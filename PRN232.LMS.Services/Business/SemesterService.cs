using PRN232.LMS.Services.Models.SemesterModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Services.Business;

public class SemesterService : ISemesterService
{
    private readonly IGenericRepository<Semester> _semesterRepo;

    public SemesterService(IGenericRepository<Semester> semesterRepo)
    {
        _semesterRepo = semesterRepo;
    }

    public async Task<SemesterResponseModel?> GetSemesterByIdAsync(int id)
    {
        var semester = await _semesterRepo.GetByIdWithIncludesAsync(
            s => s.SemesterId == id, 
            "Courses");

        if (semester == null)
            return null;

        // Map Entity to Business Model
        var businessModel = new SemesterBusinessModel
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };

        // Map Business Model to Response Model
        return new SemesterResponseModel
        {
            SemesterId = businessModel.SemesterId,
            SemesterName = businessModel.SemesterName,
            StartDate = businessModel.StartDate,
            EndDate = businessModel.EndDate,
            Courses = semester.Courses.Select(c => new SemesterCourseModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName
            }).ToList()
        };
    }

    public async Task<PagedResult<object>> GetSemestersAsync(QueryParameters parameters)
    {
        var result = await _semesterRepo.GetListAsync(
            parameters,
            !string.IsNullOrWhiteSpace(parameters.Search)
                ? s => s.SemesterName.Contains(parameters.Search)
                : null);

        if (!string.IsNullOrWhiteSpace(parameters.Fields))
        {
            return result;
        }

        var mappedItems = result.Items.Cast<Semester>().Select(MapSemester).Cast<object>().ToList();

        return new PagedResult<object>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            Items = mappedItems
        };
    }

    public async Task<SemesterResponseModel> CreateSemesterAsync(SemesterCreateModel model)
    {
        // Query maximum SemesterId dynamically since DB has no Identity column
        var semesters = await _semesterRepo.GetAllAsync();
        var nextId = semesters.Any() ? semesters.Max(s => s.SemesterId) + 1 : 1;

        var semester = new Semester
        {
            SemesterId = nextId,
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        await _semesterRepo.AddAsync(semester);
        return (await GetSemesterByIdAsync(semester.SemesterId))!;
    }

    public async Task<SemesterResponseModel?> UpdateSemesterAsync(int id, SemesterUpdateModel model)
    {
        var semester = await _semesterRepo.GetByIdAsync(id);

        if (semester == null)
        {
            throw new ArgumentException($"Semester with ID {id} does not exist.");
        }

        semester.SemesterName = model.SemesterName;
        semester.StartDate = model.StartDate;
        semester.EndDate = model.EndDate;

        await _semesterRepo.UpdateAsync(semester);
        return await GetSemesterByIdAsync(id);
    }

    public async Task<bool> DeleteSemesterAsync(int id)
    {
        var semester = await _semesterRepo.GetByIdAsync(id);

        if (semester == null)
        {
            return false;
        }

        await _semesterRepo.DeleteAsync(id);
        return true;
    }

    private static SemesterResponseModel MapSemester(Semester semester)
    {
        // Map to Business Model first
        var businessModel = new SemesterBusinessModel
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };

        // Map Business Model to Response Model
        return new SemesterResponseModel
        {
            SemesterId = businessModel.SemesterId,
            SemesterName = businessModel.SemesterName,
            StartDate = businessModel.StartDate,
            EndDate = businessModel.EndDate,
            Courses = semester.Courses.Select(c => new SemesterCourseModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName
            }).ToList()
        };
    }
}
