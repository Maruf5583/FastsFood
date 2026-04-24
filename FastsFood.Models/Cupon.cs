using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastsFood.Models
{
    public class Cupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public CuponType Type { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Discount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double MinimumAmount { get; set; }

        public byte[] CuponPicture { get; set; }

        public bool IsActive { get; set; }
    }

    public enum CuponType
    {
        Percentage,
        Amount
    }
}