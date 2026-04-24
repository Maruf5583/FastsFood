using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastsFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Category/Index
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Title)
                .ToListAsync();

            var viewModel = categories.Select(c => new CategoryVM
            {
                Id = c.Id,
                Title = c.Title
            }).ToList();

            return View(viewModel);
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View(new CategoryVM());
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var category = new Category
            {
                Title = vm.Title.Trim()
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            var vm = new CategoryVM
            {
                Id = category.Id,
                Title = category.Title
            };

            return View(vm);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var category = await _context.Categories.FindAsync(vm.Id);
            if (category == null)
                return NotFound();

            category.Title = vm.Title.Trim();

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var vm = new CategoryVM
            {
                Id = category.Id,
                Title = category.Title
            };

            return View(vm);
        }
    }
}