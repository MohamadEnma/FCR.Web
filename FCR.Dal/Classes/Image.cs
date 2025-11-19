using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Dal.Classes
{
    [Index(nameof(CarId))]
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        [Required, StringLength(500)]
        public string Url { get; set; } = string.Empty;  

        [StringLength(150)]
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // Foreign Key
        [ForeignKey(nameof(Car))]
        public int CarId { get; set; }

        // Navigation Property
        public virtual Car Car { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
