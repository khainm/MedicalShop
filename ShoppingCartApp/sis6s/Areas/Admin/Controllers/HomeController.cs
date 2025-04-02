using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using sis6s.Data;
using sis6s.Models;
using System.Linq;
using System.Threading.Tasks;

namespace sis6s.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng số
            ViewBag.TongNguoiDung = await _context.Users.CountAsync();
            ViewBag.TongSanPham = await _context.Products.CountAsync();
            ViewBag.TongDonHang = await _context.Orders.CountAsync();
            ViewBag.TongDoanhThu = await _context.Orders
                .Where(d => d.Status == OrderStatus.Delivered || 
                            d.Status == OrderStatus.Shipped || 
                            d.Status == OrderStatus.Processing)
                .SumAsync(d => d.TotalAmount);

            // Đơn hàng mới
            ViewBag.DonHangChuaXuLy = await _context.Orders
                .Where(d => d.Status == OrderStatus.Pending || 
                            d.Status == OrderStatus.Processing)
                .CountAsync();

            // Đơn hàng đang giao
            ViewBag.DonHangDangGiao = await _context.Orders
                .Where(d => d.Status == OrderStatus.Shipped || 
                            d.Status == OrderStatus.Processing)
                .CountAsync();

            // Sản phẩm bán chạy
            var sanPhamBanChay = await _context.OrderItems
                .Include(c => c.Product)
                .GroupBy(c => c.ProductId)
                .Select(g => new {
                    Product = g.First().Product,
                    TotalQuantity = g.Sum(c => c.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            ViewBag.SanPhamBanChay = sanPhamBanChay;

            // Đơn hàng gần đây
            var donHangGanDay = await _context.Orders
                .Include(d => d.User)
                .OrderByDescending(d => d.OrderDate)
                .Take(10)
                .ToListAsync();

            return View(donHangGanDay);
        }
    }
} 