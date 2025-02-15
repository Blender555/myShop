using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;

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

        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart obj = new ShoppingCart()
            {
                Product = _unitOfWork.Product.GetFirstOrDefault(i => i.Id == id, IncludeWord: "Category"),
                Count = 1
            };
            return View(obj);
        }
    }
}
