using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
using Stripe;
using Utilities;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            var orderHeaders = _unitOfWork.OrderHeader.GetAll(IncludeWord: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }

        [HttpGet]
        public IActionResult Details(int orderid)
        {
            OrderVM orderVM = new OrderVM() 
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderid, IncludeWord: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(x => x.OrderHeaderId == orderid, IncludeWord: "Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);
            orderFromDb.Name = OrderVM.OrderHeader.Name;
            orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderFromDb.Address = OrderVM.OrderHeader.Address;
            orderFromDb.City = OrderVM.OrderHeader.City;

            if(OrderVM.OrderHeader.Carrier != null)
            {
                orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Complete();
            TempData["Update"] = "Order has been updated successfully";
            return RedirectToAction("Details", "Order" ,new { orderid = orderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProccess()
        {
            _unitOfWork.OrderHeader.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Processing, null);
            _unitOfWork.Complete();
            TempData["Update"] = "Order status has been updated successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShip()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);
            orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderFromDb.OrderStatus = SD.Shipped;
            orderFromDb.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Complete();

            TempData["Update"] = "Order status has been shipped successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);
            if(orderFromDb.PaymentStatus == SD.Approve)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFromDb.PaymentIntendId,
                };
                var service = new RefundService();
                Refund refund = service.Create(option);

                _unitOfWork.OrderHeader.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Refund);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Cancelled);
            }
            _unitOfWork.Complete();

            TempData["Update"] = "Order status has been cancelled successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }
    }
}
