using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;

namespace BookProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sterm="", int genreId = 0, string sortBy = "")
        {
            IEnumerable<Book> books = await _homeRepository.GetBooks(sterm,genreId);
            books = sortBy switch
            {
                "PriceAsc" => books.OrderBy(b => b.Price),
                "PriceDesc" => books.OrderByDescending(b => b.Price),
                "Title" => books.OrderBy(b => b.BookName),
                "Author" => books.OrderBy(b => b.AuthorName),
                _ => books
            };
            IEnumerable<Genre> genres = await _homeRepository.Genres();
            BookDisplayModel bookModel = new BookDisplayModel
            {
                Books = books,
                Genres = genres,
                STerm = sterm,
                GenreId = genreId,
                SortBy = sortBy
            };

            return View(bookModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
