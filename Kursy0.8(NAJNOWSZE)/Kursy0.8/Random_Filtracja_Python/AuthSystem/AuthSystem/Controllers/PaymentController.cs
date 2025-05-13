using System.Security.Cryptography;
using System.Text;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class PaymentController : Controller
{
  
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult PaymentStep1()
    {
        return View();
    }

    [HttpPost]
    public IActionResult PaymentStep2(string firstName, string lastName, string email)
    {
        // Tutaj możesz zapisać dane osobowe, jeśli potrzebujesz
        var model = new PurchasedCourseViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(PurchasedCourseViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.SaveCard)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    string cardNumber = model.CardNumber;
                    string encryptedCardNumber = EncryptString(cardNumber, "TwójSuperTajnyKlucz"); // PAMIĘTAJ O BEZPIECZNYM ZARZĄDZANIU KLUCZAMI!

                    user.EncryptedCardNumber = encryptedCardNumber;
                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        // Obsłuż błąd zapisu
                        ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas zapisywania danych karty.");
                        return View("PaymentStep2", model);
                    }
                }
            }

            // Tutaj powinna nastąpić PRAWDZIWA logika płatności (np. integracja z operatorem płatności)
            // ...

            // Jeśli płatność przebiegła pomyślnie, przekieruj lub zwróć widok, który wywoła pop-up
            return View("PaymentSuccess"); // Utwórz ten widok
        }

        // Jeśli model nie jest prawidłowy, wróć do kroku 2 z błędami
        return View("PaymentStep2", model);
    }

    // Metoda do szyfrowania (PAMIĘTAJ O BEZPIECZEŃSTWIE KLUCZA!)
    private string EncryptString(string plainText, string key)
    {
        byte[] iv = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(iv);
        }

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = iv;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                var encryptedBytes = msEncrypt.ToArray();
                var result = new byte[iv.Length + encryptedBytes.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
                return Convert.ToBase64String(result);
            }
        }
    }

    // Metoda do deszyfrowania (używaj z rozwagą i tylko gdy to konieczne)
    // private string DecryptString(string cipherText, string key) { ... }

    public IActionResult PaymentSuccess()
    {
        return View();
    }
}
