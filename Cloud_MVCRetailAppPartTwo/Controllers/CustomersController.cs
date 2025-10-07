using Cloud_MVCRetailAppPartTwo.Models;
using Cloud_MVCRetailAppPartTwo.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Cloud_MVCRetailAppPartTwo.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly TableService _svc;
        private readonly QueueService _queueService;

        public CustomersController(IHttpClientFactory httpClientFactory, IConfiguration configuration, TableService svc, QueueService queueService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _svc = svc;
            _queueService = queueService;
        }


        public async Task<IActionResult> Index()
        {
            var customers = await _svc.ListCustomersAsync();
            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var details = await _svc.GetCustomerAsync(partitionKey, rowKey);
            return View(details);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("CustomerId,FirstName,LastName,Email,PhoneNumber,Address")] Customer customer)
        {
            customer.CustomerId = customer.GetHashCode();
            customer.PartitionKey = "CUSTOMER";
            customer.RowKey = Guid.NewGuid().ToString("N");

            // Call service method
            var response = await _svc.AddCustomerAsync(customer);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Server error: {errorContent}");
                return View(customer);
            }
        }

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }
            var oldCustomer = await _svc.GetCustomerAsync(partitionKey, rowKey);

            if (oldCustomer == null)
            {
                return NotFound();
            }

            return View(oldCustomer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("CustomerId,FirstName,LastName,Email,PhoneNumber,Address,PartitionKey,RowKey")] Customer customer)
        {

            if (customer == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _svc.UpdateCustomerAsync(customer);



                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _svc.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }


    }
}

