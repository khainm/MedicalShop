﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sis6s.Data;
using System.Linq;
using System.Threading.Tasks;
using sis6s.Models;
using sis6s.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using sis6s.Models;

namespace sis6s.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVNPayService _vnPayService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ApplicationDbContext context,
            IVNPayService vnPayService,
            UserManager<ApplicationUser> userManager,
            ILogger<CartController> logger)
        {
            _context = context;
            _vnPayService = vnPayService;
            _userManager = userManager;
            _logger = logger;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            if (!User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", 
                    new { area = "Identity", returnUrl = Url.Action("Index", "Cart", new { area = "User" }) });
                return;
            }
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            // Ensure CartItems is not null
            var cartItems = cart.CartItems ?? new List<CartItem>();
            
            // Create a tuple with cart items and checkout view model
            var viewModel = (cartItems, new Order());
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(int productId, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity", returnUrl = Url.Action("Details", "Product", new { id = productId }) });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Check user role
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("User"))
            {
                // If user doesn't have the User role, add it
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation($"Added User role to user {user.UserName}");
            }

            // Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Check stock quantity
            if (product.Stock < quantity)
            {
                TempData["Message"] = "Số lượng sản phẩm trong kho không đủ!";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Check user's cart
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                // Create new cart
                cart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if product is already in cart
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ct => ct.CartId == cart.Id && ct.ProductId == productId);

            if (cartItem != null)
            {
                // If exists, update quantity
                cartItem.Quantity += quantity;
                cartItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                // If not, create new
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sản phẩm đã được thêm vào giỏ hàng thành công!";
                return RedirectToAction(nameof(Index), "Cart", new { area = "User" });
            }
            catch (DbUpdateException ex)
            {
                // Log error
                _logger.LogError(ex, "Error adding product to cart");
                TempData["Message"] = "Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng!";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = await _context.CartItems
                .Include(c => c.Cart)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.Cart.UserId == user.Id);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = await _context.CartItems
                .Include(c => c.Cart)
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.Cart.UserId == user.Id);

            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutPost(Order model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
                
                var cartItems = cart?.CartItems ?? new List<CartItem>();
                return View("Index", (cartItems, model));
            }
            
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            // Khởi tạo thanh toán qua VNPay
            var order = new Order
            {
                UserId = userId,
                TotalAmount = model.TotalAmount,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var returnUrl = Url.Action("PaymentCallback", "Cart", new { area = "User" }, protocol: HttpContext.Request.Scheme);
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, returnUrl);

            return Redirect(paymentUrl);
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var query = HttpContext.Request.Query;
            var response = _vnPayService.ProcessPaymentResponse(query);

            if (response.ResponseCode == "00")
            {
                // Cập nhật trạng thái đơn hàng
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == Convert.ToInt32(response.OrderId));
                if (order != null)
                {
                    order.Status = OrderStatus.Paid;
                    await _context.SaveChangesAsync();
                }

                TempData["Message"] = "Thanh toán thành công!";
            }
            else
            {
                TempData["Message"] = "Thanh toán thất bại!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UpdateSelectedItems(List<int> selectedProductIds)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == user.Id)
                .ToListAsync();

            decimal selectedTotal = 0;
            decimal shippingFee = 5; // Phí vận chuyển cố định 5$

            if (selectedProductIds != null && selectedProductIds.Any())
            {
                selectedTotal = cartItems
                    .Where(ci => selectedProductIds.Contains(ci.ProductId))
                    .Sum(ci => ci.Product.Price * ci.Quantity);
            }

            selectedTotal += shippingFee; // Thêm phí vận chuyển vào tổng tiền
            TempData["SelectedTotal"] = selectedTotal.ToString("N0") + " $";
            TempData["ShippingFee"] = shippingFee.ToString("N0") + " $";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckoutGet(Order model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
                
                var cartItems = cart?.CartItems ?? new List<CartItem>();
                return View("Index", (cartItems, model));
            }
            
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            // Rest of checkout logic...
            
            return View();
        }


    }
}


