using Microsoft.AspNetCore.Mvc;
using AuthSystem.Repositories;
using System.Linq;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly CourseRepository.ICourseRepositoryService _courseRepository;

    public CategoryMenuViewComponent(CourseRepository.ICourseRepositoryService courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public IViewComponentResult Invoke()
    {
        var categories = _courseRepository.GetAll()
            .Where(c => !string.IsNullOrEmpty(c.CourseType))
            .Select(c => c.CourseType)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return View(categories);
    }
}