using Microsoft.AspNetCore.Mvc;
using sis6s.Data;
using sis6s.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace sis6s.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductManagementController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/SanPham
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(s => s.Category)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            ViewBag.DanhSachTheLoai = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/SanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? fileImage)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (fileImage != null && fileImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "hinhanh");
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileImage.CopyToAsync(stream);
                    }

                    product.Image = uniqueFileName;
                }

                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DanhSachTheLoai = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/SanPham/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (product == null) return NotFound();

            ViewBag.DanhSachTheLoai = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? fileImage, string? image)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null) return NotFound();

                    // Xử lý upload ảnh mới
                    if (fileImage != null && fileImage.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "hinhanh");
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileImage.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fileImage.CopyToAsync(stream);
                        }

                        // Nếu có ảnh cũ thì xoá
                        if (!string.IsNullOrEmpty(image))
                        {
                            string oldFilePath = Path.Combine(uploadsFolder, image);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        existingProduct.Image = uniqueFileName;
                    }
                    else
                    {
                        existingProduct.Image = image;
                    }

                    // Cập nhật các thông tin khác
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Unit = product.Unit;
                    existingProduct.Stock = product.Stock;
                    existingProduct.DiscountPercentage = product.DiscountPercentage;
                    existingProduct.UpdatedAt = DateTime.Now;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DanhSachTheLoai = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/SanPham/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(s => s.Category)
                .Include(s => s.Carts)
                .Include(s => s.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/SanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(s => s.Carts)
                .Include(s => s.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product != null)
            {
                if (product.Carts.Any() || product.OrderItems.Any())
                {
                    TempData["Error"] = "Không thể xóa sản phẩm này vì đang có trong giỏ hàng hoặc đơn hàng.";
                    return RedirectToAction(nameof(Index));
                }

                // Xoá file ảnh nếu có
                if (!string.IsNullOrEmpty(product.Image))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "hinhanh", product.Image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
