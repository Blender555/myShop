using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using System.Security.Claims;
using Utilities;
using X.PagedList.Extensions;

namespace myshop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(int? page)
        {
            var PageNumber = page ?? 1;
            int PageSize = 8;

            var products = _unitOfWork.Product.GetAll().ToPagedList(PageNumber, PageSize);
            return View(products);
        }

        public IActionResult Details(int ProductId)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(i => i.Id == ProductId, IncludeWord: "Category");

            if (product == null)
            {
                return NotFound();
            }

            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = ProductId,
                Product = product,
                Count = 1
            };

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart CartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                 u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId
            );

            if (CartObj == null) 
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Complete();
                HttpContext.Session.SetInt32(SD.SessionKey,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count()
                );

            }
                
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(CartObj, shoppingCart.Count);
                _unitOfWork.Complete();

            }

            return RedirectToAction("Index");
        }
    }
}
