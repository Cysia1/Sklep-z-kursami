using AuthSystem.Models;
using AuthSystem.Data;
using AuthSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AuthSystem.Entities.Database;
using static AuthSystem.Repositories.CourseRepository;

namespace AuthSystem.Controllers
{
    public class CoursesController : Controller
    {
        private readonly IRepositoryService<Courses, int> _courseRepository; 

        public CoursesController(IRepositoryService<Courses, int> courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public IActionResult Index(string name, decimal? minPrice, decimal? maxPrice, string courseType)
        {
            var courses = _courseRepository.GetAll();

            if (!string.IsNullOrEmpty(name)) //Filtracja po nazwie
            {
                courses = courses.Where(c => c.CourseName.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(courseType)) //Filtracja po typie kursu
            {
                if (Enum.TryParse<Entities.Database.CourseType>(courseType, out var parsedType))
                {
                    courses = courses.Where(c => c.CourseType == parsedType);
                }
            }

            if (minPrice.HasValue) // Wybór minimalnej ceny
            {
                courses = courses.Where(c => c.CoursePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue) //Wybór maksymalnej ceny
            {
                courses = courses.Where(c => c.CoursePrice <= maxPrice.Value);
            }

            var viewModel = courses.Select(p => new CoursesItemViewModel
            {
                CourseId = p.Id,
                CourseName = p.CourseName,
                CourseType = p.CourseType.ToString(),
                CoursePrice = p.CoursePrice,
                Thumbnail = p.Thumbnail
            }).ToList();

            return View("Index", viewModel);
        }

        public IActionResult Details(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course == null)
            {
                return NotFound();
            }
            var courses = new CoursesDetailViewModel
            {
                CourseId = course.Id,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                CourseDescription = course.CourseDescription,
                CoursePrice = course.CoursePrice,
                Thumbnail = course.Thumbnail,

            };
            return View(courses); 
        }

        public IActionResult Create()
        {
            return View(new CoursesDetailViewModel()); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoursesDetailViewModel coursesViewModel, IFormFile Thumbnail)
        {
            if (ModelState.IsValid)
            {
                var course = new Courses
                {
                    CourseName = coursesViewModel.CourseName,
                    CourseType = coursesViewModel.CourseType,
                    CourseDescription = coursesViewModel.CourseDescription,
                    CoursePrice = coursesViewModel.CoursePrice,
                    Thumbnail = Thumbnail != null ? await SaveFileAsync(Thumbnail) : null,

                };

                _courseRepository.Add(course);
                return RedirectToAction(nameof(Index));
            }
            return View(coursesViewModel ?? new CoursesDetailViewModel());
        }

        public IActionResult Edit(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new CoursesDetailViewModel
            {
                CourseId = course.Id,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                CourseDescription = course.CourseDescription,
                CoursePrice = course.CoursePrice,
                Thumbnail = course.Thumbnail,

            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoursesDetailViewModel courseViewModel, IFormFile Thumbnail)
        {
            if (ModelState.IsValid)
            {
                var course = _courseRepository.GetSingle(courseViewModel.CourseId);
                if (course == null)
                {
                    return NotFound();
                }

                course.CourseName = courseViewModel.CourseName;
                course.CourseType = courseViewModel.CourseType;
                course.CourseDescription = courseViewModel.CourseDescription;
                course.CoursePrice = courseViewModel.CoursePrice;
                course.Thumbnail = Thumbnail != null ? await SaveFileAsync(Thumbnail) : courseViewModel.Thumbnail;


                _courseRepository.Edit(course);
                return RedirectToAction(nameof(Index));
            }

            return View(courseViewModel ?? new CoursesDetailViewModel());
        }

        public IActionResult Delete(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course == null)
            {
                return NotFound();
            }

            var courseViewModel = new CoursesDetailViewModel
            {
                CourseId = course.Id,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                CourseDescription = course.CourseDescription,
                CoursePrice = course.CoursePrice,
                Thumbnail = course.Thumbnail,

            };

            return View(courseViewModel); 
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course != null)
            {
                _courseRepository.Delete(course);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{fileName}";
        }
    }
}