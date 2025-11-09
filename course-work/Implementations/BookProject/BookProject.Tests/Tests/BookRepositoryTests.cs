using BookProject.Data;
using BookProject.Models;
using BookProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookProject.Tests.Tests
{
    public class BookRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly BookRepository _bookRepository;

        public BookRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookStoreDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _bookRepository = new BookRepository(_context);
        }

        [Fact]
        public async Task AddBook_ShouldAddBookToDatabase()
        {
            var book = new Book
            {
                Id = 1,
                BookName = "Testova kniga",
                Price = 19.99d,
                AuthorName = "Plamen Jelev"
            };

            await _bookRepository.AddBook(book);

            var addedBook = await _context.Books.FirstOrDefaultAsync(x => x.Id == 1);
            Assert.NotNull(addedBook);
            Assert.Equal("Testova kniga", addedBook.BookName);
        }

        [Fact]
        public async Task DeleteBook_ShouldRemoveBookFromDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var bookRepository = new BookRepository(_context);

            var book = new Book
            {
                Id = 2,
                BookName = "Kniga koqto shte triem",
                Price = 10.00d,
                AuthorName = "Plamen Jelev"
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            await _bookRepository.DeleteBook(book);
            var result = await bookRepository.GetBookById(book.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetBookById_ShouldReturnCorrectBook()
        {
            var book = new Book
            {
                Id = 3,
                BookName = "Kniga koqto shte tyrsim",
                Price = 25.00d,
                AuthorName = "Plamen Jelev"
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var foundBook = await _bookRepository.GetBookById(3);

            Assert.NotNull(foundBook);
            Assert.Equal("Kniga koqto shte tyrsim", foundBook.BookName);
        }

        [Fact]
        public async Task GetBooks_ShouldReturnAllBooks()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var bookRepository = new BookRepository(context);

            var book1 = new Book
            {
                Id = 1,
                BookName = "Book One",
                AuthorName = "Author One",
                Genre = new Genre { Id = 1, GenreName = "Genre One" }
            };

            var book2 = new Book
            {
                Id = 2,
                BookName = "Book Two",
                AuthorName = "Author Two",
                Genre = new Genre { Id = 2, GenreName = "Genre Two" }
            };

            context.Books.Add(book1);
            context.Books.Add(book2);

            await context.SaveChangesAsync();

            var books = await bookRepository.GetBooks();

            Assert.NotNull(books);
            Assert.Equal(2, books.Count());
        }
        [Fact]
        public async Task UpdateBook_ShouldModifyBookDetails()
        {
            var book = new Book
            {
                Id = 6,
                BookName = "Staro ime",
                Price = 21.99d,
                AuthorName = "Plamen Jelev"
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            book.BookName = "Promeneno Ime";
            await _bookRepository.UpdateBook(book);

            var updatedBook = await _context.Books.FirstOrDefaultAsync(x => x.Id == 6);
            Assert.NotNull(updatedBook);
            Assert.Equal("Promeneno Ime", updatedBook.BookName);
        }
    }
}
