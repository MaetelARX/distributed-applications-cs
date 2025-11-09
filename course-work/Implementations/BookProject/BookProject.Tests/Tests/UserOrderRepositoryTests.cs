using BookProject.Data;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Repositories
{
    public class UserOrderRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly UserOrderRepository _repository;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public UserOrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-user-id");

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") }))
            });

            _repository = new UserOrderRepository(_context, _mockHttpContextAccessor.Object, _mockUserManager.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            List<Order> orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    UserId = "test-user-id",
                    CreateDate = DateTime.UtcNow,
                    IsPaid = false,
                    OrderStatusId = 1,
                    Address = "Street of Stz rebel 28",
                    Email = "john123@gmail.com",
                    MobileNumber = "0888888888",
                    Name = "john",
                    PaymentMethod = "COD"
                },
                new Order
                {
                    Id = 2,
                    UserId = "test-user-id",
                    CreateDate = DateTime.UtcNow,
                    IsPaid = true,
                    OrderStatusId = 2,
                    Address = "Street of Stz rebel 28",
                    Email = "john123@gmail.com",
                    MobileNumber = "0888888888",
                    Name = "john",
                    PaymentMethod = "COD"
                }
            };

            List<OrderStatus> statuses = new List<OrderStatus>
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

            _context.Orders.AddRange(orders);
            _context.OrderStatuses.AddRange(statuses);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnCorrectOrder()
        {
            var order = await _repository.GetOrderById(1);
            Assert.NotNull(order);
            Assert.Equal("test-user-id", order.UserId);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnNullIfNotFound()
        {
            var order = await _repository.GetOrderById(99);
            Assert.Null(order);
        }

        [Fact]
        public async Task GetOrderStatuses_ShouldReturnAllStatuses()
        {
            var statuses = await _repository.GetOrderStatuses();
            Assert.Equal(2, statuses.Count());
        }

        [Fact]
        public async Task ChangeOrderStatus_ShouldUpdateOrderStatus()
        {
            var updateModel = new UpdateOrderStatusModel { OrderId = 1, OrderStatusId = 2 };
            await _repository.ChangeOrderStatus(updateModel);
            var order = await _context.Orders.FindAsync(1);
            Assert.Equal(2, order.OrderStatusId);
        }

        [Fact]
        public async Task TogglePaymentStatus_ShouldMarkOrderAsPaid()
        {
            await _repository.TogglePaymentStatus(1);
            var order = await _context.Orders.FindAsync(1);
            Assert.True(order.IsPaid);
        }

        [Fact]
        public async Task UserOrders_ShouldReturnOrdersForUser()
        {
            var orders = await _repository.UserOrders(null, null);
            Assert.Equal(2, orders.Count());
        }
        [Fact]
        public async Task UserOrders_ShouldFilterByDateRange()
        {
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddDays(1);

            var orders = await _repository.UserOrders(startDate, endDate);

            Assert.Equal(2, orders.Count());
        }

        [Fact]
        public async Task UserOrders_ShouldReturnAllOrdersForAdmin()
        {
            _mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<IdentityUser>(), "Admin")).ReturnsAsync(true);

            var orders = await _repository.UserOrders(null, null, true);

            Assert.Equal(2, orders.Count());
        }

        [Fact]
        public async Task ChangeOrderStatus_ShouldThrowIfOrderNotFound()
        {
            var updateModel = new UpdateOrderStatusModel { OrderId = 99, OrderStatusId = 1 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.ChangeOrderStatus(updateModel));
        }

        [Fact]
        public async Task TogglePaymentStatus_ShouldThrowIfOrderNotFound()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.TogglePaymentStatus(99));
        }
        [Fact]
        public void OrderDetailModelDTO_ShouldSetAndGetProperties()
        {
            var divId = "TestDiv123";
            List<OrderDetail> orderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = 1,
                    OrderId = 1,
                    BookId = 2,
                    Quantity = 3,
                    UnitPrice = 50.00d
                },
                new OrderDetail
                {
                    Id = 2,
                    OrderId = 2,
                    BookId = 3,
                    Quantity = 1,
                    UnitPrice = 100.0
                }
            };
            OrderDetailModelDTO dto = new OrderDetailModelDTO
            {
                DivId = divId,
                OrderDetail = orderDetails
            };

            Assert.Equal(divId, dto.DivId);
            Assert.Equal(orderDetails, dto.OrderDetail);
        }
    }
}
