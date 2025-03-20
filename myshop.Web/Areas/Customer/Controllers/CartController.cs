using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
using Stripe.Checkout;
using System.Security.Claims;
using Utilities;

namespace myshop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int TotalCarts { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
                OrderHeader = new()
            };

            foreach(var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.TotalCarts += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;

            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult POSTSummary(ShoppingCartVM shoppingCartVM)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM.CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product");

            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            shoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var item in shoppingCartVM.CartsList)
            {
                shoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Complete();

            foreach(var item in shoppingCartVM.CartsList)
            {
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.Complete();
            }

            var domain = "https://localhost:7086";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"/customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "/customer/cart/index"
            };


            foreach (var item in shoppingCartVM.CartsList)
            {
                var sessionlineoption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionlineoption);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            shoppingCartVM.OrderHeader.SessionId = session.Id;

            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId); 

            if (session.PaymentStatus.ToLower() == "paid") 
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(id, SD.Approve, SD.Approve);
                orderHeader.PaymentIntendId = session.PaymentIntentId;
                _unitOfWork.Complete();
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Complete();
            return View(id);
        }

        public IActionResult Plus(int cartid)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartid);
            _unitOfWork.ShoppingCart.IncreaseCount(cart, 1);
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartid)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartid);

            if(cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreseCount(cart, 1);
                _unitOfWork.Complete();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartid)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartid, IncludeWord: "Product");
            if(cart != null)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Complete();
            }
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return RedirectToAction("Index");
        }
    }
}
