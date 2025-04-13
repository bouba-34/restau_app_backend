using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Order
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}