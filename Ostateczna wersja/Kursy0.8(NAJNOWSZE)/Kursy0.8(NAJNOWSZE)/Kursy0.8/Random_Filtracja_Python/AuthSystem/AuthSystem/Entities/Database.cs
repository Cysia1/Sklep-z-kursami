using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthSystem.Areas.Identity.Data;
using static AuthSystem.Repositories.CourseRepository;


namespace AuthSystem.Entities
{
    public class Database
    {
        public class Courses : IEntity<int>
        {
            [Key]
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public string CourseDescription { get; set; }
            public string? CourseType { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal CoursePrice { get; set; }

            public byte[] Thumbnail { get; set; }

            public string? VideoUrl { get; set; }

            public byte[]? PdfFile { get; set; }

            [Column(TypeName = "nvarchar(450)")] // Zgodnie z typem Id w AspNetUsers
            public string? OwnerId { get; set; }

            // Jawna implementacja właściwości Id z interfejsu IEntity<int>
            int IEntity<int>.Id
            {
                get => CourseId;
                set => CourseId = value;
            }
        }

        public class UserCards : IEntity<int>
        {
            [Key]
            public int Id { get; set; }

            [Column(TypeName = "nvarchar(450)")] // ID użytkownika ASP.NET Identity
            public string UserId { get; set; }

            [Required]
            [Column(TypeName = "nvarchar(255)")]
            public string StripeCustomerId { get; set; } // ID klienta Stripe (do zarządzania zapisanymi metodami płatności)

            [Required]
            [Column(TypeName = "nvarchar(255)")]
            public string StripePaymentMethodId { get; set; } // ID metody płatności Stripe (konkretna karta)

            // Opcjonalne pola do wyświetlania użytkownikowi (nie do przetwarzania płatności)
            [Column(TypeName = "nvarchar(4)")]
            public string Last4 { get; set; } // Ostatnie 4 cyfry karty

            [Column(TypeName = "nvarchar(20)")]
            public string CardBrand { get; set; } // Marka karty (Visa, MasterCard itp.)

            // Jawna implementacja właściwości Id z interfejsu IEntity<int>
            int IEntity<int>.Id
            {
                get => Id;
                set => Id = value;
            }
        }

        public class Payments
        {
            [Key]
            public string PaymentId { get; set; }

            public int CourseId { get; set; }
            [ForeignKey("CourseId")]
            public Courses Course { get; set; }

            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public DateTime PaymentDate { get; set; }

            public string? UserId { get; set; }
            [ForeignKey("UserId")]
            public ApplicationUser? User { get; set; }

            public string Email { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentStatus { get; set; }
            public string StripeChargeId { get; set; }
        }

        public class Purchases
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string UserId { get; set; }

            [ForeignKey("UserId")]
            public ApplicationUser User { get; set; }

            [Required]
            public int CourseId { get; set; }

            public DateTime PurchaseDate { get; set; }
        }
    }
}