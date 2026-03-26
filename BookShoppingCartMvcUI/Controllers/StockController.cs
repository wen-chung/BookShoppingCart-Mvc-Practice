using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class StockController : Controller
    {
        private readonly IStockRepository _stockRepo;

        public StockController(IStockRepository stockRepo)
        {
            _stockRepo = stockRepo;
        }

        // GET: StockController
        public async Task<IActionResult> Index(string sTerm = "")
        {
            var stock = await _stockRepo.GetStocks(sTerm);
            return View(stock);
        }
        public async Task<IActionResult> ManageStock(int bookId)
        {
            var existingStock = await _stockRepo.GetStockByBookId(bookId);
            var stock = new StockDTO
            {
                BookId = bookId,
                Quantity = existingStock != null ? existingStock.Quantity : 0
            };
            return View(stock);
        }

        [HttpPost]
        public async Task<IActionResult> ManageStock(StockDTO stock)
        {
            if (!ModelState.IsValid) return View(stock);

            try
            {
                await _stockRepo.ManageStock(stock);
                TempData["successMessage"] = "Stock is updated successfully.";
            }
            catch (System.Exception)
            {
                TempData["errorMessage"] = "Something went worng!!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
