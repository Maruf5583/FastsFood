using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastsFood.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public DateTime TimeofPick { get; set; }

        [Required]
        public DateTime DateofPick { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double OrderTotal { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double SubtTotal { get; set; }

        public string CouponCode { get; set; }

        [Range(0, double.MaxValue)]
        public double Coupondiscount { get; set; }

        public string TransactionId { get; set; }

        public string OrderStatus { get; set; }

        public string PaymentStatus { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        // Navigation Properties
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}