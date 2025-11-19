using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCR.Bll.DTOs.User
{
    public class AdminStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalCars { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
