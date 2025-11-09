using System.ComponentModel.DataAnnotations;

namespace BookProject.Models.DTOS
{
    public class CheckOutModel
    {
        [Required]
        [MaxLength(30)]
        public string? Name { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? MobileNumber { get; set; }
        [Required]
        [MaxLength(250)]
        public string? Address { get; set; }
        [Required]
        [MaxLength(30)]
        public string? PaymentMethod { get; set; }
    }
}
