using System.Threading.Tasks;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Services.Models.SubjectModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<SubjectResponseModel?> GetSubjectByIdAsync(int id);
    Task<PagedResult<object>> GetSubjectsAsync(QueryParameters parameters);
    Task<SubjectResponseModel> CreateSubjectAsync(SubjectCreateModel model);
    Task<SubjectResponseModel?> UpdateSubjectAsync(int id, SubjectUpdateModel model);
    Task<bool> DeleteSubjectAsync(int id);
}