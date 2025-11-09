using BookProject.Controllers;
using BookProject.Models;
using BookProject.Repositories.Interfaces;
using BookProject.Utilities.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BookProject.Models.DTOS;
using Microsoft.AspNetCore.Http;

namespace BookProject.Tests.Tests
{
    public class BookControllerTests
    {
        private readonly Mock<IBookRepository> _mockBookRepo;
        private readonly Mock<IGenreRepository> _mockGenreRepo;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IDetailsRepository> _mockDetailsRepo;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            _mockDetailsRepo = new Mock<IDetailsRepository>();
            _mockBookRepo = new Mock<IBookRepository>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockFileService = new Mock<IFileService>();
            _mockGenreRepo = new Mock<IGenreRepository>();

            var tempData = new Mock<ITempDataDictionary>();
            _controller = new BookController
                (
                    _mockBookRepo.Object,
                    _mockGenreRepo.Object,
                    _mockFileService.Object,
                    _mockEnvironment.Object,
                    _mockDetailsRepo.Object
                )
            {
                TempData = tempData.Object
            };
        }
        [Fact]
        public async Task Index_ShouldReturnViewWithBooks()
        {
            List<Book> books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    BookName = "Book 1"
                },
                new Book
                {
                    Id = 2,
                    BookName = "Book 2"
                }
            };
            _mockBookRepo.Setup(repo => repo.GetBooks()).ReturnsAsync(books);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(books, viewResult.Model);
        }

        [Fact]
        public async Task AddBook_Get_ShouldReturnViewWithGenres()
        {
            List<Genre> genres = new List<Genre>
            {
                new Genre
                {
                    Id = 1,
                    GenreName = "Fiction"
                },
                new Genre
                {
                    Id = 2,
                    GenreName = "Non-Fiction"
                }
            };
            _mockGenreRepo.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

            var result = await _controller.AddBook();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDTO>(viewResult.Model);
            Assert.NotNull(model.GenreList);
        }

        [Fact]
        public async Task AddBook_Post_ShouldRedirectOnSuccess()
        {
            BookDTO bookDto = new BookDTO
            {
                BookName = "Test Book",
                AuthorName = "Test Author",
                Price = 9.99d,
                GenreId = 1
            };
            _mockBookRepo.Setup(repo => repo.AddBook(It.IsAny<Book>())).Returns(Task.CompletedTask);

            var result = await _controller.AddBook(bookDto);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddBook", redirectResult.ActionName);
        }
        [Fact]
        public async Task AddBook_Post_ShouldHandleInvalidModel()
        {
            BookDTO bookDto = new BookDTO
            {
                BookName = "",
                AuthorName = "",
                Price = -5,
                GenreId = 0
            };

            _controller.ModelState.AddModelError("BookName", "BookName is required");

            var result = await _controller.AddBook(bookDto);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(bookDto, viewResult.Model);
        }
        [Fact]
        public async Task UpdateBook_Get_ShouldReturnViewWithBookDetails()
        {
            Book book = new Book
            {
                Id = 1,
                BookName = "Testova Nick ga",
                GenreId = 1
            };
            _mockBookRepo.Setup(repo => repo.GetBookById(1)).ReturnsAsync(book);

            List<Genre> genres = new List<Genre>
            {
                new Genre
                {
                    Id = 1,
                    GenreName = "Fiction"
                },
                new Genre
                {
                    Id = 2,
                    GenreName = "Non-Fiction"
                }
            };
            _mockGenreRepo.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

            var result = await _controller.UpdateBook(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDTO>(viewResult.Model);
            Assert.Equal(book.BookName, model.BookName);
        }

        [Fact]
        public async Task UpdateBook_Post_ShouldRedirectOnSuccess()
        {
            BookDTO bookDTO = new BookDTO
            {
                Id = 1,
                BookName = "Updated Book",
                AuthorName = "Updated Author",
                Price = 12.99d,
                GenreId = 1
            };
            _mockBookRepo.Setup(repo => repo.UpdateBook(It.IsAny<Book>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateBook(bookDTO);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        [Fact]
        public async Task UpdateBook_Post_ShouldHandleInvalidModel()
        {
            BookDTO bookDto = new BookDTO
            {
                Id = 1,
                BookName = "",
                AuthorName = "",
                Price = -10,
                GenreId = 0
            };

            _controller.ModelState.AddModelError("BookName", "BookName is required");

            var result = await _controller.UpdateBook(bookDto);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(bookDto, viewResult.Model);
        }

        [Fact]
        public async Task DeleteBook_ShouldRedirectOnSuccess()
        {
            Book book = new Book
            {
                Id = 1,
                BookName = "Test Book"
            };
            _mockBookRepo.Setup(repo => repo.GetBookById(1)).ReturnsAsync(book);
            _mockBookRepo.Setup(repo => repo.DeleteBook(book)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteBook(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteBook_ShouldHandleBookNotFound()
        {
            _mockBookRepo.Setup(repo => repo.GetBookById(1)).ReturnsAsync((Book)null);

            var result = await _controller.DeleteBook(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task AddDetails_Post_ShouldRedirectOnSuccess()
        {
            Details details = new Details
            {
                Id = 1,
                BookId = 1,
                Description = "Test Details"
            };
            _mockDetailsRepo.Setup(repo => repo.AddDetails(details)).Returns(Task.CompletedTask);

            var result = await _controller.AddDetails(details);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        [Fact]
        public async Task AddDetails_Post_ShouldHandleInvalidModel()
        {
            Details details = new Details
            {
                Id = 1,
                BookId = 1,
                Description = ""
            };
            _controller.ModelState.AddModelError("Description", "Description is required");

            var result = await _controller.AddDetails(details);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(details, viewResult.Model);
        }

        [Fact]
        public async Task ViewDetails_ShouldReturnViewWithDetails()
        {
            Details details = new Details
            {
                Id = 1,
                BookId = 1,
                Description = "Test Details"
            };
            _mockDetailsRepo.Setup(repo => repo.GetDetailsByBookId(1)).ReturnsAsync(details);

            var result = await _controller.ViewDetails(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(details, viewResult.Model);
        }

        [Fact]
        public async Task ViewDetails_SholdHandleDetailsNotFound()
        {
            _mockDetailsRepo.Setup(repo => repo.GetDetailsByBookId(1)).ReturnsAsync((Details)null);

            var result = await _controller.ViewDetails(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task AddBook_Get_ShouldReturnGenresSelectedList()
        {
            List<Genre> genres = new List<Genre>
            {
                new Genre
                {
                    Id = 1,
                    GenreName = "Action"
                },
                new Genre
                {
                    Id = 2,
                    GenreName = "Adventure"
                }
            };
            _mockGenreRepo.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

            var result = await _controller.AddBook();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDTO>(viewResult.Model);
            Assert.NotNull(model.GenreList);
            Assert.Equal(2, model.GenreList.Count());
        }

        [Fact]
        public async Task AddBook_Post_ShouldHandleFileUploadError()
        {
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            BookDTO bookDTO = new BookDTO
            {
                BookName = "Test Book",
                AuthorName = "Author",
                Price = 19.99d,
                GenreId = 1,
                ImageFile = new Mock<IFormFile>().Object
            };

            _mockFileService.Setup(service => service.SaveFile(bookDTO.ImageFile, It.IsAny<string[]>()))
                .ThrowsAsync(new InvalidOperationException("Invalid file"));

            var result = await _controller.AddBook( bookDTO);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(bookDTO, viewResult.Model);
            Assert.True(_controller.TempData.ContainsKey("errorMessage"));
        }
        [Fact]
        public async Task UpdateBook_Get_ShouldRedirectIfBookNotFound()
        {
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _mockBookRepo.Setup(repo => repo.GetBookById(It.IsAny<int>())).ReturnsAsync((Book)null);

            var result = await _controller.UpdateBook(999);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("errorMessage"));
        }
        [Fact]
        public async Task DeleteBook_ShouldHandleExceptionGracefully()
        {
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _mockBookRepo.Setup(repo => repo.GetBookById(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.DeleteBook(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("errorMessage"));
        }

        [Fact]
        public async Task AddDetails_Get_ShouldRedirectIfBookNotFound()
        {
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _mockBookRepo.Setup(repo => repo.GetBookById(It.IsAny<int>()))
                .ReturnsAsync((Book)null);

            var result = await _controller.AddDetails(999);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("errorMessage"));
        }
    }
}
