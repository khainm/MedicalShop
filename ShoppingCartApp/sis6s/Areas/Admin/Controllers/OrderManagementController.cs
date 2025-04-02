using Microsoft.AspNetCore.Mvc;
using sis6s.Data;
using sis6s.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace sis6s.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DonHang
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        // GET: Admin/DonHang/ChuaXuLy
        public async Task<IActionResult> ChuaXuLy()
        {
            var orders = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .Where(d => d.Status == OrderStatus.Pending || 
                            d.Status == OrderStatus.Processing)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            
            ViewBag.TieuDe = "Đơn hàng chưa xử lý";
            ViewBag.TrangThai = "ChuaXuLy";
            return View("Index", orders);
        }

        // GET: Admin/DonHang/DangXuLy
        public async Task<IActionResult> DangXuLy()
        {
            var orders = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .Where(d => d.Status == OrderStatus.Processing)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            
            ViewBag.TieuDe = "Đơn hàng đang xử lý";
            ViewBag.TrangThai = "DangXuLy";
            return View("Index", orders);
        }

        // GET: Admin/DonHang/DangGiao
        public async Task<IActionResult> DangGiao()
        {
            var orders = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .Where(d => d.Status == OrderStatus.Shipped || 
                            d.Status == OrderStatus.Processing)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            
            ViewBag.TieuDe = "Đơn hàng đang giao";
            ViewBag.TrangThai = "DangGiao";
            return View("Index", orders);
        }

        // GET: Admin/DonHang/DaGiao
        public async Task<IActionResult> DaGiao()
        {
            var orders = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .Where(d => d.Status == OrderStatus.Delivered || 
                            d.Status == OrderStatus.Processing)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            
            ViewBag.TieuDe = "Đơn hàng đã giao";
            ViewBag.TrangThai = "DaGiao";
            return View("Index", orders);
        }

        // GET: Admin/DonHang/DaHuy
        public async Task<IActionResult> DaHuy()
        {
            var orders = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .Where(d => d.Status == OrderStatus.Cancelled)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            
            ViewBag.TieuDe = "Đơn hàng đã hủy/thất bại";
            ViewBag.TrangThai = "DaHuy";
            return View("Index", orders);
        }

        // POST: Admin/DonHang/CapNhatTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/DonHang/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // GET: Admin/DonHang/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Admin/DonHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingOrder = await _context.Orders.FindAsync(id);
                    if (existingOrder == null) return NotFound();

                    existingOrder.Status = order.Status;
                    existingOrder.UpdatedAt = DateTime.Now;

                    _context.Update(existingOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Orders.Any(e => e.Id == order.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // GET: Admin/DonHang/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(d => d.User)
                .Include(d => d.OrderItems)
                    .ThenInclude(ct => ct.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (order == null) return NotFound();
                return View(order);
        }

        // POST: Admin/DonHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(d => d.OrderItems)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 