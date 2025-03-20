using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess.Implementation;
using myshop.Entities.Repositories;
using Utilities;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            ViewBag.Orders = unitOfWork.OrderHeader.GetAll().Count();
            ViewBag.ApprovedOrders = unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == SD.Approve).Count();
            ViewBag.Users = unitOfWork.ApplicationUser.GetAll().Count();
            ViewBag.Products = unitOfWork.Product.GetAll().Count();
            return View();
        }
    }
}
