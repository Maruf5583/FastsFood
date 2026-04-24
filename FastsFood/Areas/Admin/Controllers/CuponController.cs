using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel.FastsFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FastsFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CuponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuponController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Cupon
        public async Task<IActionResult> Index()
        {
            // Fixed: Changed "Copons" to "Coupons" to match DbSet name
            var cupons = await _context.Coupons.ToListAsync();
            return View(cupons);
        }

        // GET: Admin/Cupon/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cupon = await _context.Coupons.FindAsync(id);

            if (cupon == null)
            {
                return NotFound();
            }

            // Convert byte array to base64 for display
            if (cupon.CuponPicture != null && cupon.CuponPicture.Length > 0)
            {
                ViewBag.ImageBase64 = Convert.ToBase64String(cupon.CuponPicture);
            }

            return View(cupon);
        }

        // GET: Admin/Cupon/Create
        public IActionResult Create()
        {
            // Fixed: Changed ViewBag name and properly bind enum values
            ViewBag.CuponTypes = new SelectList(Enum.GetValues(typeof(CuponType))
                .Cast<CuponType>()
                .Select(e => new { Value = e, Text = e.ToString() }),
                "Value", "Text");

            return View();
        }

        // POST: Admin/Cupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CuponVm vm)
        {
            // Remove validation for CuponPicture since it's optional
            ModelState.Remove("ExistingImageBase64");

            if (ModelState.IsValid)
            {
                Cupon model = new Cupon();

                // Image Handling (store as byte array in database)
                if (vm.CuponImageFile != null && vm.CuponImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await vm.CuponImageFile.CopyToAsync(memoryStream);
                        model.CuponPicture = memoryStream.ToArray();
                    }
                }

                // Mapping properties
                model.Title = vm.Title;
                model.Type = vm.Type;
                model.Discount = vm.Discount;
                model.MinimumAmount = vm.MinimumAmount;
                model.IsActive = vm.IsActive;

                _context.Coupons.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cupon created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload ViewBag if model is invalid
            ViewBag.CuponTypes = new SelectList(Enum.GetValues(typeof(CuponType))
                .Cast<CuponType>()
                .Select(e => new { Value = e, Text = e.ToString() }),
                "Value", "Text", vm.Type);

            return View(vm);
        }

        // GET: Admin/Cupon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cupon = await _context.Coupons.FindAsync(id);

            if (cupon == null)
            {
                return NotFound();
            }

            CuponVm vm = new CuponVm
            {
                Id = cupon.Id,
                Title = cupon.Title,
                Type = cupon.Type,
                Discount = cupon.Discount,
                MinimumAmount = cupon.MinimumAmount,
                IsActive = cupon.IsActive
            };

            // Convert existing image to base64 for preview
            if (cupon.CuponPicture != null && cupon.CuponPicture.Length > 0)
            {
                vm.ExistingImageBase64 = Convert.ToBase64String(cupon.CuponPicture);
            }

            // Fixed: Changed "CoponTypes" to "CuponTypes" to match ViewBag name
            ViewBag.CuponTypes = new SelectList(Enum.GetValues(typeof(CuponType))
                .Cast<CuponType>()
                .Select(e => new { Value = e, Text = e.ToString() }),
                "Value", "Text", cupon.Type);

            return View(vm);
        }

        // POST: Admin/Cupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CuponVm vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            // Remove validation for ExistingImageBase64
            ModelState.Remove("ExistingImageBase64");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCupon = await _context.Coupons.FindAsync(id);

                    if (existingCupon == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload (replace if new image provided)
                    if (vm.CuponImageFile != null && vm.CuponImageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await vm.CuponImageFile.CopyToAsync(memoryStream);
                            existingCupon.CuponPicture = memoryStream.ToArray();
                        }
                    }
                    // If no new image, keep the existing one (no change needed)

                    // Update properties
                    existingCupon.Title = vm.Title;
                    existingCupon.Type = vm.Type;
                    existingCupon.Discount = vm.Discount;
                    existingCupon.MinimumAmount = vm.MinimumAmount;
                    existingCupon.IsActive = vm.IsActive;

                    _context.Update(existingCupon);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cupon updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuponExists(vm.Id))
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

            // Reload ViewBag if model is invalid
            ViewBag.CuponTypes = new SelectList(Enum.GetValues(typeof(CuponType))
                .Cast<CuponType>()
                .Select(e => new { Value = e, Text = e.ToString() }),
                "Value", "Text", vm.Type);

            return View(vm);
        }

        // GET: Admin/Cupon/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var cupon = await _context.Coupons.FindAsync(id);

            if (cupon == null)
            {
                return NotFound();
            }

            if (cupon.CuponPicture != null && cupon.CuponPicture.Length > 0)
            {
                ViewBag.ImageBase64 = Convert.ToBase64String(cupon.CuponPicture);
            }

            return View(cupon);
        }

        // POST: Admin/Cupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupon = await _context.Coupons.FindAsync(id);

            if (cupon != null)
            {
                _context.Coupons.Remove(cupon);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cupon deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Toggle Active Status (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var cupon = await _context.Coupons.FindAsync(id);
            if (cupon == null)
            {
                return Json(new { success = false, message = "Cupon not found" });
            }

            cupon.IsActive = !cupon.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = cupon.IsActive });
        }

        private bool CuponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }
}