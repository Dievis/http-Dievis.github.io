using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using ShopBanDoDienTu_Nhom1.Models;

namespace ShopBanDoDienTu_Nhom1.Controllers
{
    public class ProductsController : Controller
    {
        // GET: Products
        public ActionResult Index(int page = 1)
        {
            MyDBContext db = new MyDBContext();
            List<Product> products = db.Products.ToList();
            return View(products);
        }

        public ActionResult Detail(int id)
        {
            MyDBContext db = new MyDBContext();
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult FilterByBrand(int brandId)
        {
            MyDBContext db = new MyDBContext();
            List<Product> productsByBrand = db.Products.Where(p => p.BrandId == brandId).ToList();

            return View("FilterByBrand", productsByBrand); // Sử dụng view Index để hiển thị danh sách sản phẩm đã lọc theo brand
        }


        public ActionResult FilterByBrandList()
        {
            MyDBContext db = new MyDBContext();
            List<Brand> brands = db.Brands.ToList();
            ViewBag.Brands = brands;
            return View(brands);
        }
    }
}
