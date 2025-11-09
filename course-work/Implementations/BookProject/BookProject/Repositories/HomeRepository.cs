using BookProject.Data;
using BookProject.Models;
using BookProject.Repositories;
using BookProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<Genre>>Genres()
        {
            return await _db.Genres.ToListAsync();
        }
        public async Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0)
        {
            sTerm = sTerm.ToLower();
            IEnumerable<Book> books = await (from book in _db.Books
                         join genre in _db.Genres
                         on book.GenreId equals genre.Id
                         join stock in _db.Stocks
                         on book.Id equals stock.BookId
                         into book_stocks
                         from bookWithStcok in book_stocks.DefaultIfEmpty()
                         where string.IsNullOrWhiteSpace(sTerm) || (book!=
                         null && book.BookName.ToLower().StartsWith(sTerm))
                         select new Book
                         {
                             Id = book.Id,
                             Image = book.Image,
                             AuthorName = book.AuthorName,
                             BookName = book.BookName,
                             GenreId = book.GenreId,
                             Price = book.Price,
                             GenreName = genre.GenreName,
                             Quantity = bookWithStcok==null?
                             0: bookWithStcok.Quantity
                         }
                        ).ToListAsync();

            if(genreId > 0)
            {
                books = books.Where(a => a.GenreId == genreId).ToList();
            }

            return books;
        }
    }
}
