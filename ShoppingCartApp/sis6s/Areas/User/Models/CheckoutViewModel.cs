using System.ComponentModel.DataAnnotations;

namespace sis6s.Areas.User.Models
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }
        
        [Required]
        [Display(Name = "Shipping Method")]
        public string ShippingMethod { get; set; }
        
        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
    }
} 