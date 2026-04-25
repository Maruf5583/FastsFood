using FastsFood.Models;
using FastsFood.Repository;
using FastsFood.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace FastsFood.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<OrderController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // ── GET: Order/Checkout ───────────────────────────────────────────

        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItems = await _context.Carts
                .Include(x => x.Item)
                .Where(x => x.ApplicationUserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _context.Users.FindAsync(userId);
            var subTotal = cartItems.Sum(x => x.Item.Price * x.Count);
            var deliveryFee = 60.0;
            var total = subTotal + deliveryFee;

            var vm = new CheckoutVm
            {
                Name = user?.Name ?? "",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                City = user?.City ?? "",
                DeliveryAddress = user?.Address ?? "",
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                Total = total
            };

            return View(vm);
        }

        // ── POST: Order/Checkout (AJAX JSON) ─────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([FromBody] CheckoutVm vm)
        {
            if (vm == null)
                return Json(new { success = false, message = "Invalid request data." });

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Please login to place order." });

                // ── 1. Load cart ──────────────────────────────────────────
                var cartItems = await _context.Carts
                    .Include(x => x.Item)
                    .Where(x => x.ApplicationUserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                    return Json(new { success = false, message = "Your cart is empty!" });

                // ── 2. Totals ─────────────────────────────────────────────
                var subTotal = cartItems.Sum(x => x.Item.Price * x.Count);
                var deliveryFee = 60.0;
                var discount = vm.Discount > 0 ? vm.Discount : 0;
                var total = subTotal + deliveryFee - discount;

                // ── 3. Create OrderHeader ─────────────────────────────────
                var orderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
                    OrderDate = DateTime.Now,
                    DateofPick = DateTime.Now,
                    TimeofPick = DateTime.Now.AddHours(1),
                    Name = (vm.Name ?? "").Trim(),
                    PhoneNumber = (vm.PhoneNumber ?? "").Trim(),
                    SubtTotal = subTotal,
                    OrderTotal = total,
                    CouponCode = vm.CouponCode ?? "",
                    Coupondiscount = discount,
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    TransactionId = ""
                };

                _context.OrderHeaders.Add(orderHeader);
                await _context.SaveChangesAsync();   // Id পাওয়ার জন্য আগেই save

                // ── 4. Create OrderDetails ────────────────────────────────
                foreach (var ci in cartItems)
                {
                    _context.OrderDetails.Add(new OrderDetails
                    {
                        OrderHeaderId = orderHeader.Id,
                        ItemId = ci.ItemId,
                        Count = ci.Count,
                        Name = ci.Item.Title ?? "Item",
                        Description = ci.Item.Description ?? "",   // null safe
                        Price = ci.Item.Price
                    });
                }
                await _context.SaveChangesAsync();

                // ── 5. Clear Cart ─────────────────────────────────────────
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                // ── 6. Payment routing ────────────────────────────────────
                var method = (vm.PaymentMethod ?? "CashOnDelivery").Trim();

                if (method == "SSLCommerz")
                {
                    return await ProcessSSLCommerzPayment(orderHeader, vm);
                }
                else if (method == "FakePayment")
                {
                    orderHeader.PaymentStatus = "Paid (Test)";
                    orderHeader.OrderStatus = "Confirmed";
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, redirectUrl = $"/Order/Confirmation/{orderHeader.Id}" });
                }
                else // CashOnDelivery (default)
                {
                    orderHeader.PaymentStatus = "Cash on Delivery";
                    orderHeader.OrderStatus = "Confirmed";
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, redirectUrl = $"/Order/Confirmation/{orderHeader.Id}" });
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Database-specific inner exception বের করে দেখায়
                var inner = dbEx.InnerException?.InnerException?.Message
                         ?? dbEx.InnerException?.Message
                         ?? dbEx.Message;

                _logger.LogError(dbEx, "DB error during checkout: {Inner}", inner);
                return Json(new { success = false, message = "Database error: " + inner });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout error: {Message}", ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ── SSLCommerz ────────────────────────────────────────────────────

        private async Task<IActionResult> ProcessSSLCommerzPayment(OrderHeader order, CheckoutVm vm)
        {
            var storeId = _configuration["SSLCommerz:StoreId"] ?? "testbox";
            var storePassword = _configuration["SSLCommerz:StorePassword"] ?? "qwerty123";
            var isSandbox = true;

            var transactionId = "FAST" + DateTime.Now.Ticks;
            order.TransactionId = transactionId;
            await _context.SaveChangesAsync();

            var baseAppUrl = $"{Request.Scheme}://{Request.Host}";

            var postData = new Dictionary<string, string>
            {
                ["store_id"] = storeId,
                ["store_passwd"] = storePassword,
                ["total_amount"] = order.OrderTotal.ToString("F2"),
                ["currency"] = "BDT",
                ["tran_id"] = transactionId,
                ["success_url"] = $"{baseAppUrl}/Order/PaymentSuccess?orderId={order.Id}&tran_id={transactionId}",
                ["fail_url"] = $"{baseAppUrl}/Order/PaymentFailed?orderId={order.Id}",
                ["cancel_url"] = $"{baseAppUrl}/Order/PaymentCancel?orderId={order.Id}",
                ["ipn_url"] = $"{baseAppUrl}/Order/PaymentIPN?orderId={order.Id}",
                ["cus_name"] = vm.Name ?? "Customer",
                ["cus_email"] = vm.Email ?? "customer@example.com",
                ["cus_phone"] = vm.PhoneNumber ?? "0000000000",
                ["cus_add1"] = vm.DeliveryAddress ?? "N/A",
                ["cus_city"] = vm.City ?? "Dhaka",
                ["cus_country"] = "Bangladesh",
                ["shipping_method"] = "YES",
                ["product_name"] = "Food Order #" + order.Id,
                ["product_category"] = "Food",
                ["product_profile"] = "general"
            };

            try
            {
                var gatewayBase = isSandbox
                    ? "https://sandbox.sslcommerz.com"
                    : "https://secure.sslcommerz.com";

                using var client = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(postData);
                var response = await client.PostAsync($"{gatewayBase}/gwprocess/v4/api.php", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                if (responseData != null
                    && responseData.TryGetValue("status", out var status)
                    && status == "SUCCESS"
                    && responseData.TryGetValue("GatewayPageURL", out var gatewayUrl))
                {
                    return Json(new { success = true, redirectUrl = gatewayUrl });
                }

                _logger.LogError("SSLCommerz failed: {Resp}", responseString);
                return Json(new { success = false, message = "Payment gateway error. Please try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz exception");
                return Json(new { success = false, message = "Payment gateway unreachable. Try another method." });
            }
        }

        // ── Payment Callbacks ─────────────────────────────────────────────

        public async Task<IActionResult> PaymentSuccess(int orderId, string tran_id)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.PaymentStatus = "Paid";
            order.OrderStatus = "Confirmed";
            order.TransactionId = tran_id ?? "";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successful! Your order is confirmed.";
            return RedirectToAction("Confirmation", new { id = orderId });
        }

        public async Task<IActionResult> PaymentFailed(int orderId)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = "Failed";
                order.OrderStatus = "Cancelled";
                await _context.SaveChangesAsync();
            }
            TempData["Error"] = "Payment failed. Please try again.";
            return RedirectToAction("Checkout");
        }

        public async Task<IActionResult> PaymentCancel(int orderId)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = "Cancelled";
                await _context.SaveChangesAsync();
            }
            TempData["Warning"] = "Payment was cancelled.";
            return RedirectToAction("Checkout");
        }

        [HttpPost]
        public async Task<IActionResult> PaymentIPN(int orderId)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order != null && order.PaymentStatus != "Paid")
            {
                order.PaymentStatus = "Paid";
                order.OrderStatus = "Confirmed";
                await _context.SaveChangesAsync();
                _logger.LogInformation("IPN: Order {Id} confirmed", orderId);
            }
            return Ok();
        }

        // ── Confirmation ──────────────────────────────────────────────────

        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.OrderHeaders
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.ApplicationUserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var vm = new OrderConfirmationVm
            {
                OrderId = order.Id,
                OrderNumber = "ORD-" + order.Id.ToString("D6"),
                OrderDate = order.OrderDate,
                Total = order.OrderTotal,
                PaymentStatus = order.PaymentStatus,
                TransactionId = order.TransactionId,
                Items = order.OrderDetails.Select(x => new OrderItemVm
                {
                    Title = x.Name,
                    Quantity = x.Count,
                    Price = x.Price,
                    Total = x.Price * x.Count
                }).ToList()
            };

            return View(vm);
        }

        // ── My Orders ─────────────────────────────────────────────────────

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.OrderHeaders
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.OrderHeaders
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> TrackOrder(int orderId)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order == null) return NotFound();
            return View(order);
        }

        // ── Admin ─────────────────────────────────────────────────────────

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.OrderHeaders
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order == null)
                return Json(new { success = false, message = "Order not found" });

            order.OrderStatus = status;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Order status updated" });
        }
    }
}