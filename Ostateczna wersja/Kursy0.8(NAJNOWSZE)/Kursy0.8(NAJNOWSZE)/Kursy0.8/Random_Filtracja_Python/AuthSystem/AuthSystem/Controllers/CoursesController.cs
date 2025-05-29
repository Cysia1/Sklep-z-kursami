using AuthSystem.Models;
using AuthSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using static AuthSystem.Entities.Database;
using static AuthSystem.Repositories.CourseRepository;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;

namespace AuthSystem.Controllers
{
    public class CoursesController : Controller
    {
        private readonly IRepositoryService<Courses, int> _courseRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;

        public CoursesController(IRepositoryService<Courses, int> courseRepository,
                                   IHttpClientFactory httpClientFactory,
                                   UserManager<ApplicationUser> userManager,
                                   AuthDbContext authDbContext)
        {
            _courseRepository = courseRepository;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        public IActionResult Index(string name, decimal? minPrice, decimal? maxPrice, string courseType, string ownershipFilter, string purchaseStatusFilter)
        {
            var courses = _courseRepository.GetAll();
            var currentUserId = _userManager.GetUserId(User);

            // -- NOWY KOD --
            List<int> purchasedCourseIds = new List<int>();
            if (currentUserId != null)
            {
                purchasedCourseIds = _authDbContext.Purchases
                    .Where(p => p.UserId == currentUserId)
                    .Select(p => p.CourseId)
                    .ToList();
            }
            // -- KONIEC NOWEGO KODU --

            if (!string.IsNullOrEmpty(name))
                courses = courses.Where(c => EF.Functions.Like(c.CourseName, $"%{name}%"));

            if (!string.IsNullOrEmpty(courseType))
                courses = courses.Where(c => c.CourseType != null && EF.Functions.Like(c.CourseType.ToLower(), $"%{courseType.ToLower()}%"));

            if (minPrice.HasValue)
                courses = courses.Where(c => c.CoursePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                courses = courses.Where(c => c.CoursePrice <= maxPrice.Value);

            // Logika dla filtra własności
            if (!string.IsNullOrEmpty(ownershipFilter) && currentUserId != null)
            {
                if (ownershipFilter == "myCourses")
                {
                    courses = courses.Where(c => c.OwnerId == currentUserId);
                }
                else if (ownershipFilter == "otherCourses")
                {
                    courses = courses.Where(c => c.OwnerId != currentUserId);

                    // -- NOWY KOD: Logika dla nowego filtra zakupu --
                    if (!string.IsNullOrEmpty(purchaseStatusFilter))
                    {
                        if (purchaseStatusFilter == "purchased")
                        {
                            courses = courses.Where(c => purchasedCourseIds.Contains(c.CourseId));
                        }
                        else if (purchaseStatusFilter == "toBuy")
                        {
                            courses = courses.Where(c => !purchasedCourseIds.Contains(c.CourseId));
                        }
                    }
                    // -- KONIEC NOWEGO KODU --
                }
            }

            var viewModel = courses.Select(p => new CoursesItemViewModel
            {
                CourseId = p.CourseId,
                CourseName = p.CourseName,
                CourseType = p.CourseType,
                CoursePrice = p.CoursePrice,
                ThumbnailUrl = p.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = p.CourseId }) : null,
                VideoUrl = p.VideoUrl,
                OwnerId = p.OwnerId,
                IsPurchased = purchasedCourseIds.Contains(p.CourseId)
            }).ToList();

            var distinctCourseTypes = _courseRepository.GetAll()
                                                        .Select(c => c.CourseType)
                                                        .Where(ct => !string.IsNullOrEmpty(ct))
                                                        .Distinct()
                                                        .ToList();

            ViewBag.NameFilter = name;
            ViewBag.CourseTypeFilter = courseType;
            ViewBag.MinPriceFilter = minPrice;
            ViewBag.MaxPriceFilter = maxPrice;
            ViewBag.CourseTypes = distinctCourseTypes;
            ViewBag.OwnershipFilter = ownershipFilter;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.PurchaseStatusFilter = purchaseStatusFilter;

            return View("Index", viewModel);
        }

