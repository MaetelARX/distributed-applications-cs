using BookProject.Controllers;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Controllers
{
    public class CartControllerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepo;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _mockCartRepo = new Mock<ICartRepository>();
            _controller = new CartController(_mockCartRepo.Object);
        }

        [Fact]
        public async Task AddItem_ShouldReturnOkWithCartCount()
        {
            int bookId = 1;
            int qty = 2;
            int cartCount = 5;

            _mockCartRepo.Setup(repo => repo.AddItem(bookId, qty))
                         .ReturnsAsync(cartCount);

            var result = await _controller.AddItem(bookId, qty);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(cartCount, okResult.Value);
        }

        [Fact]
        public async Task AddItem_ShouldRedirectToGetUserCart()
        {
            int bookId = 1;
            int qty = 1;

            _mockCartRepo.Setup(repo => repo.AddItem(bookId, qty))
                         .ReturnsAsync(3);

            var result = await _controller.AddItem(bookId, qty, redirect: 1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetUserCart", redirectResult.ActionName);
        }

        [Fact]
        public async Task RemoveItem_ShouldRedirectToGetUserCart()
        {
            int bookId = 1;

            _mockCartRepo.Setup(repo => repo.RemoveItem(bookId))
                         .ReturnsAsync(0);

            var result = await _controller.RemoveItem(bookId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetUserCart", redirectResult.ActionName);
        }

        [Fact]
        public async Task GetUserCart_ShouldReturnViewWithCart()
        {
            var cart = new ShoppingCart { Id = 1, UserId = "test-user" };

            _mockCartRepo.Setup(repo => repo.GetUserCart())
                         .ReturnsAsync(cart);

            var result = await _controller.GetUserCart();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(cart, viewResult.Model);
        }

        [Fact]
        public async Task Checkout_ShouldRedirectToOrderSuccess()
        {
            var model = new CheckOutModel { Name = "Test", Email = "test@test.com" };

            _mockCartRepo.Setup(repo => repo.DoCheckout(model))
                         .ReturnsAsync(true);
            var result = await _controller.Checkout(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrderSuccess", redirectResult.ActionName);
        }
        [Fact]
        public async Task GetTotalItemInCart_ShouldReturnOkWithCartItemCount()
        {
            int itemCount = 5;

            _mockCartRepo.Setup(repo => repo.GetCartItemCount(It.IsAny<string>()))
                         .ReturnsAsync(itemCount);
            var result = await _controller.GetTotalItemInCart();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(itemCount, okResult.Value);
        }
        [Fact]
        public async Task Checkout_ShouldRedirectToOrderFailure()
        {
            var model = new CheckOutModel { Name = "Test", Email = "test@test.com" };

            _mockCartRepo.Setup(repo => repo.DoCheckout(model))
                         .ReturnsAsync(false);

            var result = await _controller.Checkout(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrderFailure", redirectResult.ActionName);
        }

        [Fact]
        public async Task Checkout_ShouldReturnViewIfModelInvalid()
        {
            var model = new CheckOutModel { Name = "Test", Email = "test@test.com" };
            _controller.ModelState.AddModelError("Name", "Name is required");

            var result = await _controller.Checkout(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task GetBookQuantity_ShouldReturnOkWithQuantity()
        {
            int bookId = 1;
            int quantity = 3;

            _mockCartRepo.Setup(repo => repo.GetBookQuantityInCart(bookId))
                         .ReturnsAsync(quantity);

            var result = await _controller.GetBookQuantity(bookId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(quantity, okResult.Value);
        }

        [Fact]
        public async Task GetBookQuantity_ShouldReturnBadRequestOnError()
        {
            int bookId = 1;

            _mockCartRepo.Setup(repo => repo.GetBookQuantityInCart(bookId))
                         .ThrowsAsync(new Exception("Test error"));

            var result = await _controller.GetBookQuantity(bookId);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
            Assert.Equal("Test error", message);
        }

        [Fact]
        public async Task OrderSuccess_ShouldReturnView()
        {
            var result = await _controller.OrderSuccess();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task OrderFailure_ShouldReturnView()
        {
            var result = await _controller.OrderFailure();
            Assert.IsType<ViewResult>(result);
        }
    }
}
