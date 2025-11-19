using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Booking;
using FluentValidation;
using System;

namespace FCR.Bll.Validators
{
    public class BookingCreateDtoValidator : AbstractValidator<BookingCreateDto>
    {
        public BookingCreateDtoValidator()
        {
            RuleFor(x => x.CarId)
                .GreaterThan(0).WithMessage("Valid car ID is required");

            RuleFor(x => x.PickupDate)
                .NotEmpty().WithMessage("Pickup date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Pickup date cannot be in the past");

            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Return date is required")
                .GreaterThan(x => x.PickupDate)
                .WithMessage("Return date must be after pickup date");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Notes must not exceed 500 characters");
        }
    }
}