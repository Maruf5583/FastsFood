using System.ComponentModel.DataAnnotations;

namespace FastsFood.ViewModel
{
    public class CheckoutVm
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Delivery address is required")]
        [StringLength(500)]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Delivery Note")]
        [StringLength(500)]
        public string DeliveryNote { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "SSLCommerz";

        public double SubTotal { get; set; }
        public double DeliveryFee { get; set; } = 60;
        public double Discount { get; set; }
        public double Total { get; set; }
        public string CouponCode { get; set; }
    }

    public class OrderConfirmationVm
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public double Total { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public List<OrderItemVm> Items { get; set; }
    }

    public class OrderItemVm
    {
        public string Title { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
    }

    public class FakePaymentVm
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public double Amount { get; set; }
    }

    public class CartItemVm
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Total => Price * Quantity;
    }
}