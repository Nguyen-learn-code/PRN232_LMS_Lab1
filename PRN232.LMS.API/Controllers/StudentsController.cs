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

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")] // RESTful naming rule
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IEnrollmentService _enrollmentService;

    public StudentsController(IStudentService studentService, IEnrollmentService enrollmentService)
    {
        _studentService = studentService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentById(int id)
    {
        try
        {
            var student = await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
            }

            return Ok(ApiResponse<StudentResponseModel>.Ok(student));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve student.", ex.Message));
        }
    }

    [HttpGet("{id}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentEnrollments(int id)
    {
        try
        {
            var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(id);

            if (enrollments == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
            }

            return Ok(ApiResponse<IEnumerable<EnrollmentResponseModel>>.Ok(enrollments, "Student enrollments retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve student enrollments.", ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudents([FromQuery] QueryParameters parameters)
    {
        try
        {
            var result = await _studentService.GetStudentsAsync(parameters);
            return result.ToPagedResponse();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve students.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateStudent([FromBody] StudentCreateModel model)
    {
        try
        {
            var student = await _studentService.CreateStudentAsync(model);
            return CreatedAtAction(nameof(GetStudentById), new { id = student.StudentId }, ApiResponse<StudentResponseModel>.Ok(student, "Student created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create student.", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to create student.", ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentUpdateModel model)
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
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to update student.", ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        try
        {
            var deleted = await _studentService.DeleteStudentAsync(id);

            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} does not exist."));
            }

            return Ok(ApiResponse<object>.Ok(new { }, "Student deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to delete student.", ex.Message));
        }
    }
}
