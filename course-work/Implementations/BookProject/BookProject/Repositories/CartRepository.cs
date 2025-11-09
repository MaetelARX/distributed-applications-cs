using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isTestEnvironment;
        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor, bool isTestEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _isTestEnvironment = isTestEnvironment;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not logged-in");
            }

            using var transaction = !_isTestEnvironment ? _db.Database.BeginTransaction() : null;

            try
            {
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                    await _db.SaveChangesAsync();
                }
                var cartItem = await _db.CartDetails
                    .FirstOrDefaultAsync(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);

                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = await _db.Books.FindAsync(bookId);
                    if (book == null)
                    {
                        throw new InvalidOperationException("Book does not exist");
                    }

                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }

                await _db.SaveChangesAsync();

                if (!_isTestEnvironment)
                {
                    transaction?.Commit();
                }
            }
            catch
            {
                if (!_isTestEnvironment)
                {
                    transaction?.Rollback();
                }
                throw;
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        public async Task<int> RemoveItem(int bookId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("user is not logged-in");
                }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new InvalidOperationException("Invalid cart");
                }
                var cartItem = _db.CartDetails.FirstOrDefault(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);
                if(cartItem is null)
                {
                    throw new InvalidOperationException("No items in cart");
                }

                else if(cartItem.Quantity ==1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;
                }
                _db.SaveChanges();
                //transaction.Commit();
            }
            catch (Exception ex)
            {
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new InvalidOperationException("Invalid userId");
            }
            var shoppingCart = await _db.ShoppingCarts
				.Include(x => x.CartDetails)
				.ThenInclude(x => x.Book)
				.ThenInclude(x => x.Stock)
				.Include(x => x.CartDetails)
                .ThenInclude(x => x.Book)
                .ThenInclude(x => x.Genre)
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();
            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { cartDetail.Id }).ToListAsync();

            return data.Count;

        }
        public async Task<bool> DoCheckout(CheckOutModel model)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User is not logged in.");
                }

                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new InvalidOperationException("Invalid cart.");
                }

                var cartDetails = await _db.CartDetails
                    .Where(x => x.ShoppingCartId == cart.Id)
                    .ToListAsync();

                if (!cartDetails.Any())
                {
                    throw new InvalidOperationException("Cart is empty.");
                }
                var pendingRecord = _db.OrderStatuses.FirstOrDefault(x => x.StatusName == "Pending");
                if (pendingRecord is null)
                {
                    throw new InvalidOperationException("Order status does not have Pending status");
                }
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
                foreach (var item in cartDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _db.OrderDetails.Add(orderDetail);

                    var stock = await _db.Stocks.FirstOrDefaultAsync(x =>
                    x.BookId == item.BookId);

                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items are available in the stock");
                    }
                    stock.Quantity -= item.Quantity;
                }
                //await _db.SaveChangesAsync();
                _db.CartDetails.RemoveRange(cartDetails);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Checkout failed: {ex.Message}");
                return false;
            }
        }
        public async Task<int> GetBookQuantityInCart(int bookId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }

            var cart = await GetCart(userId);
            if (cart == null)
            {
                return 0;
            }

            var cartItem = await _db.CartDetails
                .Where(cd => cd.ShoppingCartId == cart.Id && cd.BookId == bookId)
                .FirstOrDefaultAsync();

            return cartItem?.Quantity ?? 0;
        }



        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

    }
}
