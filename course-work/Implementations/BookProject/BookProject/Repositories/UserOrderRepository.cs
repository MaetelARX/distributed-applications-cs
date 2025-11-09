using BookProject.Models;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookProject.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
            _userManager = userManager;
        }

        public async Task ChangeOrderStatus(UpdateOrderStatusModel data)
        {
            var order = await _db.Orders.FindAsync(data.OrderId);
            if (order == null)
            {
                throw new InvalidOperationException($"order within id: {data.OrderId} is not found");
            }
            order.OrderStatusId = data.OrderStatusId;
            await _db.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _db.Orders.FindAsync(id);
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
            return await _db.OrderStatuses.ToListAsync();
        }

        public async Task TogglePaymentStatus(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"order within id: {orderId} is not found");
            }
            order.IsPaid = true;
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> UserOrders(DateTime? startDate, DateTime? endDate, bool getAll = false)
        {
            var orders = _db.Orders
                .Include(x => x.OrderStatus)
                .Include(x => x.OrderDetail)
                    .ThenInclude(x => x.Book)
                    .ThenInclude(x => x.Genre)
                .AsQueryable();

            var userId = GetUserId();

            if (await IsUserInRole(userId, "Admin"))
            {
                getAll = true;
            }

            if (!getAll)
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User is not logged-in");
                }
                orders = orders.Where(x => x.UserId == userId);
            }

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.CreateDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.CreateDate.Date <= endDate.Value.Date);
            }

            return await orders.ToListAsync();
        }

        private async Task<bool> IsUserInRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.IsInRoleAsync(user, role);
        }



        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
