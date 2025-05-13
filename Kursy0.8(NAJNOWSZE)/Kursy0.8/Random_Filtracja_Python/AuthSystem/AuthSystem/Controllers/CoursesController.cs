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

namespace AuthSystem.Controllers
{
    public class CoursesController : Controller
    {
        private readonly IRepositoryService<Courses, int> _courseRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public CoursesController(IRepositoryService<Courses, int> courseRepository, IHttpClientFactory httpClientFactory)
        {
            _courseRepository = courseRepository;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index(string name, decimal? minPrice, decimal? maxPrice, string courseType)
        {
            var courses = _courseRepository.GetAll();

            if (!string.IsNullOrEmpty(name))
                courses = courses.Where(c => EF.Functions.Like(c.CourseName, $"%{name}%")); 

            if (!string.IsNullOrEmpty(courseType))
                courses = courses.Where(c => EF.Functions.Like(c.CourseType.ToLower(), $"%{courseType.ToLower()}%"));
            if (minPrice.HasValue)
                courses = courses.Where(c => c.CoursePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                courses = courses.Where(c => c.CoursePrice <= maxPrice.Value);

            var viewModel = courses.Select(p => new CoursesItemViewModel
            {
                CourseId = p.CourseId,
                CourseName = p.CourseName,
                CourseType = p.CourseType,
                CoursePrice = p.CoursePrice,
                ThumbnailUrl = p.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = p.CourseId }) : null
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

            return View("Index", viewModel);
        }

        public IActionResult Details(int id)
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
                ThumbnailUrl = course.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = course.CourseId }) : null
            };
            return View(viewModel);
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
        public async Task<IActionResult> Create(CoursesDetailViewModel coursesViewModel, IFormFile localImage)
        {
            if (ModelState.IsValid)
            {
                string thumbnailId = null;

                var course = new Courses
                {
                    CourseName = coursesViewModel.CourseName,
                    CourseType = coursesViewModel.CourseType,
                    CourseDescription = coursesViewModel.CourseDescription,
                    CoursePrice = coursesViewModel.CoursePrice,
                    Thumbnail = null 
                };

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
                ThumbnailUrl = course.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = course.CourseId }) : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoursesDetailViewModel courseViewModel, IFormFile localImage)
        {
            if (ModelState.IsValid)
            {
                var course = _courseRepository.GetSingle(courseViewModel.CourseId);
                if (course == null) return NotFound();

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
                ThumbnailUrl = course.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = course.CourseId }) : null
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
                    return new PythonResponse { Success = true, ThumbnailId = Convert.FromBase64String(idProperty.GetString()) };
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
    }
    public class PythonResponse
    {
        public bool Success { get; set; }
        public byte[]? ThumbnailId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}