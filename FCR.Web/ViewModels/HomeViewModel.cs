using FCR.Web.Services.Base;

namespace FCR.Web.ViewModels
{
    public class HomeViewModel
    {
        public List<CarResponseDto> AllCars { get; set; } = new List<CarResponseDto>();
    }
}