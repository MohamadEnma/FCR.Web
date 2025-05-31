using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Dal.Classes
{
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        [Required, StringLength(500)]
        public string Url { get; set; } = string.Empty;  // e.g., /images/cars/volvo123.jpg

        [StringLength(150)]
        public string? AltText { get; set; }             // For accessibility

        // Foreign Key
        [ForeignKey(nameof(Car))]
        public int CarId { get; set; }

        // Navigation Property
        public virtual Car Car { get; set; } = null!;
    }
}
