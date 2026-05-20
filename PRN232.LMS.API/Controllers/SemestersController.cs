using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Models.SemesterModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSemesterById(int id)
    {
        try
        {
            var semester = await _semesterService.GetSemesterByIdAsync(id);

            if (semester == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} does not exist."));
            }

            return Ok(ApiResponse<SemesterResponseModel>.Ok(semester));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve semester.", ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SemesterResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSemesters([FromQuery] QueryParameters parameters)
    {
        try
        {
            var result = await _semesterService.GetSemestersAsync(parameters);
            return result.ToPagedResponse();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve semesters.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSemester([FromBody] SemesterCreateModel model)
    {
        try
        {
            var semester = await _semesterService.CreateSemesterAsync(model);
            return CreatedAtAction(nameof(GetSemesterById), new { id = semester.SemesterId }, ApiResponse<SemesterResponseModel>.Ok(semester, "Semester created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create semester.", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to create semester.", ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSemester(int id, [FromBody] SemesterUpdateModel model)
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
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to update semester.", ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSemester(int id)
    {
        try
        {
            var deleted = await _semesterService.DeleteSemesterAsync(id);

            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} does not exist."));
            }

            return Ok(ApiResponse<object>.Ok(new { }, "Semester deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to delete semester.", ex.Message));
        }
    }
}