using FastsFood.Models;
using FastsFood.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace FastsFood.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int cartCount = 0;
            double cartTotal = 0;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cartItems = await _context.Carts
                    .Include(x => x.Item)
                    .Where(x => x.ApplicationUserId == userId)
                    .ToListAsync();

                cartCount = cartItems.Sum(x => x.Count);
                cartTotal = cartItems.Sum(x => x.Item.Price * x.Count);
            }
            else
            {
                var session = HttpContext.Session;
                var cartJson = session.GetString("ShoppingCart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
                    cartCount = cartItems.Sum(x => x.Quantity);
                    cartTotal = cartItems.Sum(x => x.Total);
                }
            }

            ViewBag.CartCount = cartCount;
            ViewBag.CartTotal = cartTotal;

            return View();
        }
    }
}