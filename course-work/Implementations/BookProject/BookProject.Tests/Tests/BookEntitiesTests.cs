using BookProject.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Collections.Generic;

namespace BookProject.Tests.Tests
{
    public class BookEntitiesTests
    {
        [Fact]
        public void BookEntity_ShouldHaveRequiredProperties()
        {
            Book book = new Book
            {
                Id = 1,
                BookName = "Test",
                AuthorName = "plamen",
                Price = 19.99,
                Image = "image.jpg",
                GenreId = 1,
                Genre = new Genre
                {
                    Id = 1,
                    GenreName = "Fiction"
                },
                OrderDetail = new List<OrderDetail>(),
                CartDetail = new List<CartDetail>(),
                Stock = new Stock
                {
                    Id = 1,
                    BookId = 1,
                    Quantity = 10
                },
                GenreName = "Fiction",
                Quantity = 5
            };

            Assert.NotNull(book.BookName);
            Assert.NotNull(book.AuthorName);
            Assert.True(book.Price > 0);
            Assert.Equal(1, book.GenreId);
            Assert.NotNull(book.Genre);
            Assert.NotNull(book.Stock);
            Assert.Equal(5, book.Quantity);
        }

        [Fact]
        public void BookEntity_ShouldHandleRelationships()
        {
            Book book = new Book
            {
                Id = 1,
                BookName = "Book with Relationships",
                Genre = new Genre
                {
                    Id = 1,
                    GenreName = "Fiction"
                },
                OrderDetail = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Id = 1,
                        BookId = 1,
                        Quantity = 2
                    },
                    new OrderDetail
                    {
                        Id = 2,
                        BookId = 1,
                        Quantity = 3
                    }
                },
                CartDetail = new List<CartDetail>
                {
                    new CartDetail
                    {
                        Id = 1,
                        BookId = 1,
                        Quantity = 1
                    },
                },
                Stock = new Stock
                {
                    Id = 1,
                    BookId = 1,
                    Quantity = 10
                }
            };

            var totalOrderQuantity = book.OrderDetail.Count;
            var totalCartQuantity = book.CartDetail.Count;

            Assert.NotNull(book.Genre);
            Assert.Equal("Fiction", book.Genre.GenreName);
            Assert.Equal(2, totalOrderQuantity);
            Assert.Equal(1, totalCartQuantity);
            Assert.Equal(10, book.Stock.Quantity);
        }
    }
}
