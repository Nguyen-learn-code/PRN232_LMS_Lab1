using System.Threading.Tasks;
using PRN232.LMS.Repositories.Models.QueryModels;
using PRN232.LMS.Services.Models.StudentModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<StudentResponseModel?> GetStudentByIdAsync(int id);
    Task<PagedResult<object>> GetStudentsAsync(QueryParameters parameters);
    Task<StudentResponseModel> CreateStudentAsync(CreateStudentRequest model);
    Task<StudentResponseModel?> UpdateStudentAsync(int id, UpdateStudentRequest model);
    Task<bool> DeleteStudentAsync(int id);
}