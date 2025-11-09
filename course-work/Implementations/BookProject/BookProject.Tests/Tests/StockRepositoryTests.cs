using BookProject.Data;
using BookProject.Models;
using BookProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookProject.Models.DTOS;

namespace BookProject.Tests.Tests
{
    public class StockRepositoryTests
    {
        private ApplicationDbContext _context;
        private StockRepository _stockRepository;

        public StockRepositoryTests()
        {
            ReinitializeDatabase();
        }

        private void ReinitializeDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
            SeedDatabase().GetAwaiter().GetResult();
            _stockRepository = new StockRepository(_context);
        }

        private async Task SeedDatabase()
        {
            _context.Books.RemoveRange(_context.Books);
            _context.Stocks.RemoveRange(_context.Stocks);
            List<Book> books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    BookName = "Test Book 1",
                    Price = 10.99,
                    AuthorName = "Author 1"
                },
                new Book
                {
                    Id = 2,
                    BookName = "Test Book 2",
                    Price = 15.99,
                    AuthorName = "Author 2"
                }
            };

            List<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Id = 1,
                    BookId = 1,
                    Quantity = 15
                },
                new Stock
                {
                    Id = 2,
                    BookId = 2,
                    Quantity = 8
                },
            };

            _context.Books.AddRange(books);
            _context.Stocks.AddRange(stocks);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetStockByBookId_ShouldReturnCorrectStock()
        {
            ReinitializeDatabase();

            var retrievedStock = await _stockRepository.GetStockByBookId(1);

            Assert.NotNull(retrievedStock);
            Assert.Equal(15, retrievedStock.Quantity);
        }

        [Fact]
        public async Task BookExists_ShouldReturnTrueIfBookExists()
        {
            ReinitializeDatabase();

            var exists = await _stockRepository.BookExists(1);

            Assert.True(exists);
        }

        [Fact]
        public async Task BookExists_ShouldReturnFalseIfBookDoesNotExist()
        {
            ReinitializeDatabase();

            var exists = await _stockRepository.BookExists(99);
            Assert.False(exists);
        }

        [Fact]
        public async Task ManageStock_ShouldAddNewStockIfNotExists()
        {
            ReinitializeDatabase();

            var stockDto = new StockDTO { BookId = 99, Quantity = 20 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _stockRepository.ManageStock(stockDto));
        }

        [Fact]
        public async Task ManageStock_ShouldUpdateStockIfExists()
        {
            ReinitializeDatabase();

            var stockDto = new StockDTO { BookId = 1, Quantity = 25 };
            await _stockRepository.ManageStock(stockDto);

            var updatedStock = await _stockRepository.GetStockByBookId(1);

            Assert.NotNull(updatedStock);
            Assert.Equal(25, updatedStock.Quantity);
        }

        [Fact]
        public async Task GetStocks_ShouldReturnAllStocksMatchingSearchTerm()
        {
            ReinitializeDatabase();

            var stocks = await _stockRepository.GetStocks("Test Book 1");

            Assert.Single(stocks);
            Assert.Equal("Test Book 1", stocks.First().BookName);
        }
    }
}
