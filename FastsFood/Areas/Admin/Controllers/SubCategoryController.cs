using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FastsFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/SubCategory/Index
        public async Task<IActionResult> Index()
        {
            var subCategories = await _context.SubCategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category.Title)
                .ThenBy(s => s.Title)
                .ToListAsync();

            var viewModel = subCategories.Select(s => new SubCategoryVm
            {
                Id = s.Id,
                Title = s.Title,
                CategoryId = s.CategoryId,
                CategoryName = s.Category?.Title
            }).ToList();

            return View(viewModel);
        }

        // GET: Admin/SubCategory/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryList = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            return View(new SubCategoryVm());
        }

        // POST: Admin/SubCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryVm vm)
        {
            // Remove CategoryName from validation (display only field)
            ModelState.Remove("CategoryName");

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title", vm.CategoryId);
                return View(vm);
            }

            var subCategory = new SubCategory
            {
                Title = vm.Title.Trim(),
                CategoryId = vm.CategoryId
            };

            await _context.SubCategories.AddAsync(subCategory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "SubCategory created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/SubCategory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound();

            var vm = new SubCategoryVm
            {
                Id = subCategory.Id,
                Title = subCategory.Title,
                CategoryId = subCategory.CategoryId
            };

            ViewBag.CategoryList = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title", vm.CategoryId);
            return View(vm);
        }

        // POST: Admin/SubCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryVm vm)
        {
            // Remove CategoryName from validation (display only field)
            ModelState.Remove("CategoryName");

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title", vm.CategoryId);
                return View(vm);
            }

            var subCategory = await _context.SubCategories.FindAsync(vm.Id);
            if (subCategory == null)
                return NotFound();

            subCategory.Title = vm.Title.Trim();
            subCategory.CategoryId = vm.CategoryId;

            _context.SubCategories.Update(subCategory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "SubCategory updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/SubCategory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound();

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "SubCategory deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/SubCategory/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var subCategory = await _context.SubCategories
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subCategory == null)
                return NotFound();

            var vm = new SubCategoryVm
            {
                Id = subCategory.Id,
                Title = subCategory.Title,
                CategoryId = subCategory.CategoryId,
                CategoryName = subCategory.Category?.Title
            };

            return View(vm);
        }
    }
}