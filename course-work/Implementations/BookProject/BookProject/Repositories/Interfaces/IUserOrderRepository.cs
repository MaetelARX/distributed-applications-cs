namespace BookProject.Repositories.Interfaces
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOrders(DateTime? startDate, DateTime? endDate, bool getAll = false);
        Task ChangeOrderStatus(UpdateOrderStatusModel data);
        Task TogglePaymentStatus(int orderId);
        Task<Order?> GetOrderById(int id);
        Task<IEnumerable<OrderStatus>> GetOrderStatuses();
    }
}