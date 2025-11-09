using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BookProject.Models.DTOS
{
    public class UpdateOrderStatusModel
    {
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
        public IEnumerable<SelectListItem>? OrderStatusList { get; set; }
    }
}
