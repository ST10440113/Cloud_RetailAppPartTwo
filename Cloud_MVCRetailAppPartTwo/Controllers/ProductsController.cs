using Cloud_MVCRetailAppPartTwo.Models;
using Cloud_MVCRetailAppPartTwo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cloud_MVCRetailAppPartTwo.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableService _svc;
        private readonly BlobService _blobService;
        private readonly QueueService _queueService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;


        public ProductsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, TableService table, BlobService blobService, QueueService queueService)
        {
            _svc = table;
            _blobService = blobService;
            _queueService = queueService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString)
        {
            var products = await _svc.ListProductsAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => (p.ProductName != null && p.ProductName.ToUpper().Contains(searchString.ToUpper()))).ToList();
            }

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var details = await _svc.GetProductAsync(partitionKey, rowKey);
            return View(details);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create       
        [HttpPost]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Description,Price,StockQuantity")] IFormFile file, Product product)
        {

            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["BlobFunctionApi:BaseUrl"];
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(product.ProductName), "ProductName");
            formData.Add(new StringContent(product.Description), "Description");
            formData.Add(new StringContent(product.Price), "Price");
            formData.Add(new StringContent(product.StockQuantity.ToString()), "StockQuantity");

            if (file != null && file.Length > 0)
            {
                formData.Add(new StreamContent(file.OpenReadStream()), "ImageUrl", file.FileName);
            }
            var httpResponseMessage = await httpClient.PostAsync($"{apiBaseUrl}products", formData);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var statusCode = httpResponseMessage.StatusCode;
                var errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"API Error: {statusCode} - {errorContent}");
                return View(product);
            }

        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }
            var oldProduct = await _svc.GetProductAsync(partitionKey, rowKey);

            if (oldProduct == null)
            {
                return NotFound();
            }

            return View(oldProduct);
        }


        // POST: Products/Edit/5 
        // To protect from overposting attacks, enable the specific properties you want to bind to. 
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile? file, [Bind("ProductId,ProductName,Description,Price,StockQuantity,PartitionKey,RowKey")] Product product)
        {
            if (product == null || string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                return NotFound();
            }


            var oldProduct = await _svc.GetProductAsync(product.PartitionKey, product.RowKey);

            if (oldProduct == null)
            {
                return NotFound();
            }

            if (file != null && file.Length > 0)
            {
                await _blobService.EditAsync(file, product);
            }
            else
            {
                product.ImageUrl = oldProduct.ImageUrl;
            }
            await _svc.UpdateProductAsync(product);
            return RedirectToAction(nameof(Index));
        }







        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            //get product to delete
            var p = await _svc.GetProductAsync(partitionKey, rowKey);

            if (p == null)
            {
                return NotFound();
            }

            if (p != null && !string.IsNullOrEmpty(p.ImageUrl))
            {
                await _blobService.DeleteAsync(p);
            }
            await _svc.DeleteProductAsync(partitionKey, rowKey);

            return RedirectToAction(nameof(Index));
        }

    }

}


