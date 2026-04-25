using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace FastsFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get Categories (First 4)
                var categories = await _context.Categories
                    .Take(4)
                    .ToListAsync();

                // Get Popular Items (First 8) - সব আইটেম দেখাবে
                var popularItems = await _context.Items
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .OrderByDescending(x => x.Id)
                    .Take(8)
                    .ToListAsync();

                // Get Today's Special Items (First 4)
                var todaySpecial = await _context.Items
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(4)
                    .ToListAsync();

                // Get Hero/Featured Items
                var heroItems = await _context.Items
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .Where(x => x.Category.Title == "Featured" || x.Title.Contains("Special"))
                    .Take(6)
                    .ToListAsync();

                // যদি কোন Hero Items না থাকে তাহলে সব থেকে প্রথম আইটেম দেখাবে
                if (heroItems == null || !heroItems.Any())
                {
                    heroItems = await _context.Items
                        .Include(x => x.Category)
                        .Include(x => x.SubCategory)
                        .Take(3)
                        .ToListAsync();
                }

                var viewModel = new HomeViewModel
                {
                    Categories = categories ?? new List<Category>(),
                    PopularItems = popularItems ?? new List<Item>(),
                    TodaySpecial = todaySpecial ?? new List<Item>(),
                    HeroItems = heroItems ?? new List<Item>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                var emptyViewModel = new HomeViewModel
                {
                    Categories = new List<Category>(),
                    PopularItems = new List<Item>(),
                    TodaySpecial = new List<Item>(),
                    HeroItems = new List<Item>()
                };
                return View(emptyViewModel);
            }
        }

        // GET: Home/Menu
        public async Task<IActionResult> Menu(int? categoryId, int? subCategoryId, string searchTerm)
        {
            var query = _context.Items
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .AsQueryable();

            // Apply filters
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (subCategoryId.HasValue && subCategoryId > 0)
            {
                query = query.Where(x => x.SubCategoryId == subCategoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.Title.Contains(searchTerm) ||
                                        (x.Description != null && x.Description.Contains(searchTerm)));
            }

            var items = await query.ToListAsync();

            var viewModel = new MenuViewModel
            {
                Items = items,
                Categories = await _context.Categories.ToListAsync(),
                SelectedCategoryId = categoryId,
                SelectedSubCategoryId = subCategoryId,
                SearchTerm = searchTerm
            };

            // Get subcategories if category selected
            if (categoryId.HasValue)
            {
                viewModel.SubCategories = await _context.SubCategories
                    .Where(x => x.CategoryId == categoryId.Value)
                    .ToListAsync();
            }

            return View(viewModel);
        }

        // GET: Home/Details/5
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

            // Get related items (same category)
            var relatedItems = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.CategoryId == item.CategoryId && x.Id != id)
                .Take(4)
                .ToListAsync();

            var viewModel = new ItemDetailsViewModel
            {
                Item = item,
                RelatedItems = relatedItems,
                IsInCart = User.Identity.IsAuthenticated && await IsInCart(id)
            };

            return View(viewModel);
        }

        // GET: Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["Success"] = "Thank you for contacting us! We'll get back to you soon.";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }

        // Helper method to check if item is in cart
        private async Task<bool> IsInCart(int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false;

            return await _context.Carts
                .AnyAsync(x => x.ItemId == itemId && x.ApplicationUserId == userId);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}