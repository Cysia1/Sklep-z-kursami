using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using AuthSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using static AuthSystem.Entities.Database;

namespace AuthSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly KursyDbContext _kursyContext;
        private readonly CourseRepository.ICourseRepositoryService _courseRepository; 

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, AuthDbContext context, KursyDbContext kursyContext, CourseRepository.ICourseRepositoryService courseRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _kursyContext = kursyContext;
            _courseRepository = courseRepository;
        }

        
        public IActionResult Index()
        {
            var allCourses = _courseRepository.GetAll()?.ToList() ?? new List<Courses>();

            var popularCourses = allCourses
                .OrderBy(_ => Guid.NewGuid())
                .Take(Math.Min(3, allCourses.Count))
                .Select(c => new CoursesItemViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    CourseType = c.CourseType != null ? c.CourseType.ToString() : null, 
                    CoursePrice = c.CoursePrice,
                    ThumbnailUrl = c.Thumbnail != null && c.Thumbnail.Length > 0
                        ? $"data:image/png;base64,{Convert.ToBase64String(c.Thumbnail)}"
                        : null
                })
                .ToList();

            if (popularCourses.Count < 3)
            {
          
                ViewBag.PopularCourses = new List<CoursesItemViewModel>();
            }
            else
            {
                ViewBag.PopularCourses = popularCourses; 
            }

            var randomQuote = _context.Quotes
                .OrderBy(q => Guid.NewGuid())
                .FirstOrDefault();

            ViewData["UserID"] = _userManager.GetUserId(this.User);
            ViewData["RandomQuoteText"] = randomQuote?.Text;
            ViewData["RandomQuoteAuthor"] = randomQuote?.AuthorName;

            var distinctCourseTypes = _kursyContext.Courses
                .Select(c => c.CourseType)
                .Where(ct => !string.IsNullOrEmpty(ct))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            ViewBag.CategoryTypes = distinctCourseTypes;

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Statute()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}