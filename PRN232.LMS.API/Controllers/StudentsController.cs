using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Models.StudentModels;
using PRN232.LMS.Services.Models.EnrollmentModels;
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
[Route("api/v{version:apiVersion}/students")]
[Authorize] // All student endpoints require a valid JWT by default
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IEnrollmentService _enrollmentService;

    public StudentsController(IStudentService studentService, IEnrollmentService enrollmentService)
    {
        _studentService = studentService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet("{id:int}", Name = "GetStudentById")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentById([FromRoute] int id, [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        // Demonstrate header usage
        if (!string.IsNullOrEmpty(requestId))
        {
            Response.Headers.Append("X-Request-Id", requestId);
        }

        var student = await _studentService.GetStudentByIdAsync(id);

        if (student == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
        }

        return Ok(ApiResponse<StudentResponseModel>.Ok(student));
    }

    [HttpGet("{id:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentEnrollments([FromRoute] int id)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(id);

        if (enrollments == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
        }

        return Ok(ApiResponse<IEnumerable<EnrollmentResponseModel>>.Ok(enrollments, "Student enrollments retrieved successfully"));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudents([FromQuery] StudentQueryRequest request)
    {
        var result = await _studentService.GetStudentsAsync(request);
        return result.ToPagedResponse();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admins can create students
    [ProducesResponseType(typeof(ApiResponse<StudentResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest model)
    {
        try
        {
            var student = await _studentService.CreateStudentAsync(model);
            return CreatedAtRoute("GetStudentById", new { id = student.StudentId }, ApiResponse<StudentResponseModel>.Ok(student, "Student created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create student.", ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")] // Only Admins can update students
    [ProducesResponseType(typeof(ApiResponse<StudentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStudent([FromRoute] int id, [FromBody] UpdateStudentRequest model)
    {
        try
        {
            var student = await _studentService.UpdateStudentAsync(id, model);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
            }

            return Ok(ApiResponse<StudentResponseModel>.Ok(student, "Student updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to update student.", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")] // Only Admins can delete students (role-based authorization requirement)
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteStudent([FromRoute] int id)
    {
        var deleted = await _studentService.DeleteStudentAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Student deleted successfully"));
    }
}
