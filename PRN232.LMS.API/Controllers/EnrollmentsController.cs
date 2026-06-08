using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.EnrollmentModels;
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
[Route("api/v{version:apiVersion}/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrollmentById([FromRoute] int id)
    {
        var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);

        if (enrollment == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Enrollment with ID {id} does not exist."));
        }

        return Ok(ApiResponse<EnrollmentResponseModel>.Ok(enrollment));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrollments([FromQuery] QueryParameters parameters)
    {
        var result = await _enrollmentService.GetEnrollmentsAsync(parameters);
        return result.ToPagedResponse();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEnrollment([FromBody] EnrollmentCreateModel model)
    {
        try
        {
            var enrollment = await _enrollmentService.CreateEnrollmentAsync(model);
            return CreatedAtAction(nameof(GetEnrollmentById), new { id = enrollment.EnrollmentId, version = HttpContext.GetRequestedApiVersion()?.ToString() }, ApiResponse<EnrollmentResponseModel>.Ok(enrollment, "Enrollment created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create enrollment.", ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEnrollment([FromRoute] int id, [FromBody] EnrollmentUpdateModel model)
    {
        try
        {
            var enrollment = await _enrollmentService.UpdateEnrollmentAsync(id, model);

            if (enrollment == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Enrollment with ID {id} does not exist."));
            }

            return Ok(ApiResponse<EnrollmentResponseModel>.Ok(enrollment, "Enrollment updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to update enrollment.", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEnrollment([FromRoute] int id)
    {
        var deleted = await _enrollmentService.DeleteEnrollmentAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail($"Enrollment with ID {id} does not exist."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Enrollment deleted successfully"));
    }
}
