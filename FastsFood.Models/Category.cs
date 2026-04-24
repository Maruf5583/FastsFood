using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastsFood.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        // Navigation Properties
        public virtual ICollection<SubCategory> SubCategories { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}