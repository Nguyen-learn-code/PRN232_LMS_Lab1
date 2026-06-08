using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Models.SubjectModels;
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
[Route("api/v{version:apiVersion}/subjects")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSubjectById([FromRoute] int id)
    {
        var subject = await _subjectService.GetSubjectByIdAsync(id);

        if (subject == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Subject with ID {id} does not exist."));
        }

        return Ok(ApiResponse<SubjectResponseModel>.Ok(subject));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubjectResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSubjects([FromQuery] QueryParameters parameters)
    {
        var result = await _subjectService.GetSubjectsAsync(parameters);
        return result.ToPagedResponse();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSubject([FromBody] SubjectCreateModel model)
    {
        try
        {
            var subject = await _subjectService.CreateSubjectAsync(model);
            return CreatedAtAction(nameof(GetSubjectById), new { id = subject.SubjectId, version = HttpContext.GetRequestedApiVersion()?.ToString() }, ApiResponse<SubjectResponseModel>.Ok(subject, "Subject created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create subject.", ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSubject([FromRoute] int id, [FromBody] SubjectUpdateModel model)
    {
        try
        {
            var subject = await _subjectService.UpdateSubjectAsync(id, model);

            if (subject == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Subject with ID {id} does not exist."));
            }

            return Ok(ApiResponse<SubjectResponseModel>.Ok(subject, "Subject updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to update subject.", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSubject([FromRoute] int id)
    {
        var deleted = await _subjectService.DeleteSubjectAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail($"Subject with ID {id} does not exist."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Subject deleted successfully"));
    }
}