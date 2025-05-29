using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Repositories;
using AuthSystem.Models;
using System.Linq;
using System;
using static AuthSystem.Entities.Database;
using System.Collections.Generic;
using AuthSystem.Data; // Added for AuthDbContext
using Microsoft.EntityFrameworkCore;
using static AuthSystem.Repositories.CourseRepository;

public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepositoryService<Courses, int> _courseRepository;
    private readonly AuthDbContext _authDbContext;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserController(UserManager<ApplicationUser> userManager,
                          IRepositoryService<Courses, int> courseRepository,
                          AuthDbContext authDbContext,
                          SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _courseRepository = courseRepository;
        _authDbContext = authDbContext;
        _signInManager = signInManager;
    }

    // Zmieniamy na async Task<IActionResult>
    public async Task<IActionResult> UserPanel(string name, decimal? minPrice, decimal? maxPrice, string courseType, string ownershipFilter, string purchaseStatusFilter)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // Jeśli użytkownik nie jest zalogowany, przekieruj na stronę logowania lub główną
            return RedirectToAction("Login", "Account"); // Zakładając, że masz kontroler Account z akcją Login
        }

        var currentUserId = user.Id;

        // 1. Pobieramy wszystkie kursy z repozytorium
        // Użyj .ToList() na początku, aby uniknąć wielokrotnego odpytywania bazy danych
        var allCourses = _courseRepository.GetAll().ToList();

        // 2. Pobieramy ID wszystkich kursów zakupionych przez bieżącego użytkownika
        var purchasedCourseIds = await _authDbContext.Purchases
            .Where(p => p.UserId == currentUserId)
            .Select(p => p.CourseId)
            .ToListAsync(); // Użyj ToListAsync dla operacji asynchronicznej na bazie

        // 3. Tworzymy listę zakupionych kursów (od innych użytkowników)
        var purchasedCourses = allCourses
            .Where(c => purchasedCourseIds.Contains(c.CourseId) && c.OwnerId != currentUserId)
            .Select(c => new CoursesItemViewModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseType = c.CourseType,
                CoursePrice = c.CoursePrice,
                ThumbnailUrl = c.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = c.CourseId }) : null,
                VideoUrl = c.VideoUrl,
                OwnerId = c.OwnerId,
                IsPurchased = true // Oczywiście zakupiony
            })
            .ToList();

        // 4. Tworzymy listę kursów stworzonych przez bieżącego użytkownika
        var myCourses = allCourses
            .Where(c => c.OwnerId == currentUserId)
            .Select(c => new CoursesItemViewModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                CourseType = c.CourseType,
                CoursePrice = c.CoursePrice,
                ThumbnailUrl = c.Thumbnail != null ? Url.Action("GetThumbnail", "Courses", new { id = c.CourseId }) : null,
                VideoUrl = c.VideoUrl,
                OwnerId = c.OwnerId,
                // Dla własnych kursów IsPurchased może być true, jeśli użytkownik sam go kupił (raczej rzadkie)
                IsPurchased = purchasedCourseIds.Contains(c.CourseId)
            })
            .ToList();

        // 5. Wypełniamy ViewModel danymi użytkownika i listami kursów
        var userPanelViewModel = new UserPanelViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            PurchasedCourses = purchasedCourses,
            MyCourses = myCourses
        };

        // --- Poniższa logika filtracji odnosi się do ogólnego widoku kursów, nie zakładek w panelu użytkownika ---
        // Jeśli chcesz mieć możliwość filtrowania wewnątrz zakładek "Zakupione kursy" i "Twoje kursy",
        // musisz dodać podobną logikę filtrowania bezpośrednio do list 'purchasedCourses' i 'myCourses'
        // przed przypisaniem ich do ViewModelu.
        // Jeśli te filtry dotyczą innej sekcji widoku, możesz je pozostawić tak, jak są.

        var coursesForDisplay = allCourses.AsQueryable(); // Używamy AsQueryable() do dalszej filtracji LINQ to Entities

        if (!string.IsNullOrEmpty(name))
            coursesForDisplay = coursesForDisplay.Where(c => EF.Functions.Like(c.CourseName, $"%{name}%"));

        if (!string.IsNullOrEmpty(courseType))
            coursesForDisplay = coursesForDisplay.Where(c => c.CourseType != null && EF.Functions.Like(c.CourseType.ToLower(), $"%{courseType.ToLower()}%"));

        if (minPrice.HasValue)
            coursesForDisplay = coursesForDisplay.Where(c => c.CoursePrice >= minPrice.Value);

        if (maxPrice.HasValue)
            coursesForDisplay = coursesForDisplay.Where(c => c.CoursePrice <= maxPrice.Value);

        // Reszta logiki filtrowania (ownershipFilter, purchaseStatusFilter) powinna być dostosowana
        // do tego, gdzie faktycznie chcesz jej użyć. Jeśli ma wpływać na ViewModel,
        // to musisz zastosować ją przed tworzeniem 'purchasedCourses' i 'myCourses'.
        // W przeciwnym razie, może ona filtrować inną listę kursów, którą wyświetlasz.

        var coursesDisplayItems = coursesForDisplay.Select(p => new CoursesItemViewModel
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

        // ViewBag dla ogólnych filtrów (jeśli są używane na stronie)
        ViewBag.NameFilter = name;
        ViewBag.CourseTypeFilter = courseType;
        ViewBag.MinPriceFilter = minPrice;
        ViewBag.MaxPriceFilter = maxPrice;
        ViewBag.CourseTypes = allCourses.Select(c => c.CourseType).Where(ct => !string.IsNullOrEmpty(ct)).Distinct().ToList();
        ViewBag.OwnershipFilter = ownershipFilter;
        ViewBag.CurrentUserId = currentUserId;
        ViewBag.PurchaseStatusFilter = purchaseStatusFilter;
        ViewBag.AllCourses = coursesDisplayItems; // To jest lista wszystkich kursów, które są wyświetlane na głównym panelu (jeśli jest taki panel)

        // Random courses and category types (jeśli są używane)
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

        var categoryTypes = allCourses
            .Where(c => !string.IsNullOrEmpty(c.CourseType))
            .Select(c => c.CourseType)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
        ViewBag.CategoryTypes = categoryTypes;

        return View("~/Views/Account/UserPanel.cshtml", userPanelViewModel); // Przekazujemy ViewModel
    }

    // Pozostałe akcje kontrolera (UpdateUserData, DeleteAccount) bez zmian,
    // ponieważ ich logika jest niezależna od wyświetlania kursów w panelu.
    // Upewnij się, że ich implementacja jest kompletna i poprawna.

    [HttpPost]
    public async Task<IActionResult> UpdateUserData(string FirstName, string LastName, string Email, string PhoneNumber, string CurrentPassword, string NewPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Aktualizacja danych osobowych
        user.FirstName = FirstName;
        user.LastName = LastName;
        user.Email = Email;
        user.UserName = Email; // Upewnij się, że UserName jest aktualizowany, jeśli używasz go jako identyfikatora logowania
        user.NormalizedEmail = Email.ToUpperInvariant();
        user.NormalizedUserName = Email.ToUpperInvariant();
        user.PhoneNumber = PhoneNumber;

        // Logika zmiany hasła
        if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword))
        {
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["Error"] = "Błąd zmiany hasła: " + string.Join("; ", changePasswordResult.Errors.Select(e => e.Description));
                // Aby błędy były wyświetlone w widoku, musisz przekazać Model do widoku ponownie,
                // np. return View("~/Views/Account/UserPanel.cshtml", await BuildUserPanelViewModel());
                return RedirectToAction("UserPanel"); // Na razie przekierowanie
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Zaktualizowano dane użytkownika.";
        }
        else
        {
            TempData["Error"] = "Wystąpił błąd podczas aktualizacji danych: " + string.Join("; ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction("UserPanel");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var userId = user.Id;

        // KROK 1: ZACHOWAJ POWIĄZANE DANE (odłącz od użytkownika)
        var userPayments = _authDbContext.Payments.Where(p => p.UserId == userId).ToList();
        foreach (var payment in userPayments)
        {
            payment.UserId = null; // Ustawiamy na null, aby nie usuwać płatności
        }

        var userPurchases = _authDbContext.Purchases.Where(p => p.UserId == userId).ToList();
        foreach (var purchase in userPurchases)
        {
            purchase.UserId = null; // Ustawiamy na null, aby nie usuwać historii zakupów
        }

        await _authDbContext.SaveChangesAsync();

        // KROK 2: USUŃ KURSY STWORZONE PRZEZ UŻYTKOWNIKA
        var userCourses = _courseRepository.GetAll().Where(c => c.OwnerId == userId).ToList();
        foreach (var course in userCourses)
        {
            _courseRepository.Delete(course); // Zakładamy, że metoda Delete usuwa również powiązane dane (np. thumbnail)
        }

        // KROK 3: WYLOGUJ UŻYTKOWNIKA
        await _signInManager.SignOutAsync();

        // KROK 4: USUŃ UŻYTKOWNIKA
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = "Twoje konto zostało pomyślnie usunięte.";
        }
        else
        {
            TempData["Error"] = "Wystąpił błąd podczas usuwania konta.";
            // Jeśli wystąpią błędy, możesz je zalogować lub wyświetlić szczegóły
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error deleting user: {error.Code} - {error.Description}");
            }
        }

        return RedirectToAction("Index", "Home");
    }
}