using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FCR.Dal.Classes;
using FCR.Dal.Data;
using FCR.Dal.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace FCR.Web.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cars.ToListAsync());
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details( int CarId)
        {
            if (CarId == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Images)
                .FirstOrDefaultAsync(m => m.CarId == CarId);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            var viewModel = new CarViewModel
            {
                ImageUrls = new List<string> { "" }
            };
            return View(viewModel);

        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var car = new Car
                {
                    Brand = viewModel.Brand,
                    Model = viewModel.Model,
                    Category = viewModel.Category,
                    Year = viewModel.Year,
                    DailyRate = viewModel.DailyRate,
                    IsAvailable = viewModel.IsAvailable,
                    Transmission = viewModel.Transmission,
                    FuelType = viewModel.FuelType,
                    Seats = viewModel.Seats,
                    Images = viewModel.ImageUrls
                            .Where(url => !string.IsNullOrWhiteSpace(url))
                            .Select(url => new Image
                            {
                                Url = url,
                                AltText = $"{viewModel.Brand} {viewModel.Model} Image"
                            })
                            .ToList()
                };
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CarId,Brand,Model,Category,Year,DailyRate,IsAvailable,Transmission,FuelType,Seats,IsDeleted")] Car car)
        {
            if (id != car.CarId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.CarId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.CarId == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.CarId == id);
        }

        [HttpGet("Images/{carId}")]
        public async Task<IActionResult> Images(int carId)
        {
            var car = await _context.Cars
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.CarId == carId);

            if (car == null) return NotFound();

            var viewModel = new ImagesViewModel
            {
                CarId = car.CarId,
                Brand = car.Brand,
                Model = car.Model,
                Images = car.Images.ToList(),
                ImageUpload = new AddImagesDto { CarId = car.CarId }
            };

            return View(viewModel);
        }

        [HttpPost("Images/{carId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Images(int carId, AddImagesDto dto)
        {
            var car = await _context.Cars
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.CarId == carId);

            if (car == null) return NotFound();

            if (ModelState.IsValid)
            {
                foreach (var url in dto.ImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                {
                    car.Images.Add(new Image
                    {
                        Url = url,
                        AltText = $"{car.Brand} {car.Model}",
                        CarId = carId
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Images), new { carId });
            }

            // If validation fails, rebuild the ViewModel
            var viewModel = new ImagesViewModel
            {
                CarId = car.CarId,
                Brand = car.Brand,
                Model = car.Model,
                Images = car.Images.ToList(),
                ImageUpload = dto
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int carId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Images), new { carId = carId });
        }
    }
}