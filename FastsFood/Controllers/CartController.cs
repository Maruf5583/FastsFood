using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace FastsFood.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart/Index
        public async Task<IActionResult> Index()
        {
            var cartItems = new List<Cart>();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                cartItems = await _context.Carts
                    .Include(x => x.Item)
                    .ThenInclude(x => x.Category)
                    .Where(x => x.ApplicationUserId == userId)
                    .ToListAsync();
            }
            else
            {
                var sessionCart = GetSessionCart();
                foreach (var item in sessionCart)
                {
                    var dbItem = await _context.Items.FindAsync(item.ItemId);
                    if (dbItem != null)
                    {
                        cartItems.Add(new Cart
                        {
                            ItemId = item.ItemId,
                            Count = item.Quantity,
                            Item = dbItem
                        });
                    }
                }
            }

            ViewBag.Total = cartItems.Sum(x => x.Item.Price * x.Count);
            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int itemId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
                return Json(new { success = false, message = "Item not found" });

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingCart = await _context.Carts
                    .FirstOrDefaultAsync(x => x.ItemId == itemId && x.ApplicationUserId == userId);

                if (existingCart != null)
                {
                    existingCart.Count += quantity;
                    _context.Update(existingCart);
                }
                else
                {
                    var cart = new Cart
                    {
                        ItemId = itemId,
                        ApplicationUserId = userId,
                        Count = quantity
                    };
                    await _context.Carts.AddAsync(cart);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var cart = GetSessionCart();
                var existingItem = cart.FirstOrDefault(x => x.ItemId == itemId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ItemId = itemId,
                        Title = item.Title ?? "",
                        ImageUrl = item.ImageUrl ?? "",
                        Price = item.Price,
                        Quantity = quantity
                    });
                }
                SaveSessionCart(cart);
            }

            var cartCount = await GetCartCountAsync();
            return Json(new { success = true, count = cartCount, message = "Item added to cart!" });
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            if (quantity < 1)
                return Json(new { success = false, message = "Quantity must be at least 1" });

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(x => x.ItemId == itemId && x.ApplicationUserId == userId);

                if (cart != null)
                {
                    cart.Count = quantity;
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var cart = GetSessionCart();
                var item = cart.FirstOrDefault(x => x.ItemId == itemId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    SaveSessionCart(cart);
                }
            }

            var total = await GetCartTotalAsync();
            var cartCount = await GetCartCountAsync();
            return Json(new { success = true, total = total, count = cartCount });
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(x => x.ItemId == itemId && x.ApplicationUserId == userId);

                if (cart != null)
                {
                    _context.Carts.Remove(cart);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var cart = GetSessionCart();
                var item = cart.FirstOrDefault(x => x.ItemId == itemId);
                if (item != null)
                {
                    cart.Remove(item);
                    SaveSessionCart(cart);
                }
            }

            var total = await GetCartTotalAsync();
            var cartCount = await GetCartCountAsync();
            return Json(new { success = true, total = total, count = cartCount });
        }

        // GET: Cart/GetCartCount
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var count = await GetCartCountAsync();
            return Json(count);
        }

        // GET: Cart/GetCartTotal
        [HttpGet]
        public async Task<IActionResult> GetCartTotal()
        {
            var total = await GetCartTotalAsync();
            return Json(total);
        }

        // GET: Cart/GetCartItems
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var cartItems = new List<CartItemVm>();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var items = await _context.Carts
                    .Include(x => x.Item)
                    .Where(x => x.ApplicationUserId == userId)
                    .ToListAsync();

                cartItems = items.Select(x => new CartItemVm
                {
                    ItemId = x.ItemId,
                    Title = x.Item.Title ?? "",
                    ImageUrl = x.Item.ImageUrl ?? "",
                    Price = x.Item.Price,
                    Quantity = x.Count
                }).ToList();
            }
            else
            {
                var sessionCart = GetSessionCart();
                cartItems = sessionCart.Select(x => new CartItemVm
                {
                    ItemId = x.ItemId,
                    Title = x.Title ?? "",
                    ImageUrl = x.ImageUrl ?? "",
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList();
            }

            return Json(cartItems);
        }

        // ── Helper Methods ────────────────────────────────────────────────

        private async Task<int> GetCartCountAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return await _context.Carts
                    .Where(x => x.ApplicationUserId == userId)
                    .SumAsync(x => x.Count);
            }
            else
            {
                var sessionCart = GetSessionCart();
                return sessionCart.Sum(x => x.Quantity);
            }
        }

        private async Task<double> GetCartTotalAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cartItems = await _context.Carts
                    .Include(x => x.Item)
                    .Where(x => x.ApplicationUserId == userId)
                    .ToListAsync();
                return cartItems.Sum(x => x.Item.Price * x.Count);
            }
            else
            {
                var sessionCart = GetSessionCart();
                return sessionCart.Sum(x => x.Total);
            }
        }

        // ── Session Methods ───────────────────────────────────────────────

        private const string CartSessionKey = "ShoppingCart";

        private List<CartItem> GetSessionCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(cartJson)
                   ?? new List<CartItem>();
        }

        private void SaveSessionCart(List<CartItem> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }
    }
}