        public IActionResult Details(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            bool isPurchased = false;
            if (currentUserId != null)
            {
                isPurchased = _authDbContext.Purchases.Any(p => p.UserId == currentUserId && p.CourseId == id);
            }

            var viewModel = new CoursesDetailViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                CourseDescription = course.CourseDescription,
                CoursePrice = course.CoursePrice,
                ThumbnailUrl = course.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = course.CourseId }) : null,
                VideoUrl = course.VideoUrl,
                PdfAvailable = course.PdfFile != null && course.PdfFile.Length > 0,
                OwnerId = course.OwnerId,
                IsPurchased = isPurchased
            };
            return View(viewModel);
        }

        // Endpoint do pobierania PDF
        public IActionResult GetPdf(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course?.PdfFile != null && course.PdfFile.Length > 0)
            {
                return File(course.PdfFile, "application/pdf", $"{course.CourseName}.pdf");
            }
            return NotFound();
        }

        public IActionResult Create()
        {
            var courseTypes = _courseRepository.GetAll()
                .Where(c => !string.IsNullOrEmpty(c.CourseType))
                .Select(c => c.CourseType)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            ViewBag.CourseTypes = courseTypes;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoursesDetailViewModel coursesViewModel, IFormFile localImage, IFormFile pdfFile) // Added pdfFile
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var course = new Courses
                {
                    CourseName = coursesViewModel.CourseName,
                    CourseType = coursesViewModel.CourseType,
                    CourseDescription = coursesViewModel.CourseDescription,
                    CoursePrice = coursesViewModel.CoursePrice,
                    Thumbnail = null,
                    VideoUrl = coursesViewModel.VideoUrl,
                    OwnerId = userId,
                    PdfFile = null // Initialize PdfFile
                };

                // Zapisanie PDF do bazy (jeśli przesłano)
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await pdfFile.CopyToAsync(memoryStream);
                    course.PdfFile = memoryStream.ToArray();
                }

                _courseRepository.Add(course);

                if (localImage != null && localImage.Length > 0 && course.CourseId > 0)
                {
                    var pythonResponse = await UploadLocalFileToPythonAsync(localImage, course.CourseId, coursesViewModel.CourseType);
                    if (pythonResponse != null && pythonResponse.Success && pythonResponse.ThumbnailId != null)
                    {
                        course.Thumbnail = pythonResponse.ThumbnailId;
                        _courseRepository.Edit(course);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(coursesViewModel ?? new CoursesDetailViewModel());
        }

        public IActionResult Edit(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course == null) return NotFound();

            var viewModel = new CoursesDetailViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                CourseDescription = course.CourseDescription,
                CoursePrice = course.CoursePrice,
                ThumbnailUrl = course.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = course.CourseId }) : null,
                VideoUrl = course.VideoUrl,
                PdfAvailable = course.PdfFile != null && course.PdfFile.Length > 0 // PDF Availability Check
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoursesDetailViewModel courseViewModel, IFormFile localImage, IFormFile pdfFile) // Added pdfFile
        {
            if (ModelState.IsValid)
            {
                var course = _courseRepository.GetSingle(courseViewModel.CourseId);
                if (course == null) return NotFound();

                // Aktualizacja PDF (jeśli jest nowy plik)
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await pdfFile.CopyToAsync(memoryStream);
                    course.PdfFile = memoryStream.ToArray();
                }

                if (localImage != null && localImage.Length > 0)
                {
                    var pythonResponse = await UploadLocalFileToPythonAsync(localImage, course.CourseId, courseViewModel.CourseType);
                    if (pythonResponse != null && pythonResponse.Success && pythonResponse.ThumbnailId != null)
                    {
                        course.Thumbnail = pythonResponse.ThumbnailId;
                    }
                }

                course.CourseName = courseViewModel.CourseName;
                course.CourseType = courseViewModel.CourseType;
                course.CourseDescription = courseViewModel.CourseDescription;
                course.CoursePrice = courseViewModel.CoursePrice;
                course.VideoUrl = courseViewModel.VideoUrl;

                _courseRepository.Edit(course);
                return RedirectToAction(nameof(Index));
            }

            return View(courseViewModel ?? new CoursesDetailViewModel());
        }

        public IActionResult Delete(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course == null) return NotFound();

            var courseViewModel = new CoursesDetailViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                CourseDescription = course.CourseDescription,
                CoursePrice = course.CoursePrice,
                ThumbnailUrl = course.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = course.CourseId }) : null,
                VideoUrl = course.VideoUrl,
                PdfAvailable = course.PdfFile != null && course.PdfFile.Length > 0 // PDF Availability Check
            };

            return View(courseViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course != null)
                _courseRepository.Delete(course);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetThumbnail(int id)
        {
            var course = _courseRepository.GetSingle(id);
            if (course?.Thumbnail != null)
            {
                return File(course.Thumbnail, "image/jpeg");
            }
            return NotFound();
        }

        private async Task<PythonResponse> UploadUrlToPythonAsync(string imageUrl, int courseId, string? courseType)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return new PythonResponse { Success = false };

            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                imageUrl = imageUrl,
                courseId = courseId,
                courseType = courseType
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync("http://localhost:5000/upload-url-blob", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseString);
                if (json.TryGetProperty("id", out var idProperty))
                {
                    return new PythonResponse { Success = true, ThumbnailId = Convert.FromBase64String(idProperty.GetString()) };
                }
                else if (json.TryGetProperty("message", out var messageProperty) && messageProperty.GetString().Contains("zaktualizowane"))
                {
                    using var imageResponse = await client.GetAsync(imageUrl);
                    if (imageResponse.IsSuccessStatusCode)
                    {
                        return new PythonResponse { Success = true, ThumbnailId = await imageResponse.Content.ReadAsByteArrayAsync() };
                    }
                    return new PythonResponse { Success = false };
                }
                return new PythonResponse { Success = false };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Błąd HTTP podczas wysyłania do Pythona: {e.Message}");
                return new PythonResponse { Success = false, ErrorMessage = e.Message };
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Błąd deserializacji JSON z Pythona: {e.Message}");
                return new PythonResponse { Success = false, ErrorMessage = e.Message };
            }
        }

        private async Task<PythonResponse> UploadLocalFileToPythonAsync(IFormFile imageFile, int courseId, string? courseType)
        {
            if (imageFile == null || imageFile.Length == 0)
                return new PythonResponse { Success = false };

            var client = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(imageFile.OpenReadStream()), "imageFile", imageFile.FileName);
            content.Add(new StringContent(courseId.ToString()), "courseId");
            content.Add(new StringContent(courseType ?? ""), "courseType");

            try
            {
                var response = await client.PostAsync("http://localhost:5000/upload-local-blob", content);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseString);
                if (json.TryGetProperty("id", out var idProperty))
                {
                    // Zakładamy, że Python zwraca ID (bajty miniatury) zakodowane w Base64
                    return new PythonResponse { Success = true, ThumbnailId = Convert.FromBase64String(idProperty.GetString()) };
                }
                else if (json.TryGetProperty("message", out var messageProperty) && messageProperty.GetString().Contains("zaktualizowane"))
                {
                    // Jeśli Python zwrócił tylko informację o aktualizacji, spróbujmy pobrać obrazek,
                    // choć sensowniejsze byłoby, aby Python zwracał ID miniatury (lub bajty)
                    // przy każdej operacji, jeśli ma to być przechowywane w Courses.Thumbnail
                    return new PythonResponse { Success = true, ThumbnailId = null }; // lub pobierz z bazy danych
                }
                return new PythonResponse { Success = false };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Błąd HTTP podczas wysyłania lokalnego pliku do Pythona: {e.Message}");
                return new PythonResponse { Success = false, ErrorMessage = e.Message };
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Błąd deserializacji JSON z Pythona: {e.Message}");
                return new PythonResponse { Success = false, ErrorMessage = e.Message };
            }
        }

        private string? GetYouTubeVideoId(string url)
        {
            var regex = new Regex(@"(?:youtube\.com\/(?:[^\/]+\/[^\/]+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})");
            var match = regex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }

        public string? GetYouTubeEmbedUrl(string? videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return null;
            }
            var videoId = GetYouTubeVideoId(videoUrl);
            if (videoId != null)
            {
                // To jest prawdopodobnie błąd, powinno być "https://www.youtube.com/embed/{videoId}"
                // "https://www.youtube.com/embed/{videoId}" to wygląda na błędny URL
                return $"https://www.youtube.com/embed/{videoId}";
            }
            return null;
        }

        public class PythonResponse
        {
            public bool Success { get; set; }
            public byte[]? ThumbnailId { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}