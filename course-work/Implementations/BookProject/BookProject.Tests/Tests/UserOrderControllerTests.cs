using BookProject.Controllers;
using BookProject.Models;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Tests
{
    public class UserOrderControllerTests
    {
        private readonly Mock<IUserOrderRepository> _mockUserOrderRepo;
        private readonly UserOrderController _controller;

        public UserOrderControllerTests()
        {
            _mockUserOrderRepo = new Mock<IUserOrderRepository>();
            _controller = new UserOrderController(_mockUserOrderRepo.Object);
        }

        [Fact]
        public async Task UserOrders_ShouldReturnViewWithOrders()
        {
            var orders = new List<Order>
            {
                new Order { Id = 1, CreateDate = DateTime.Now },
                new Order { Id = 2, CreateDate = DateTime.Now.AddDays(-1) }
            };

            _mockUserOrderRepo
                .Setup(repo => repo.UserOrders(null, null, false))
                .ReturnsAsync(orders);


            var result = await _controller.UserOrders(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);

            Assert.Equal(orders, model);
        }

        [Fact]
        public async Task UserOrders_ShouldFilterByStartDate()
        {
            DateTime startDate = DateTime.Now.AddDays(-2);
            var filteredOrders = new List<Order>
            {
                new Order { Id = 1, CreateDate = DateTime.Now.AddDays(-1) }
            };

            _mockUserOrderRepo
                .Setup(repo => repo.UserOrders(startDate, null, false))
                .ReturnsAsync(filteredOrders);

            var result = await _controller.UserOrders(startDate, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);

            Assert.Equal(filteredOrders, model);
            Assert.Equal(startDate.ToString("yyyy-MM-dd"), viewResult.ViewData["StartDate"]);
        }

        [Fact]
        public async Task UserOrders_ShouldFilterByEndDate()
        {
            DateTime endDate = DateTime.Now.AddDays(-1);
            var filteredOrders = new List<Order>
            {
                new Order { Id = 1, CreateDate = DateTime.Now.AddDays(-2) }
            };

            _mockUserOrderRepo
                .Setup(repo => repo.UserOrders(null, endDate, false))
                .ReturnsAsync(filteredOrders);

            var result = await _controller.UserOrders(null, endDate);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);

            Assert.Equal(filteredOrders, model);
            Assert.Equal(endDate.ToString("yyyy-MM-dd"), viewResult.ViewData["EndDate"]);
        }
    }
}
