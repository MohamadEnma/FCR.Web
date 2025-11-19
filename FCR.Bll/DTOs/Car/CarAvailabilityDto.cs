using System;
using System.Collections.Generic;

namespace FCR.Bll.DTOs
{
    public class CarAvailabilityDto
    {
        public int CarId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public List<BookingConflict>? Conflicts { get; set; }
    }

    public class BookingConflict
    {
        public int BookingId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}