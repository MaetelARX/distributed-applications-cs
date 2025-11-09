using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class StockController : Controller
    {
        private readonly IStockRepository _stockRepo;
        private readonly ApplicationDbContext _context;

        public StockController(IStockRepository stockRepo, ApplicationDbContext context)
        {
            _stockRepo = stockRepo;
            _context = context;
        }

        public async Task<IActionResult> Index(string sTerm = "")
        {
            var stocks = await _stockRepo.GetStocks(sTerm);
            return View(stocks);
        }

        [HttpGet]
        public async Task<IActionResult> ManangeStock(int bookId)
        {
            if (bookId == 0)
            {
                TempData["errorMessage"] = "Book ID is missing or invalid.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"Received bookId: {bookId}");

            var bookExists = await _stockRepo.BookExists(bookId);
            if (!bookExists)
            {
                TempData["errorMessage"] = $"Book with ID {bookId} does not exist.";
                return RedirectToAction(nameof(Index));
            }

            var existingStock = await _stockRepo.GetStockByBookId(bookId);
            var stock = new StockDTO
            {
                BookId = bookId,
                Quantity = existingStock != null ? existingStock.Quantity : 0
            };
            return View(stock);
        }


        [HttpPost]
        public async Task<IActionResult> ManangeStock(StockDTO stock)
        {
            if (!ModelState.IsValid)
            {
                return View(stock);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var bookExists = await _context.Books.AnyAsync(b => b.Id == stock.BookId);
                    if (!bookExists)
                    {
                        TempData["errorMessage"] = $"Book with ID {stock.BookId} does not exist.";
                        return RedirectToAction(nameof(Index));
                    }
                    await _stockRepo.ManageStock(stock);

                    await transaction.CommitAsync();
                    TempData["successMessage"] = "Stock is updated successfully.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["errorMessage"] = $"Something went wrong!! {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}