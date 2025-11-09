using BookProject.Data;
using BookProject.Models;
using BookProject.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookProject.Tests.Tests
{
    public class DetailsRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly DetailsRepository _repository;

        public DetailsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new DetailsRepository(_context);

            SeedDatabase();
        }
        private void SeedDatabase()
        {
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

            List<Details> details = new List<Details>
            {
                new Details 
                { 
                    Id = 1,
                    BookId = 1,
                    Description = "Nov detail za kniga 1",
                    Publisher = "Spisanie misyl"
                },
                new Details 
                { 
                    Id = 2,
                    BookId = 2,
                    Description = "Details for Book 2",
                    Publisher = "Spisanie misyl"
                }
            };

            _context.Books.AddRange(books);
            _context.Details.AddRange(details);
            _context.SaveChanges();
        }
        [Fact]
        public async Task AddDetails_ShouldAddDetailsToDatabase()
        {
            Details newDetails = new Details
            {
                Id = 3,
                BookId = 1,
                Description = "Nov detail za kniga 1",
                Publisher = "Spisanie misyl"
            };
            await _repository.AddDetails(newDetails);

            var details = await _context.Details.FindAsync(3);
            Assert.NotNull(details);
            Assert.Equal("Nov detail za kniga 1", details.Description);
        }
        [Fact]
        public async Task GetDetailsByBookId_ShouldReturnCorrectDetails()
        {
            var details = await _repository.GetDetailsByBookId(1);

            Assert.NotNull(details);
            Assert.Equal("Nov detail za kniga 1", details.Description);
        }
        [Fact]
        public async Task GetDetailsByBookId_ShouldReturnNullIfNotFound()
        {
            var details = await _repository.GetDetailsByBookId(99);
            Assert.Null(details);
        }
        [Fact]
        public async Task UpdateDetails_ShouldModifyDetails()
        {
            var details = await _repository.GetDetailsByBookId(1);
            details.Description = "promenen detail za kniga 1";

            await _repository.UpdateDetails(details);

            var updatedDetails = await _context.Details.FindAsync(1);
            Assert.Equal("promenen detail za kniga 1", updatedDetails.Description);
        }
    }
}
