using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Auth;
using FCR.Bll.DTOs.Booking;
using FCR.Bll.DTOs.User;
using FCR.Dal.Classes;
using Mapster;
using System.Linq;

namespace FCR.Bll.Mappings
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // Car to CarResponseDto - NULL-SAFE without ?. operator
            TypeAdapterConfig<Car, CarResponseDto>
                .NewConfig()
                .Map(dest => dest.PrimaryImageUrl,
                     src => src.Images != null && src.Images.Any()
                            ? src.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
                            : null)
                .Map(dest => dest.TotalBookings,
                     src => src.Bookings != null ? src.Bookings.Count : 0);

            // Booking to BookingResponseDto - NULL-SAFE without ?. operator
            TypeAdapterConfig<Booking, BookingResponseDto>
                .NewConfig()
                .Map(dest => dest.BookingId, src => src.BookingId)
                .Map(dest => dest.BookingDate, src => src.CreatedAt)
                .Map(dest => dest.CarBrand, src => src.Car != null ? src.Car.Brand : string.Empty)
                .Map(dest => dest.CarModel, src => src.Car != null ? src.Car.Model : string.Empty)
                .Map(dest => dest.CarYear, src => src.Car != null ? src.Car.Year : 0)
                .Map(dest => dest.CarImageUrl,
                     src => src.Car != null && src.Car.Images != null && src.Car.Images.Any()
                            ? src.Car.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
                            : null)
                .Map(dest => dest.UserFullName,
                     src => src.User != null
                            ? string.Concat(src.User.FirstName, " ", src.User.LastName)
                            : string.Empty)
                .Map(dest => dest.UserEmail,
                     src => src.User != null ? src.User.Email : string.Empty)
                .Map(dest => dest.TotalDays,
                     src => (src.ReturnDate - src.PickupDate).Days)
                .Map(dest => dest.DailyRate,
                     src => src.Car != null ? src.Car.DailyRate : 0);

            // ApplicationUser to UserDto
            TypeAdapterConfig<ApplicationUser, UserDto>
                .NewConfig()
                .Map(dest => dest.DisplayName,
                     src => string.Concat(src.FirstName, " ", src.LastName));

            // ApplicationUser to UserProfileDto
            TypeAdapterConfig<ApplicationUser, UserProfileDto>
                .NewConfig()
                .Map(dest => dest.DisplayName,
                     src => string.Concat(src.FirstName, " ", src.LastName))
                .Ignore(dest => dest.TotalBookings)
                .Ignore(dest => dest.ActiveBookings)
                .Ignore(dest => dest.CompletedBookings)
                .Ignore(dest => dest.CancelledBookings)
                .Ignore(dest => dest.TotalSpent);

            // RegisterDto to ApplicationUser
            TypeAdapterConfig<RegisterDto, ApplicationUser>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.Email);

            // CarCreateDto to Car
            TypeAdapterConfig<CarCreateDto, Car>
                .NewConfig()
                .Map(dest => dest.IsAvailable, src => true)
                .Map(dest => dest.IsDeleted, src => false);
        }
    }
}