using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookProject.Models
{
	public class Details
	{
		public int Id { get; set; }
		[Required]
		[MaxLength(1000)]
		public string Description { get; set; }
		[MaxLength(100)]
		public string Publisher { get; set; }
		public DateTime? PublicationDate { get; set; }
		[Required]
		public int BookId { get; set; }
		[ForeignKey("BookId")]
		public Book? book { get; set; }
	}
}
