using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Repositories;
using AuthSystem.Models;
using System.Linq;
using System;
using static AuthSystem.Entities.Database;

public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CourseRepository.ICourseRepositoryService _courseRepository;

    public UserController(UserManager<ApplicationUser> userManager, CourseRepository.ICourseRepositoryService courseRepository)
    {
        _userManager = userManager;
        _courseRepository = courseRepository;
    }

    public IActionResult UserPanel()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var allCourses = _courseRepository.GetAll()?.ToList() ?? new List<Courses>();
        var randomCourses = allCourses
            .OrderBy(_ => Guid.NewGuid())
            .Take(Math.Min(3, allCourses.Count))
            .Select(c => new CoursesItemViewModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseType = c.CourseType?.ToString(),
                CoursePrice = c.CoursePrice,
                ThumbnailUrl = c.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = c.CourseId }) : null
            })
            .ToList();
        ViewBag.RandomUserCourses = randomCourses;
        var categoryTypes = _courseRepository.GetAll()
            .Where(c => !string.IsNullOrEmpty(c.CourseType))
            .Select(c => c.CourseType)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
        ViewBag.CategoryTypes = categoryTypes;

        return View("~/Views/Account/UserPanel.cshtml");
    }
}