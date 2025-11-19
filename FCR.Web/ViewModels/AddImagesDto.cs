using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Web.ViewModels
{
    public class AddImagesDto
    {
        public int CarId { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
