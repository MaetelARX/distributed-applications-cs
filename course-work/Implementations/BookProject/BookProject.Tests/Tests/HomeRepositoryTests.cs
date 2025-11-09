using BookProject.Data;
using BookProject.Models;
using BookProject.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Repositories
{
    public class HomeRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly HomeRepository _repository;

        public HomeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new HomeRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.Genres.AddRange(
                new Genre { Id = 1, GenreName = "Fiction" },
                new Genre { Id = 2, GenreName = "Non-Fiction" }
            );

            _context.Books.AddRange(
                new Book { Id = 1, BookName = "Book 1", AuthorName = "Author 1", Price = 10.99, GenreId = 1, Image = "image1.jpg" },
                new Book { Id = 2, BookName = "Book 2", AuthorName = "Author 2", Price = 15.99, GenreId = 2, Image = "image2.jpg" }
            );

            _context.Stocks.AddRange(
                new Stock { Id = 1, BookId = 1, Quantity = 5 },
                new Stock { Id = 2, BookId = 2, Quantity = 10 }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task Genres_ShouldReturnAllGenres()
        {
            var genres = await _repository.Genres();

            Assert.NotNull(genres);
            Assert.Equal(2, genres.Count());
            Assert.Contains(genres, g => g.GenreName == "Fiction");
            Assert.Contains(genres, g => g.GenreName == "Non-Fiction");
        }

        [Fact]
        public async Task GetBooks_ShouldReturnAllBooks()
        {
            var books = await _repository.GetBooks();

            Assert.NotNull(books);
            Assert.Equal(2, books.Count());
            Assert.Contains(books, b => b.BookName == "Book 1");
            Assert.Contains(books, b => b.BookName == "Book 2");
        }

        [Fact]
        public async Task GetBooks_ShouldFilterBySearchTerm()
        {
            var books = await _repository.GetBooks("Book 1");

            Assert.Single(books);
            Assert.Equal("Book 1", books.First().BookName);
        }

        [Fact]
        public async Task GetBooks_ShouldFilterByGenre()
        {
            var books = await _repository.GetBooks("", genreId: 1);

            Assert.Single(books);
            Assert.Equal(1, books.First().GenreId);
            Assert.Equal("Fiction", books.First().GenreName);
        }

        [Fact]
        public async Task GetBooks_ShouldReturnEmptyIfNoMatches()
        {
            var books = await _repository.GetBooks("Nonexistent");

            Assert.Empty(books);
        }

        [Fact]
        public async Task GetBooks_ShouldIncludeStockInformation()
        {
            var books = await _repository.GetBooks();

            var bookWithStock = books.FirstOrDefault(b => b.BookName == "Book 1");
            Assert.NotNull(bookWithStock);
            Assert.Equal(5, bookWithStock.Quantity);
        }
    }
}
