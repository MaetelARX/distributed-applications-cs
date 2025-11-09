using BookProject.Models;
using BookProject.Utilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Details> Details { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = 1, GenreName = "Action" },
                new Genre { Id = 2, GenreName = "Horror" },
                new Genre { Id = 3, GenreName = "Romance" },
                new Genre { Id = 4, GenreName = "Science" },
                new Genre { Id = 5, GenreName = "Novel"},
                new Genre { Id = 6, GenreName = "Fantasy"},
                new Genre { Id = 7, GenreName = "Poem"}
            );

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var books = new[]
            {
            new { Id = 1, BookName = "Война и Мир", AuthorName = "Лев Толстой", Price = 20.50, GenreId = 5, Image = "war_and_peace.jfif" },
            new { Id = 2, BookName = "Ана Каренина", AuthorName = "Лев Толстой", Price = 18.00, GenreId = 3, Image = "anna_karenina.jpg" },
            new { Id = 3, BookName = "Владетелят", AuthorName = "Николо Макиавели", Price = 15.00, GenreId = 1, Image = "the_prince.jpg" },
            new { Id = 4, BookName = "Нова земя", AuthorName = "Иван Вазов", Price = 12.50, GenreId = 3, Image = "new_earth.jpg" },
            new { Id = 5, BookName = "Държавата", AuthorName = "Платон", Price = 14.00, GenreId = 4, Image = "the_republic.jpg" },
            new { Id = 6, BookName = "Фауст", AuthorName = "Йохан Волфганг Гьоте", Price = 16.00, GenreId = 3, Image = "faust.jpg" },
            new { Id = 7, BookName = "Воля за власт", AuthorName = "Фридрих Ницше", Price = 17.99, GenreId = 4, Image = "will_to_power.jpg" },
            new { Id = 8, BookName = "Защо светът е такъв какъвто е", AuthorName = "Пол Кенеди", Price = 22.99, GenreId = 4, Image = "why_world_is.jpg" },
            new { Id = 9, BookName = "48-те закона на властта", AuthorName = "Робърт Грийн", Price = 19.99, GenreId = 1, Image = "48_laws_of_power.jpg" },
            new { Id = 10, BookName = "Под Игото", AuthorName = "Иван Вазов", Price = 21.99, GenreId = 5, Image = "under_the_yoke.jpg" },
            new { Id = 11, BookName = "Песен за огън и лед", AuthorName = "Джордж Р. Р. Мартин", Price = 39.99, GenreId = 6, Image = "song_of_ice_and_water.jpg" },
            new { Id = 12, BookName = "Берсерк", AuthorName = "Кентаро Миура", Price = 109.99, GenreId = 6, Image = "berserk.jpg" },
            new { Id = 13, BookName = "Вагабонд", AuthorName = "Такехико Иноуе", Price = 59.99, GenreId = 5, Image = "vagabond.png" },
            new { Id = 14, BookName = "Винланд Сага", AuthorName = "Макото Юкимура", Price = 19.99, GenreId = 1, Image = "vinland_saga.jpg" },
            new { Id = 15, BookName = "Железен Пламък", AuthorName = "Ребека Яроз", Price = 33.00, GenreId = 1, Image = "iron_flame.jpg" },
            new { Id = 16, BookName = "То", AuthorName = "Стивън Кинг", Price = 19.99, GenreId = 2, Image = "it.jpg" },
            new { Id = 17, BookName = "33-те стратегии за войната", AuthorName = "Робърт Грийн", Price = 33.00, GenreId = 1, Image = "33_strategies_of_war.jpg" },
            new { Id = 18, BookName = "Идиот", AuthorName = "Фьодор Достоевски", Price = 59.99, GenreId = 5, Image = "idiot.jpg" },
            new { Id = 19, BookName = "Престъпление и наказание", AuthorName = "Фьодор Достоевски", Price = 30.00, GenreId = 5, Image = "crime_and_punishment.jpg" },
            new { Id = 20, BookName = "Евгений Онегин", AuthorName = "Алескандър Пушкин", Price = 50.00, GenreId = 5, Image = "eugene_onegin.jpg" },
            new { Id = 21, BookName = "Клетниците", AuthorName = "Виктор Юго", Price = 30.00, GenreId = 5, Image = "the_wretch.jpg" },
            new { Id = 22, BookName = "Борба", AuthorName = "Христо Ботев", Price = 25.00, GenreId = 7, Image = "struggle.jpg" },
            new { Id = 23, BookName = "До моето първо либе", AuthorName = "Христо Ботев", Price = 21.99, GenreId = 7, Image = "to_my_first_love.jpg" },
            new { Id = 24, BookName = "Тютюн", AuthorName = "Димитър Димов", Price = 40.00, GenreId = 1, Image = "tobacco.jfif" },
        };

            foreach (var book in books)
            {
                var originalFilePath = Path.Combine(uploadPath, book.Image);
                var resizedFilePath = Path.Combine(uploadPath, "resized_" + book.Image);

                if (File.Exists(originalFilePath) && !File.Exists(resizedFilePath))
                {
                    ImageResizer.ResizeImage(originalFilePath, resizedFilePath, 300, 450);
                }

                modelBuilder.Entity<Book>().HasData(new Book
                {
                    Id = book.Id,
                    BookName = book.BookName,
                    AuthorName = book.AuthorName,
                    Price = book.Price,
                    GenreId = book.GenreId,
                    Image = "/images/books/resized_" + book.Image
                });
            }
            modelBuilder.Entity<OrderStatus>().HasData
            (
                new OrderStatus {Id = 1, StatusName = "Pending", StatusId = 1 },
                new OrderStatus {Id = 2, StatusName = "Shipped", StatusId = 2 },
                new OrderStatus {Id = 3, StatusName = "Delivered", StatusId = 3 },
                new OrderStatus {Id = 4, StatusName = "Cancelled", StatusId = 4 },
                new OrderStatus {Id = 5, StatusName = "Returned", StatusId = 5 },
                new OrderStatus {Id = 6, StatusName = "Refund", StatusId = 6 }
            );
        }
    }
}
