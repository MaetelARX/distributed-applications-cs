using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using BookProject.Models;
using BookProject.Repositories.Interfaces;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace BookProject.Tests.Views
{
    public class RazorViewTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<ICartRepository> _mockCartRepo;

        public RazorViewTests(WebApplicationFactory<Program> factory)
        {
            _mockCartRepo = new Mock<ICartRepository>();

            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped(_ => _mockCartRepo.Object);
                });

                builder.UseContentRoot(@"C:\Users\Konos\OneDrive\Documents\GitHub\BookProject\BookProject");
            }).CreateClient();
        }

        [Fact]
        public async Task Home_Index_ShouldReturnViewWithBooks()
        {
            var response = await _client.GetAsync("/Home/Index");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Избери жанр", content);
            Assert.Contains("Търси", content);
        }

        [Fact]
        public async Task Home_Index_ShouldRenderBooks()
        {
            var response = await _client.GetAsync("/Home/Index");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Add to cart", content);
            Assert.Contains("Out of stock", content);
        }

        [Fact]
        public async Task Cart_GetUserCart_ShouldRenderWithItems()
        {
            // Arrange
            var shoppingCart = new ShoppingCart
            {
                CartDetails = new List<CartDetail>
                {
                    new CartDetail
                    {
                        Book = new Book
                        {
                            BookName = "Test Book",
                            Image = "/images/test.jpg",
                            Genre = new Genre { GenreName = "Fiction" },
                            Price = 10.99d,
                            Stock = new Stock { Quantity = 5 }
                        },
                        Quantity = 2,
                        BookId = 1
                    }
                }
            };

            _mockCartRepo.Setup(repo => repo.GetUserCart())
                         .ReturnsAsync(shoppingCart);
            var response = await _client.GetAsync("/Cart/GetUserCart");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("<table", content);
            Assert.Contains("Test Book", content);
            Assert.Contains("Fiction", content);
            Assert.Contains("10.99", content);
            Assert.Contains("Add to cart", content);
        }

        [Fact]
        public async Task Cart_GetUserCart_ShouldRenderEmptyCartMessage()
        {
            // Arrange
            _mockCartRepo.Setup(repo => repo.GetUserCart())
                         .ReturnsAsync(new ShoppingCart { CartDetails = new List<CartDetail>() });

            // Act
            var response = await _client.GetAsync("/Cart/GetUserCart");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            // Assert that the "Cart Is empty" message is displayed
            Assert.Contains("Cart Is empty", content);
        }

    }
}
