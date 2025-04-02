using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sis6s.Models;

namespace sis6s.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.Migrate();

            // Add Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Add Admin User
            string adminEmail = "admin@admin.com";
            string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin",
                    Address = "123 Admin Street",
                    PhoneNumber = "0123456789"
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Check and create categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Antibiotics", Description = "Various antibiotics" },
                    new Category { Name = "Pain Relief", Description = "Various pain relief medications" },
                    new Category { Name = "Vitamins & Minerals", Description = "Various vitamins and minerals" },
                    new Category { Name = "Medical Devices", Description = "Various medical devices" }
                );
                await context.SaveChangesAsync();
            }

            // Check and create sample products
            if (!context.Products.Any())
            {
                var categories = await context.Categories.ToListAsync();

                foreach (var category in categories)
                {
                    switch (category.Name)
                    {
                        case "Antibiotics":
                            context.Products.Add(new Product
                            {
                                Name = "Amoxicillin 500mg",
                                Price = 50000,
                                Description = "Broad-spectrum antibiotic",
                                Image = "amoxicillin.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "GlaxoSmithKline",
                                Unit = "Pill",
                                Stock = 100
                            });

                            context.Products.Add(new Product
                            {
                                Name = "Cephalexin 500mg",
                                Price = 65000,
                                Description = "Cephalosporin antibiotic",
                                Image = "cephalexin.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Novartis",
                                Unit = "Pill",
                                Stock = 80
                            });
                            break;

                        case "Pain Relief":
                            context.Products.Add(new Product
                            {
                                Name = "Paracetamol 500mg",
                                Price = 30000,
                                Description = "Pain relief and fever reduction",
                                Image = "paracetamol.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Sanofi",
                                Unit = "Pill",
                                Stock = 200
                            });

                            context.Products.Add(new Product
                            {
                                Name = "Ibuprofen 400mg",
                                Price = 45000,
                                Description = "Non-steroidal anti-inflammatory drug",
                                Image = "ibuprofen.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Pfizer",
                                Unit = "Pill",
                                Stock = 150
                            });
                            break;

                        case "Vitamins & Minerals":
                            context.Products.Add(new Product
                            {
                                Name = "Vitamin C 1000mg",
                                Price = 120000,
                                Description = "Vitamin C supplement",
                                Image = "vitaminc.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Bayer",
                                Unit = "Pill",
                                Stock = 100
                            });

                            context.Products.Add(new Product
                            {
                                Name = "Vitamin D3 1000IU",
                                Price = 150000,
                                Description = "Vitamin D supplement",
                                Image = "vitamind.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Roche",
                                Unit = "Pill",
                                Stock = 80
                            });
                            break;

                        case "Medical Devices":
                            context.Products.Add(new Product
                            {
                                Name = "Digital Thermometer",
                                Price = 150000,
                                Description = "Body temperature measurement",
                                Image = "thermometer.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Omron",
                                Unit = "Piece",
                                Stock = 50
                            });

                            context.Products.Add(new Product
                            {
                                Name = "Blood Pressure Monitor",
                                Price = 1200000,
                                Description = "Home blood pressure monitoring",
                                Image = "bpmonitor.jpg",
                                CategoryId = category.Id,
                                Manufacturer = "Omron",
                                Unit = "Piece",
                                Stock = 30
                            });
                            break;
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
