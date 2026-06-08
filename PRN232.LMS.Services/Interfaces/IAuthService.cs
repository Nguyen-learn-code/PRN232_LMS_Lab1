using System.Threading.Tasks;
using PRN232.LMS.Services.Models.AuthModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseData?> LoginAsync(LoginRequest request);
    Task<AuthResponseData?> RefreshTokenAsync(string refreshToken);
}
