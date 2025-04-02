using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sis6s.Data;
using sis6s.Models;
using sis6s.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace sis6s.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVNPayService _vnPayService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ApplicationDbContext context,
            IVNPayService vnPayService,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _vnPayService = vnPayService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var response = _vnPayService.ProcessPaymentResponse(Request.Query);
                
                // Sử dụng transaction để đảm bảo tính nhất quán của dữ liệu
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                   
                    var orderId = int.Parse(response.OrderId);
                    var donHang = await _context.Orders
                        .Include(d => d.OrderItems)
                            .ThenInclude(c => c.Product)
                        .FirstOrDefaultAsync(d => d.Id == orderId);

                    if (donHang == null)
                    {
                        _logger.LogWarning($"Không tìm thấy đơn hàng với ID: {response.OrderId}");
                        TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                        return RedirectToAction("Index", "DonHang");
                    }

                    if (response.ResponseCode == "00")
                    {
                        // Cập nhật trạng thái đơn hàng
                        donHang.Status = OrderStatus.Processing;
                        donHang.UpdatedAt = DateTime.Now;
                        
                        // Lưu thông tin thanh toán
                        var payment = new Payment
                        {
                            OrderId = donHang.Id,
                            TransactionId = response.TransactionId,
                            Amount = donHang.TotalAmount,
                            PaymentDate = DateTime.Now,
                            PaymentMethod = "VNPay",
                            Status = "Success",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        
                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();
                        
                        await transaction.CommitAsync();
                        
                        TempData["Success"] = $"Thanh toán đơn hàng #{donHang.Id} thành công!";
                    }
                    else
                    {
                        // Cập nhật trạng thái đơn hàng thất bại
                        donHang.Status = OrderStatus.Cancelled;
                        donHang.UpdatedAt = DateTime.Now;
                        
                        // Lưu thông tin thanh toán thất bại
                        var payment = new Payment
                        {
                            OrderId = donHang.Id,
                            TransactionId = response.TransactionId ?? "N/A",
                            Amount = donHang.TotalAmount,
                            PaymentDate = DateTime.Now,
                            PaymentMethod = "VNPay",
                            Status = "Failed",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        
                        _context.Payments.Add(payment);
                        
                        // Hoàn trả số lượng sản phẩm về kho
                        foreach (var item in donHang.OrderItems)
                        {
                            item.Product.Stock += item.Quantity;
                        }
                        
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        TempData["Error"] = $"Thanh toán thất bại: {response.Message}";
                    }

                    return RedirectToAction("Index", "DonHang");
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi xử lý callback thanh toán");
                    TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message;
                    return RedirectToAction("Index", "DonHang");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định trong quá trình xử lý callback thanh toán");
                TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "DonHang");
            }
        }
    }
} 