using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class PurchasedCourseViewModel
    {
        // ----- POCZĄTEK ZMIAN -----
        // TE POLA SĄ ZBĘDNE PRZY KORZYSTANIU ZE STRIPE ELEMENTS I POWODUJĄ BŁĄD WALIDACJI
        // NALEŻY JE USUNĄĆ LUB ZAKOMENTOWAĆ

        // [Required(ErrorMessage = "Numer karty jest wymagany.")]
        // [Display(Name = "Numer karty")]
        // [RegularExpression(@"^\d{13,16}$", ErrorMessage = "Numer karty musi zawierać od 13 do 16 cyfr.")]
        // public string CardNumber { get; set; }

        // [Required(ErrorMessage = "Data ważności jest wymagana.")]
        // [Display(Name = "Data ważności")]
        // [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Nieprawidłowy format daty (MM/RR).")]
        // public string ExpiryDate { get; set; }

        // [Required(ErrorMessage = "Kod CVV jest wymagany.")]
        // [Display(Name = "CVV")]
        // [StringLength(3, MinimumLength = 3, ErrorMessage = "Kod CVV musi mieć 3 cyfry.")]
        // [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "Nieprawidłowy format kodu CVV.")]
        // public string Cvv { get; set; }

        // ----- KONIEC ZMIAN -----

        public bool SaveCard { get; set; }
        public decimal CoursePrice { get; set; }

        [Required(ErrorMessage = "Token płatności jest wymagany.")]
        public string StripeToken { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email.")]
        public string Email { get; set; }
    }
}
