using System.ComponentModel.DataAnnotations;

namespace BookProject.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
        [Required]
        public int StatusId { get; set; }
        [Required]
        [MaxLength(30)]
        public string? StatusName { get; set; }
    }
}
