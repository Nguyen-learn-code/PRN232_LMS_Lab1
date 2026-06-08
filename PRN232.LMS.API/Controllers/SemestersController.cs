using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Models.SemesterModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/semesters")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSemesterById([FromRoute] int id)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(id);

        if (semester == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} does not exist."));
        }

        return Ok(ApiResponse<SemesterResponseModel>.Ok(semester));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SemesterResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSemesters([FromQuery] QueryParameters parameters)
    {
        var result = await _semesterService.GetSemestersAsync(parameters);
        return result.ToPagedResponse();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSemester([FromBody] SemesterCreateModel model)
    {
        try
        {
            var semester = await _semesterService.CreateSemesterAsync(model);
            return CreatedAtAction(nameof(GetSemesterById), new { id = semester.SemesterId, version = HttpContext.GetRequestedApiVersion()?.ToString() }, ApiResponse<SemesterResponseModel>.Ok(semester, "Semester created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create semester.", ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSemester([FromRoute] int id, [FromBody] SemesterUpdateModel model)
    {
        try
        {
            var semester = await _semesterService.UpdateSemesterAsync(id, model);

            if (semester == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} does not exist."));
            }

            return Ok(ApiResponse<SemesterResponseModel>.Ok(semester, "Semester updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to update semester.", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSemester([FromRoute] int id)
    {
        var deleted = await _semesterService.DeleteSemesterAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} does not exist."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Semester deleted successfully"));
    }
}