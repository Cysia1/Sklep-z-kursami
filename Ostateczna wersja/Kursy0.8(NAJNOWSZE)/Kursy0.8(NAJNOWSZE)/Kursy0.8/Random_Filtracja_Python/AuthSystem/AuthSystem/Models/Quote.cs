using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class Quote
    {
        [Key]
        public int QuoteId { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
    }

}
