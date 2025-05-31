using FCR.Dal.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Dal.Models
{
    public class ImagesViewModel
    {
        public int CarId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public List<Image> Images { get; set; } = new List<Image>();
        public AddImagesDto ImageUpload { get; set; } = new AddImagesDto();
    }
}
