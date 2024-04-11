using System;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShopBanDoDienTu_Nhom1.Models;
using ShopBanDoDienTu_Nhom1.ViewModel;

namespace ShopBanDoDienTu_Nhom1.Controllers
{
    public class CheckoutController : Controller
    {
        private MyDBContext db = new MyDBContext();

        // GET: /Checkout
        public ActionResult Index(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Session["RedirectUrl"] = Url.Action("Index", new { id = id });
                return RedirectToAction("Login", "Account");
            }

            var cart = ShoppingCart.GetCart(this.HttpContext);

            var viewModel = new ShoppingCartVM
            {
                CartItems = cart.CartItems.Select(item => new CartItemVM
                {
                    ProductId = item.Product.ProductId,
                    ProductName = item.Product.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Product.Price ?? 0m,
                    TotalPrice = (item.Quantity * (item.Product.Price ?? 0m))
                }).ToList(),
                CartTotal = cart.CartItems.Sum(item => item.Product.Price * item.Quantity ?? 0m)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteOrder(string shippingAddress, string notes)
        {
            // Lấy giỏ hàng từ Session
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Lấy thông tin người dùng đang đăng nhập
            var currentUser = db.Users.Find(User.Identity.GetUserId());

            // Tạo một đơn hàng mới
            var order = new Order
            {
                UserId = currentUser.Id, // Lấy ID của người dùng đang đăng nhập
                Email = currentUser.Email, // Sử dụng email của người dùng đăng nhập
                OrderDate = DateTime.Now,
                ShippingAddress = shippingAddress,
                Notes = notes,
                TotalPrice = cart.GetTotal() // Tính tổng giá tiền từ giỏ hàng
            };

            // Lưu đơn hàng vào cơ sở dữ liệu
            db.Orders.Add(order);
            db.SaveChanges();

            // Lưu chi tiết đơn hàng
            foreach (var item in cart.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.Product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price ?? 0m
                };
                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges();

            // Xóa giỏ hàng sau khi đã thanh toán
            cart.ClearCart();

            TempData["Message"] = "Đơn hàng của bạn đã được đặt thành công.";

            return RedirectToAction("Index", "Home");
        }

    }
}
