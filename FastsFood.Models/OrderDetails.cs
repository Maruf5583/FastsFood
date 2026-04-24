using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastsFood.Models
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderHeaderId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Count { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        // Navigation Properties
        [ForeignKey("OrderHeaderId")]
        public virtual OrderHeader OrderHeader { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
    }
}