using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.Repositories.Models.QueryModels;

namespace PRN232.LMS.API.Extensions;

public static class PagedResultExtensions
{
    public static IActionResult ToPagedResponse<T>(this PagedResult<T> result)
    {
        var pagination = new PaginationMetadata
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };

        return new OkObjectResult(ApiResponse<IEnumerable<T>>.Ok(result.Items, pagination: pagination));
    }
}
