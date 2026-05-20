using PRN232.LMS.Services.Models.SubjectModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Services.Business;

public class SubjectService : ISubjectService
{
    private readonly IGenericRepository<Subject> _subjectRepo;

    public SubjectService(IGenericRepository<Subject> subjectRepo)
    {
        _subjectRepo = subjectRepo;
    }

    public async Task<SubjectResponseModel?> GetSubjectByIdAsync(int id)
    {
        var subject = await _subjectRepo.GetByIdAsync(id);

        if (subject == null)
            return null;

        // Map Entity to Business Model
        var businessModel = new SubjectBusinessModel
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };

        // Map Business Model to Response Model
        return new SubjectResponseModel
        {
            SubjectId = businessModel.SubjectId,
            SubjectCode = businessModel.SubjectCode,
            SubjectName = businessModel.SubjectName,
            Credit = businessModel.Credit
        };
    }

    public async Task<PagedResult<object>> GetSubjectsAsync(QueryParameters parameters)
    {
        var result = await _subjectRepo.GetListAsync(
            parameters,
            !string.IsNullOrWhiteSpace(parameters.Search)
                ? s => s.SubjectName.Contains(parameters.Search) || s.SubjectCode.Contains(parameters.Search)
                : null);

        if (!string.IsNullOrWhiteSpace(parameters.Fields))
        {
            return result;
        }

        var mappedItems = result.Items.Cast<Subject>().Select(MapSubject).Cast<object>().ToList();

        return new PagedResult<object>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            Items = mappedItems
        };
    }

    public async Task<SubjectResponseModel> CreateSubjectAsync(SubjectCreateModel model)
    {
        // Query maximum SubjectId dynamically since DB has no Identity column
        var subjects = await _subjectRepo.GetAllAsync();
        var nextId = subjects.Any() ? subjects.Max(s => s.SubjectId) + 1 : 1;

        var subject = new Subject
        {
            SubjectId = nextId,
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };

        await _subjectRepo.AddAsync(subject);
        return (await GetSubjectByIdAsync(subject.SubjectId))!;
    }

    public async Task<SubjectResponseModel?> UpdateSubjectAsync(int id, SubjectUpdateModel model)
    {
        var subject = await _subjectRepo.GetByIdAsync(id);

        if (subject == null)
        {
            throw new ArgumentException($"Subject with ID {id} does not exist.");
        }

        subject.SubjectCode = model.SubjectCode;
        subject.SubjectName = model.SubjectName;
        subject.Credit = model.Credit;

        await _subjectRepo.UpdateAsync(subject);
        return await GetSubjectByIdAsync(id);
    }

    public async Task<bool> DeleteSubjectAsync(int id)
    {
        var subject = await _subjectRepo.GetByIdAsync(id);

        if (subject == null)
        {
            return false;
        }

        await _subjectRepo.DeleteAsync(id);
        return true;
    }

    private static SubjectResponseModel MapSubject(Subject subject)
    {
        // Map to Business Model first
        var businessModel = new SubjectBusinessModel
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };

        // Map Business Model to Response Model
        return new SubjectResponseModel
        {
            SubjectId = businessModel.SubjectId,
            SubjectCode = businessModel.SubjectCode,
            SubjectName = businessModel.SubjectName,
            Credit = businessModel.Credit
        };
    }
}