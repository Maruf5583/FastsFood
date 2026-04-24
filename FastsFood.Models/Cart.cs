using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastsFood.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Count { get; set; }

        // Navigation Properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}