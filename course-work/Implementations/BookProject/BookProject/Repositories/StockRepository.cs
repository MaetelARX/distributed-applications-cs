using BookProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> GetStockByBookId(int bookId) => await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == bookId);
        public async Task<bool> BookExists(int bookId)
        {
            return await _context.Books.AnyAsync(b => b.Id == bookId);
        }

        public async Task ManageStock(StockDTO stockToManage)
        {
            var bookExists = await _context.Books.AnyAsync(b => b.Id == stockToManage.BookId);
            if (!bookExists)
            {
                throw new InvalidOperationException($"Book with ID {stockToManage.BookId} does not exist.");
            }
            var existingStock = await GetStockByBookId(stockToManage.BookId);

            if (existingStock is null)
            {
                var stock = new Stock
                {
                    BookId = stockToManage.BookId,
                    Quantity = stockToManage.Quantity,
                };
                _context.Stocks.Add(stock);
            }
            else
            {
                existingStock.Quantity = stockToManage.Quantity;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
                throw;
            }
        }


        public async Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "")
        {
            var stocks = await (from book in _context.Books
                                join stock in _context.Stocks
                                on book.Id equals stock.BookId
                                into book_stock
                                from bookStock in book_stock.DefaultIfEmpty()
                                where string.IsNullOrWhiteSpace(sTerm) || book.BookName.ToLower().Contains(sTerm.ToLower())
                                select new StockDisplayModel
                                {
                                    BookId = book.Id,
                                    BookName = book.BookName,
                                    Quantity = bookStock == null ? 0 : bookStock.Quantity
                                }
                                ).ToListAsync();
            return stocks;
        }
    }
}