using BookProject.Data;
using BookProject.Models;
using BookProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace BookProject.Tests.Tests
{
    public class CartRepositoryTests
    {
        private ApplicationDbContext _context;
        private CartRepository _cartRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public CartRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            var mockUserStore = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null
            );
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-user-id");

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                }))
            });

            _cartRepository = new CartRepository(_context, _mockUserManager.Object, _mockHttpContextAccessor.Object, true);

            SeedDatabase().GetAwaiter().GetResult();
        }


        private async Task SeedDatabase()
        {
            _context.Books.RemoveRange(_context.Books);
            _context.Stocks.RemoveRange(_context.Stocks);
            List<Book> books = new List<Book>
            {
                new Book 
                { 
                    Id = 1,
                    BookName = "Book 1",
                    Price = 10.99,
                    AuthorName = "Plamen Jelev",
                },
                new Book 
                {   Id = 2,
                    BookName = "Book 2", 
                    Price = 15.99,
                    AuthorName = "Plamen Jelev"
                }
            };

            List<Stock> stocks = new List<Stock>
            {
                new Stock { Id = 1, BookId = 1, Quantity = 5 },
                new Stock { Id = 2, BookId = 2, Quantity = 3 }
            };

            _context.Books.AddRange(books);
            _context.Stocks.AddRange(stocks);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddItem_ShouldAddNewItemToCart()
        {
            var bookId = 1;
            var quantity = 2;

            var itemCount = await _cartRepository.AddItem(bookId, quantity);

            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Single(cart.CartDetails);
            Assert.Equal(quantity, cart.CartDetails.First().Quantity);
        }

        [Fact]
        public async Task AddItem_ShouldIncrementQuantityIfItemExists()
        {
            var bookId = 1;
            var initialQuantity = 2;
            await _cartRepository.AddItem(bookId, initialQuantity);

            var additionalQuantity = 3;
            var itemCount = await _cartRepository.AddItem(bookId, additionalQuantity);

            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Single(cart.CartDetails);
            Assert.Equal(initialQuantity + additionalQuantity, cart.CartDetails.First().Quantity);
        }

        [Fact]
        public async Task RemoveItem_ShouldDecreaseQuantity()
        {
            var bookId = 1;
            var initialQuantity = 3;
            await _cartRepository.AddItem(bookId, initialQuantity);

            var itemCount = await _cartRepository.RemoveItem(bookId);

            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Single(cart.CartDetails);
            Assert.Equal(initialQuantity - 1, cart.CartDetails.First().Quantity);
        }

        [Fact]
        public async Task RemoveItem_ShouldRemoveItemIfQuantityReachesZero()
        {
            var bookId = 1;
            var initialQuantity = 1;
            await _cartRepository.AddItem(bookId, initialQuantity);

            var itemCount = await _cartRepository.RemoveItem(bookId);

            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Empty(cart.CartDetails);
        }

        [Fact]
        public async Task GetUserCart_ShouldReturnCartWithDetails()
        {
            var bookId = 1;
            var quantity = 2;
            await _cartRepository.AddItem(bookId, quantity);

            var cart = await _cartRepository.GetUserCart();

            Assert.NotNull(cart);
            Assert.Single(cart.CartDetails);
            Assert.Equal(quantity, cart.CartDetails.First().Quantity);
        }

        [Fact]
        public async Task GetCartItemCount_ShouldReturnCorrectCount()
        {
            var bookId1 = 1;
            var bookId2 = 2;
            await _cartRepository.AddItem(bookId1, 1);
            await _cartRepository.AddItem(bookId2, 2);

            var itemCount = await _cartRepository.GetCartItemCount();

            Assert.Equal(2, itemCount);
        }

        [Fact]
        public async Task GetBookQuantityInCart_ShouldReturnCorrectQuantity()
        {
            var bookId = 1;
            var quantity = 2;
            await _cartRepository.AddItem(bookId, quantity);

            var cartQuantity = await _cartRepository.GetBookQuantityInCart(bookId);

            Assert.Equal(quantity, cartQuantity);
        }
    }
}
