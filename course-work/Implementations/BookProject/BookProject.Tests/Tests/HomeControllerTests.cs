using BookProject.Controllers;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IHomeRepository> _mockHomeRepository;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockHomeRepository = new Mock<IHomeRepository>();
            _controller = new HomeController(_mockLogger.Object, _mockHomeRepository.Object);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithCorrectModel()
        {
            List<Book> books = new List<Book>
            {
                new Book 
                { 
                    Id = 1,
                    BookName = "Book 1",
                    AuthorName = "Author 1",
                    Price = 10.99 
                },
                new Book 
                { 
                    Id = 2,
                    BookName = "Book 2",
                    AuthorName = "Author 2",
                    Price = 15.99 
                }
            };

            List<Genre> genres = new List<Genre>
            {
                new Genre 
                { 
                    Id = 1,
                    GenreName = "Genre 1"
                },
                new Genre 
                { 
                    Id = 2,
                    GenreName = "Genre 2"
                }
            };

            _mockHomeRepository.Setup(repo => repo.GetBooks(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(books);
            _mockHomeRepository.Setup(repo => repo.Genres()).ReturnsAsync(genres);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDisplayModel>(viewResult.Model);

            Assert.Equal(books, model.Books);
            Assert.Equal(genres, model.Genres);
        }

        [Fact]
        public async Task Index_ShouldSortBooksByPriceAscending()
        {
            List<Book> books = new List<Book>
            {
                new Book 
                { 
                    Id = 1,
                    BookName = "Book 1",
                    AuthorName = "Author 1",
                    Price = 15.99 
                },
                new Book 
                { 
                    Id = 2,
                    BookName = "Book 2",
                    AuthorName = "Author 2",
                    Price = 10.99 
                }
            };

            _mockHomeRepository.Setup(repo => repo.GetBooks(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(books);

            var result = await _controller.Index(sortBy: "PriceAsc");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDisplayModel>(viewResult.Model);

            Assert.Equal(books.OrderBy(b => b.Price), model.Books);
        }

        [Fact]
        public async Task Index_ShouldSortBooksByTitle()
        {
            var books = new List<Book>
            {
                new Book { Id = 2, BookName = "B Book", AuthorName = "Author 2", Price = 10.99 },
                new Book { Id = 1, BookName = "A Book", AuthorName = "Author 1", Price = 15.99 }
            };

            _mockHomeRepository.Setup(repo => repo.GetBooks(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(books);

            var result = await _controller.Index(sortBy: "Title");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDisplayModel>(viewResult.Model);

            Assert.Equal(books.OrderBy(b => b.BookName), model.Books);
        }

        [Fact]
        public void Privacy_ShouldReturnView()
        {
            var result = _controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ShouldReturnViewWithErrorModel()
        {
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("test-trace-id");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = _controller.Error();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);

            Assert.Equal("test-trace-id", model.RequestId);
        }

    }
}
