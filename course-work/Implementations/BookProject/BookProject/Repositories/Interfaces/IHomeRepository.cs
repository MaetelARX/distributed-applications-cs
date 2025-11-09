using BookProject.Models;

namespace BookProject.Repositories.Interfaces
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0);
        Task<IEnumerable<Genre>> Genres();
    }
}
