using System.Threading.Tasks;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Services.Models.SemesterModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<SemesterResponseModel?> GetSemesterByIdAsync(int id);
    Task<PagedResult<object>> GetSemestersAsync(QueryParameters parameters);
    Task<SemesterResponseModel> CreateSemesterAsync(SemesterCreateModel model);
    Task<SemesterResponseModel?> UpdateSemesterAsync(int id, SemesterUpdateModel model);
    Task<bool> DeleteSemesterAsync(int id);
}