using System.Collections.Generic;
using System.Threading.Tasks;
using PRN232.LMS.Services.Models.EnrollmentModels;
using PRN232.LMS.Repositories.Models.QueryModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseModel?> GetEnrollmentByIdAsync(int id);
    Task<PagedResult<object>> GetEnrollmentsAsync(QueryParameters parameters);
    Task<IEnumerable<EnrollmentResponseModel>?> GetEnrollmentsByCourseAsync(int courseId);
    Task<IEnumerable<EnrollmentResponseModel>?> GetEnrollmentsByStudentAsync(int studentId);
    Task<EnrollmentResponseModel> CreateEnrollmentAsync(EnrollmentCreateModel model);
    Task<EnrollmentResponseModel?> UpdateEnrollmentAsync(int id, EnrollmentUpdateModel model);
    Task<bool> DeleteEnrollmentAsync(int id);
}
