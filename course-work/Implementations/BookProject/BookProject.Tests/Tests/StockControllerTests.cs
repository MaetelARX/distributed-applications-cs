using BookProject.Controllers;
using BookProject.Data;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Tests
{
    public class StockControllerTests
    {
        private readonly Mock<IStockRepository> _mockstockRepo;
        private readonly ApplicationDbContext _context;
        private readonly StockController _controller;

        public StockControllerTests()
        {
            _mockstockRepo = new Mock<IStockRepository>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new StockController(_mockstockRepo.Object, _context);

            var tempData = new Mock<ITempDataDictionary>();
            _controller.TempData = tempData.Object;
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithStocks()
        {
            _context.Stocks.AddRange(new Stock
            {
                Id = 1,
                BookId = 1,
                Quantity = 10
            }, new Stock
            {
                Id = 2,
                BookId = 2,
                Quantity = 5
            });

            await _context.SaveChangesAsync();

            List<StockDisplayModel> stocks = new List<StockDisplayModel>
            {
                new StockDisplayModel
                {
                    BookId = 1,
                    BookName = "Test Book 1",
                    Quantity = 10,
                    Id = 1,
                },
                new StockDisplayModel
                {
                    BookId = 2,
                    BookName = "Test Book 2",
                    Quantity = 5,
                    Id = 2
                }
            };

            _mockstockRepo.Setup(repo => repo.GetStocks("")).ReturnsAsync(stocks);
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(stocks, viewResult.Model);
        }

        [Fact]
        public async Task ManageStock_Get_ShouldReturnViewWithStock()
        {
            var bookId = 1;
            _context.Books.Add(new Book
            {
                Id = bookId,
                BookName = "Test Book 1",
                Price = 12.99,
                AuthorName = "Plamen Jelev"
            });

            _context.Stocks.Add(new Stock
            {
                Id = 1,
                BookId = bookId,
                Quantity = 10
            });

            await _context.SaveChangesAsync();

            _mockstockRepo.Setup(repo => repo.BookExists(bookId)).ReturnsAsync(true);
            _mockstockRepo.Setup(repo => repo.GetStockByBookId(bookId)).ReturnsAsync(new Stock
            {
                Id = 1,
                BookId = bookId,
                Quantity = 10
            });
            var result = await _controller.ManangeStock(bookId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var stock = Assert.IsType<StockDTO>(viewResult.Model);
            Assert.Equal(bookId, stock.BookId);
            Assert.Equal(10, stock.Quantity);
        }
        [Fact]
        public async Task ManageStock_Post_ShouldReturnViewIfModelInvalid()
        {
            StockDTO stockDto = new StockDTO
            {
                BookId = 1,
                Quantity = 10
            };

            _controller.ModelState.AddModelError("Quantity", "Quantity is required");

            var result = await _controller.ManangeStock(stockDto);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(stockDto, viewResult.Model);
        }
    }
}
