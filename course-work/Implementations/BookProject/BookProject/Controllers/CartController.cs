using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookProject.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }
        [Authorize]
        public async Task<IActionResult> AddItem(int bookId, int qty=1,int redirect = 0)
        {
            var cartCount = await _cartRepo.AddItem(bookId, qty);
            if(redirect ==0)
            {
                return Ok(cartCount);
            }
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var cartCount = await _cartRepo.RemoveItem(bookId);
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            return View(cart);
        }
        public async Task <IActionResult>GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            return Ok(cartItem);
        }
        public async Task<IActionResult> Checkout()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckOutModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            bool isCheckedOut = await _cartRepo.DoCheckout(model);
            if (!isCheckedOut)
            {
                return RedirectToAction(nameof(OrderFailure));
            }
            return RedirectToAction(nameof(OrderSuccess));
        }
        [HttpGet]
        public async Task<IActionResult> GetBookQuantity(int bookId)
        {
            try
            {
                int quantity = await _cartRepo.GetBookQuantityInCart(bookId);
                return Ok(quantity);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> OrderSuccess()
        {
            return View();
        }
        public async Task<IActionResult> OrderFailure()
        {
            return View();
        }
    }
}
