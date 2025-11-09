using BookProject.Controllers;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Controllers
{
    public class AdminOperationsControllerTests
    {
        private readonly Mock<IUserOrderRepository> _mockUserOrderRepo;
        private readonly AdminOperationsController _controller;

        public AdminOperationsControllerTests()
        {
            _mockUserOrderRepo = new Mock<IUserOrderRepository>();
            _controller = new AdminOperationsController(_mockUserOrderRepo.Object);
        }

        [Fact]
        public async Task AllOrders_ShouldReturnViewWithOrders()
        {
            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now;

            List<Order> orders = new List<Order>
            {
                new Order 
                { 
                    Id = 1,
                    CreateDate = DateTime.Now.AddDays(-2)
                },
                new Order 
                { 
                    Id = 2,
                    CreateDate = DateTime.Now.AddDays(-1)
                }
            };

            _mockUserOrderRepo
                .Setup(repo => repo.UserOrders(startDate, endDate, false))
                .ReturnsAsync(orders);

            var result = await _controller.AllOrders(startDate, endDate);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);

            Assert.Equal(orders, model);
            Assert.Equal(startDate.ToString("yyyy-MM-dd"), viewResult.ViewData["StartDate"]);
            Assert.Equal(endDate.ToString("yyyy-MM-dd"), viewResult.ViewData["EndDate"]);
        }


        [Fact]
        public async Task TogglePaymentStatus_ShouldRedirectToAllOrders()
        {
            var orderId = 1;

            _mockUserOrderRepo.Setup(repo => repo.TogglePaymentStatus(orderId))
                .Returns(Task.CompletedTask);

            var result = await _controller.TogglePaymentStatus(orderId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.AllOrders), redirectResult.ActionName);
        }

        [Fact]
        public async Task UpdateOrderStatus_Get_ShouldReturnViewWithData()
        {
            var orderId = 1;

            var order = new Order
            {
                Id = orderId,
                OrderStatusId = 2
            };

            List<OrderStatus> orderStatuses = new List<OrderStatus>
            {
                new OrderStatus 
                { 
                    Id = 1,
                    StatusName = "Pending"
                },
                new OrderStatus 
                { 
                    Id = 2,
                    StatusName = "Completed"
                }
            };

            _mockUserOrderRepo.Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync(order);
            _mockUserOrderRepo.Setup(repo => repo.GetOrderStatuses())
                .ReturnsAsync(orderStatuses);

            var result = await _controller.UpdateOrderStatus(orderId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateOrderStatusModel>(viewResult.Model);

            Assert.Equal(orderId, model.OrderId);
            Assert.Equal(order.OrderStatusId, model.OrderStatusId);
            Assert.Equal(2, model.OrderStatusList.Count());
        }

        [Fact]
        public async Task UpdateOrderStatus_Get_ShouldThrowIfOrderNotFound()
        {
            var orderId = 99;

            _mockUserOrderRepo.Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync((Order)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.UpdateOrderStatus(orderId));
        }

        [Fact]
        public async Task UpdateOrderStatus_Post_ShouldReturnViewIfModelInvalid()
        {
            UpdateOrderStatusModel data = new UpdateOrderStatusModel
            {
                OrderId = 1,
                OrderStatusId = 2
            };

            _controller.ModelState.AddModelError("OrderStatusId", "Required");

            List<OrderStatus> orderStatuses = new List<OrderStatus>
            {
                new OrderStatus 
                { 
                    Id = 1,
                    StatusName = "Pending"
                },
                new OrderStatus 
                { 
                    Id = 2,
                    StatusName = "Completed" }
            };

            _mockUserOrderRepo.Setup(repo => repo.GetOrderStatuses())
                .ReturnsAsync(orderStatuses);

            var result = await _controller.UpdateOrderStatus(data);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateOrderStatusModel>(viewResult.Model);

            Assert.Equal(data, model);
            Assert.Equal(2, model.OrderStatusList.Count());
        }
    }
}

