using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sis6s.Data;
using System.Threading.Tasks;

namespace sis6s.Areas.User.Controllers
{
    [Area("User")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "gia_tang":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "gia_giam":
                    products = products.OrderByDescending(p => p.Price);
                    break;
            }

            ViewData["SearchString"] = searchString;
            ViewData["SortOrder"] = sortOrder;

            // Tạo SelectList cho dropdown giá
            ViewBag.SortOptions = new List<SelectListItem>
    {
        new SelectListItem { Text = "-- Sắp xếp theo giá --", Value = "" },
        new SelectListItem { Text = "Giá tăng dần", Value = "gia_tang", Selected = sortOrder == "gia_tang" },
        new SelectListItem { Text = "Giá giảm dần", Value = "gia_giam", Selected = sortOrder == "gia_giam" }
    };

            return View(await products.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
