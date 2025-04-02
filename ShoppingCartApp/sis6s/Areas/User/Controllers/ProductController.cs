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

        public async Task<IActionResult> Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortBy)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Filter by search string
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice);
            }

            // Apply sorting
            switch (sortBy)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Load categories for filter dropdown
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);

            // Store filter values in ViewData for maintaining state
            ViewData["SearchString"] = searchString;
            ViewData["CategoryId"] = categoryId;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["SortBy"] = sortBy;

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
