using CustomerBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerBox.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Customers.ToListAsync();

            return View(categories);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Customer customer)
        {

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            //Aynı isimde ürün var mı kontrol et
            bool customerExists = await _context.Customers.AnyAsync(p => p.FullName.ToLower() == customer.FullName.ToLower());

            if (customerExists)
            {
                ModelState.AddModelError("FullName", "Bu kullanıcı zaten mevcut.");
                return View(customer);
            }
           

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();


            TempData["Message"] = "Customer Added Successfully.";
            return RedirectToAction("Index", "Customer", new { area = "Admin" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Customer deleted successfully.";
            return RedirectToAction("Index", "Customer", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var existingCustomer = await _context.Customers.FindAsync(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            existingCustomer.FullName = customer.FullName;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Description = customer.Description;

            await _context.SaveChangesAsync();


            TempData["Message"] = $"Customer updated successfully.";
            return RedirectToAction("Index", "Customer", new { area = "Admin" });
        }

    }
}
