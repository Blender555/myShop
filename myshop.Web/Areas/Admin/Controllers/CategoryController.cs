using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess;
using myshop.Entities.Models;
using myshop.Entities.Repositories;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                //_context.Categories.Add(category);
                _unitOfWork.Category.Add(category);

                //_context.SaveChanges();
                _unitOfWork.Complete();

                TempData["Create"] = "Data has been created succesfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0) NotFound();
            //var category = _context.Categories.Find(id);
            var categoryIndb = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(categoryIndb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                //_context.SaveChanges();
                _unitOfWork.Complete();
                TempData["Update"] = "Data has been updated succesfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null | id == 0) NotFound();
            var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int? id)
        {
            var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            if (category == null) return NotFound();
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Complete();
            TempData["Delete"] = "Data has been deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
