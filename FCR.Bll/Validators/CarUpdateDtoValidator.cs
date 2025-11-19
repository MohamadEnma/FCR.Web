using FCR.Bll.DTOs;
using FluentValidation;
using System;

namespace FCR.Bll.Validators
{
    public class CarUpdateDtoValidator : AbstractValidator<CarUpdateDto>
    {
        public CarUpdateDtoValidator()
        {
            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required")
                .MaximumLength(50).WithMessage("Brand must not exceed 50 characters");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required")
                .MaximumLength(50).WithMessage("Model must not exceed 50 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(30).WithMessage("Category must not exceed 30 characters");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, DateTime.UtcNow.Year + 1)
                .WithMessage($"Year must be between 2000 and {DateTime.UtcNow.Year + 1}");

            RuleFor(x => x.DailyRate)
                .GreaterThan(0).WithMessage("Daily rate must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Daily rate must not exceed 10,000");

            RuleFor(x => x.WeeklyRate)
                .GreaterThan(0)
                .When(x => x.WeeklyRate.HasValue)
                .WithMessage("Weekly rate must be greater than 0");

            RuleFor(x => x.MonthlyRate)
                .GreaterThan(0)
                .When(x => x.MonthlyRate.HasValue)
                .WithMessage("Monthly rate must be greater than 0");

            RuleFor(x => x.Transmission)
                .NotEmpty().WithMessage("Transmission type is required")
                .MaximumLength(20).WithMessage("Transmission must not exceed 20 characters");

            RuleFor(x => x.FuelType)
                .NotEmpty().WithMessage("Fuel type is required")
                .MaximumLength(20).WithMessage("Fuel type must not exceed 20 characters");

            RuleFor(x => x.Seats)
                .InclusiveBetween(2, 9).WithMessage("Seats must be between 2 and 9");

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0).WithMessage("Mileage cannot be negative")
                .LessThanOrEqualTo(500000).WithMessage("Mileage must not exceed 500,000");

            RuleFor(x => x.Color)
                .MaximumLength(30)
                .When(x => !string.IsNullOrEmpty(x.Color))
                .WithMessage("Color must not exceed 30 characters");

            RuleFor(x => x.LicensePlate)
                .MaximumLength(20)
                .When(x => !string.IsNullOrEmpty(x.LicensePlate))
                .WithMessage("License plate must not exceed 20 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 1000 characters");
        }
    }
}