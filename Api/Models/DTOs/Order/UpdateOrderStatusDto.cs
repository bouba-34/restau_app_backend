using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Order
{

    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}