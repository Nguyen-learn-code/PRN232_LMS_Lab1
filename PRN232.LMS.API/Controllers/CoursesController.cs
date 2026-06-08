using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Models.CourseModels;
using PRN232.LMS.Services.Models.StudentModels;
using PRN232.LMS.Services.Models.EnrollmentModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
[Authorize] // All course endpoints require a valid JWT by default
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IStudentService _studentService;

    public CoursesController(
        ICourseService courseService, 
        IEnrollmentService enrollmentService,
        IStudentService studentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _studentService = studentService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById([FromRoute] int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Course with ID {id} does not exist."));
        }

        return Ok(ApiResponse<CourseResponseModel>.Ok(course));
    }

    [HttpGet("{id:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseEnrollments([FromRoute] int id)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);

        if (enrollments == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Course with ID {id} does not exist."));
        }

        return Ok(ApiResponse<IEnumerable<EnrollmentResponseModel>>.Ok(enrollments, "Course enrollments retrieved successfully"));
    }

    // Nested resource: /api/courses/{courseId}/students
    [HttpGet("{courseId:int}/students")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentsByCourse([FromRoute] int courseId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
        
        if (enrollments == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Course with ID {courseId} does not exist."));
        }

        var students = new List<StudentResponseModel>();
        foreach (var enrollment in enrollments)
        {
            var student = await _studentService.GetStudentByIdAsync(enrollment.StudentId);
            if (student != null)
            {
                students.Add(student);
            }
        }

        return Ok(ApiResponse<IEnumerable<StudentResponseModel>>.Ok(students, "Students in course retrieved successfully"));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponseModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses([FromQuery] QueryParameters parameters)
    {
        var result = await _courseService.GetCoursesAsync(parameters);
        return result.ToPagedResponse();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Only admins can create courses
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
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")] // Only admins can update courses
    [ProducesResponseType(typeof(ApiResponse<CourseResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse([FromRoute] int id, [FromBody] CourseUpdateModel model)
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
                ApiResponse<object>.Fail("Failed to update course.", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")] // Only admins can delete courses
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse([FromRoute] int id)
    {
        var deleted = await _courseService.DeleteCourseAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail($"Course with ID {id} does not exist."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Course deleted successfully"));
    }
}