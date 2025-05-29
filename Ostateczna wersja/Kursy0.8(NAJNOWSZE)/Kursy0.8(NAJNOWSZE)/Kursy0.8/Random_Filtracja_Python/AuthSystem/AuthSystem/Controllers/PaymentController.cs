using System.Security.Cryptography;
using System.Text;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Stripe;
using AuthSystem.Data;
using static AuthSystem.Entities.Database;

public class PaymentController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string _stripeSecretKey;
    private readonly AuthDbContext _authDbContext;
    private readonly KursyDbContext _kursyDbContext;

    public PaymentController(UserManager<ApplicationUser> userManager, IConfiguration configuration, KursyDbContext kursyDbContext, AuthDbContext authDbContext)
    {
        _userManager = userManager;
        _stripeSecretKey = configuration.GetSection("Stripe")["SecretKey"];
        _kursyDbContext = kursyDbContext;
        _authDbContext = authDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> PaymentStep1(int courseId)
    {
        var course = await _kursyDbContext.Courses.FindAsync(courseId);
        if (course == null)
        {
            return NotFound();
        }
        ViewBag.CourseId = courseId;
        ViewBag.CoursePrice = course.CoursePrice;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PaymentStep2(string firstName, string lastName, string email, int courseId)
    {
        var course = await _kursyDbContext.Courses.FindAsync(courseId);
        if (course == null)
        {
            return NotFound();
        }
        var model = new PurchasedCourseViewModel
        {
            CoursePrice = course.CoursePrice
        };
        ViewBag.CourseId = courseId;
        ViewBag.Email = email;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(PurchasedCourseViewModel model, int courseId, string Email)
    {
        var course = await _kursyDbContext.Courses.FindAsync(courseId);
        if (course == null)
        {
            return Json(new { success = false, error = "Nie znaleziono kursu." });
        }

        if (ModelState.IsValid && !string.IsNullOrEmpty(model.StripeToken))
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
            var options = new ChargeCreateOptions
            {
                Amount = (long)(course.CoursePrice * 100),
                Currency = "pln",
                Description = $"Zakup kursu o ID: {courseId} przez {User.Identity?.Name ?? Email}",
                Source = model.StripeToken,
            };

            var service = new ChargeService();
            try
            {
                Charge charge = await service.CreateAsync(options);

                if (charge.Status == "succeeded")
                {
                    string userId = null;
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        var user = await _userManager.GetUserAsync(User);
                        userId = user?.Id;
                    }
                    await SavePaymentToDatabase(courseId, charge.Id, charge.Amount / 100.0m, charge.Currency, DateTime.UtcNow, userId, Email);

                    // -- Zapis informacji o zakupie --
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var purchase = new Purchases
                        {
                            UserId = userId,
                            CourseId = courseId,
                            PurchaseDate = DateTime.UtcNow
                        };
                        _authDbContext.Purchases.Add(purchase);
                        await _authDbContext.SaveChangesAsync();
                    }
                    // -- Koniec --

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = "Wystąpił błąd podczas przetwarzania płatności." });
                }
            }
            catch (StripeException e)
            {
                return Json(new { success = false, error = $"Wystąpił błąd płatności: {e.Message}" });
            }
        }

        return Json(new { success = false, error = "Nieprawidłowe dane płatności." });
    }

    private async Task SavePaymentToDatabase(int courseId, string paymentId, decimal amount, string currency, DateTime paymentDate, string userId, string email)
    {
        var payment = new Payments
        {
            PaymentId = paymentId,
            CourseId = courseId,
            Amount = amount,
            Currency = currency,
            PaymentDate = paymentDate,
            UserId = userId,
            Email = email,
            PaymentMethod = "Stripe Token",
            PaymentStatus = "Succeeded",
            StripeChargeId = paymentId
        };

        _authDbContext.Payments.Add(payment);
        await _authDbContext.SaveChangesAsync();
    }

    public IActionResult PaymentSuccess()
    {
        return View();
    }
}