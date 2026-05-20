using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Models.CourseModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(int id)
    {
        try
        {
            var course = await _courseService.GetCourseByIdAsync(id);

            if (course == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {id} does not exist."));
            }

            return Ok(ApiResponse<CourseResponseModel>.Ok(course));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve course.", ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponseModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses([FromQuery] QueryParameters parameters)
    {
        try
        {
            var result = await _courseService.GetCoursesAsync(parameters);
            return result.ToPagedResponse();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to retrieve courses.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse([FromBody] CourseCreateModel model)
    {
        try
        {
            var course = await _courseService.CreateCourseAsync(model);
            return CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, ApiResponse<CourseResponseModel>.Ok(course, "Course created successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create course.", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to create course.", ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateModel model)
    {
        try
        {
            var course = await _courseService.UpdateCourseAsync(id, model);

            if (course == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {id} does not exist."));
            }

            return Ok(ApiResponse<CourseResponseModel>.Ok(course, "Course updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<object>.Fail("Failed to create course.", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to update course.", ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        try
        {
            var deleted = await _courseService.DeleteCourseAsync(id);

            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {id} does not exist."));
            }

            return Ok(ApiResponse<object>.Ok(new { }, "Course deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Failed to delete course.", ex.Message));
        }
    }
}