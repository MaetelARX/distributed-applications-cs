namespace BookProject.Models.DTOS
{
    public class BookDisplayModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public string STerm { get; set; } = "";
        public string SortBy { get; set; } = "";
        public int GenreId { get; set; } = 0;
    }
}
