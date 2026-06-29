using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.Repositories.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;

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

        var itemsList = result.Items != null ? result.Items.ToList() : new List<T>();

        if (typeof(T) == typeof(object) && itemsList.Any())
        {
            var firstItem = itemsList.First();
            if (firstItem != null)
            {
                var itemType = firstItem.GetType();
                if (!itemType.Name.Contains("AnonymousType") && !itemType.Name.Contains("DynamicClass"))
                {
                    var castMethod = typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(itemType);
                    var toListMethod = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(itemType);
                    
                    var castedItems = castMethod.Invoke(null, new object[] { itemsList });
                    var typedList = toListMethod.Invoke(null, new object[] { castedItems! });

                    var apiResponseType = typeof(ApiResponse<>).MakeGenericType(typeof(List<>).MakeGenericType(itemType));
                    var okMethod = apiResponseType.GetMethod("Ok", new[] { typeof(List<>).MakeGenericType(itemType), typeof(string), typeof(PaginationMetadata) });
                    
                    var apiResponse = okMethod!.Invoke(null, new[] { typedList!, "Request processed successfully", pagination });
                    return new OkObjectResult(apiResponse);
                }
            }
        }

        return new OkObjectResult(ApiResponse<List<T>>.Ok(itemsList, pagination: pagination));
    }
}
