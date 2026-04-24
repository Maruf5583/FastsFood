using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel.FastsFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FastsFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Item
        public IActionResult Index()
        {
            var items = _context.Items
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .ToList();
            return View(items);
        }

        // GET: Admin/Item/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Admin/Item/Create
        public IActionResult Create()
        {
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Title");
            ViewBag.SubCategory = new SelectList(_context.SubCategories.ToList(), "Id", "Title");
            return View();
        }

        // POST: Admin/Item/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemVm vm)
        {
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                Item model = new Item();

                // Image Handling
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Items");

                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(vm.ImageFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.ImageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = "/Images/Items/" + fileName;
                }

                // Mapping properties
                model.Title = vm.Title;
                model.Price = vm.Price;
                model.Description = vm.Description;
                model.CategoryId = vm.CategoryId;
                model.SubCategoryId = vm.SubCategoryId;

                _context.Items.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // If Model Invalid, reload dropdowns
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Title", vm.CategoryId);
            ViewBag.SubCategory = new SelectList(_context.SubCategories.ToList(), "Id", "Title", vm.SubCategoryId);

            return View(vm);
        }

        // GET: Admin/Item/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            ItemVm vm = new ItemVm
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                Price = item.Price,
                CategoryId = item.CategoryId,
                SubCategoryId = item.SubCategoryId
            };

            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Title", vm.CategoryId);
            ViewBag.SubCategory = new SelectList(_context.SubCategories.Where(x => x.CategoryId == vm.CategoryId).ToList(), "Id", "Title", vm.SubCategoryId);

            return View(vm);
        }

        // POST: Admin/Item/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemVm vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.Items.FindAsync(id);

                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload
                    if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingItem.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingItem.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Upload new image
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Items");
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(vm.ImageFile.FileName);
                        string filePath = Path.Combine(uploadDir, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await vm.ImageFile.CopyToAsync(fileStream);
                        }

                        existingItem.ImageUrl = "/Images/Items/" + fileName;
                    }

                    // Update properties
                    existingItem.Title = vm.Title;
                    existingItem.Description = vm.Description;
                    existingItem.Price = vm.Price;
                    existingItem.CategoryId = vm.CategoryId;
                    existingItem.SubCategoryId = vm.SubCategoryId;

                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(vm.Id))
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

            // Reload dropdowns if validation fails
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Title", vm.CategoryId);
            ViewBag.SubCategory = new SelectList(_context.SubCategories.Where(x => x.CategoryId == vm.CategoryId).ToList(), "Id", "Title", vm.SubCategoryId);

            return View(vm);
        }

        // GET: Admin/Item/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Admin/Item/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item != null)
            {
                // Delete image file if exists
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get SubCategories by CategoryId
        [HttpGet]
        public IActionResult GetSubCategory(int CategoryId)
        {
            var subCategories = _context.SubCategories
                .Where(x => x.CategoryId == CategoryId)
                .Select(x => new { id = x.Id, title = x.Title })
                .ToList();

            return Json(subCategories);
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}