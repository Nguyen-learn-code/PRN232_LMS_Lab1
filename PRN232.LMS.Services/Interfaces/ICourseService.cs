using System.Threading.Tasks;
using PRN232.LMS.Services.Models.CourseModels;
using PRN232.LMS.Repositories.Models.QueryModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<CourseResponseModel?> GetCourseByIdAsync(int id);
    Task<PagedResult<object>> GetCoursesAsync(QueryParameters parameters);
    Task<CourseResponseModel> CreateCourseAsync(CourseCreateModel model);
    Task<CourseResponseModel?> UpdateCourseAsync(int id, CourseUpdateModel model);
    Task<bool> DeleteCourseAsync(int id);
}