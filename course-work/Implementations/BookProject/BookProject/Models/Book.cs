using NuGet.Protocol.Core.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookProject.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The BookName field is required.")]
        [MaxLength(50)]
        public string? BookName { get; set; }

        [Required(ErrorMessage = "The AuthorName field is required.")]
        [MaxLength(50)]
        public string? AuthorName { get; set; }
        [Required(ErrorMessage = "The field Price must be between.")]
        public double Price { get; set; }
        public string? Image {  get; set; }
        [Required(ErrorMessage = "The GenreId field is required.")]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
        public List<CartDetail> CartDetail { get; set; }
        public Stock Stock { get; set; }

        [NotMapped]
        public string GenreName { get; set; }
		public int Quantity { get; set; }
	}
}
