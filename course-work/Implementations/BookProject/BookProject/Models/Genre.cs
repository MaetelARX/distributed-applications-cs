using System.ComponentModel.DataAnnotations;

namespace BookProject.Models
{
    public class Genre
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string GenreName { get; set; }
        public List<Book> Books { get; set; }
    }
}