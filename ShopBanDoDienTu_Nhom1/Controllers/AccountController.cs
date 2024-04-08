using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopBanDoDienTu_Nhom1.Models;
using ShopBanDoDienTu_Nhom1.ViewModel;
using ShopBanDoDienTu_Nhom1.Identity;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;


namespace ShopBanDoDienTu_Nhom1.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterVM rvm)
        {
            if (ModelState.IsValid)
            {
                var AppDBContext = new AppDBContext();
                var UserStore = new AppUserStore(AppDBContext);
                var UserManager = new AppUserManager(UserStore);
                var PasswordHash = Crypto.HashPassword(rvm.Password);
                var user = new AppUser()
                {
                    Email = rvm.Email,
                    UserName = rvm.Username,
                    PasswordHash = PasswordHash,
                    City = rvm.City,
                    Birthday = rvm.DateOfBirth,
                    Address = rvm.Address,
                    PhoneNumber = rvm.Mobile
                };
                IdentityResult identityResult = UserManager.Create(user);
                if (identityResult.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Customer");

                    var authenManager = HttpContext.GetOwinContext().Authentication;
                    var userIdentity = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                    authenManager.SignIn(new AuthenticationProperties(), userIdentity);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("New Error", "Invalid Data");
                return View();
            }

        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginVM lvm)
        {
            var AppDBContext = new AppDBContext();
            var UserStore = new AppUserStore(AppDBContext);
            var UserManager = new AppUserManager(UserStore);
            var user = UserManager.Find(lvm.Username, lvm.Password);
            if (user != null)
            {
                var authenManager = HttpContext.GetOwinContext().Authentication;
                var userIdentity = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                authenManager.SignIn(new AuthenticationProperties(), userIdentity);
                if (UserManager.IsInRole(user.Id, "Admin"))
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else if (UserManager.IsInRole(user.Id, "Manager")) 
                {
                    return RedirectToAction("Index", "Home", new { area = "Manager" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("NewError", "Tài khoản hoặc Mật khẩu sai.");
                return View();
            }
        }

        public ActionResult Logout()
        {
            var authenManager = HttpContext.GetOwinContext().Authentication;
            authenManager.SignOut();

            // Xóa tất cả sản phẩm trong giỏ hàng khi người dùng đăng xuất
            var cart = ShoppingCart.GetCart(HttpContext);
            cart.ClearCart();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult UserProfile()
        {
            var userId = User.Identity.GetUserId();
            var AppDBContext = new AppDBContext();
            var UserStore = new AppUserStore(AppDBContext);
            var UserManager = new AppUserManager(UserStore);
            var user = UserManager.FindById(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var userProfile = new UserProfileVM
            {
                Username = user.UserName,
                Email = user.Email,
                City = user.City,
                DateOfBirth = user.Birthday,
                Address = user.Address,
                Mobile = user.PhoneNumber
            };

            return View(userProfile);
        }

        public ActionResult EditUserProfile()
        {
            var userId = User.Identity.GetUserId();
            var AppDBContext = new AppDBContext();
            var UserStore = new AppUserStore(AppDBContext);
            var UserManager = new AppUserManager(UserStore);
            var user = UserManager.FindById(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var userProfile = new UserProfileVM
            {
                Username = user.UserName,
                Email = user.Email,
                City = user.City,
                DateOfBirth = user.Birthday,
                Address = user.Address,
                Mobile = user.PhoneNumber
            };

            return View(userProfile);
        }

        [HttpPost]
        public ActionResult EditUserProfile(UserProfileVM pvm)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var AppDBContext = new AppDBContext();
                var UserStore = new AppUserStore(AppDBContext);
                var UserManager = new AppUserManager(UserStore);
                var user = UserManager.FindById(userId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update user profile information
                user.City = pvm.City;
                user.Birthday = pvm.DateOfBirth;
                user.Address = pvm.Address;
                user.PhoneNumber = pvm.Mobile;

                // Save changes to the database
                UserManager.Update(user);

                return RedirectToAction("UserProfile");
            }

            // If model state is not valid, return the view with errors
            return View(pvm);
        }

        //-------------------------------------------------------------------------------------//

        public ActionResult EmailSubmit(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                ViewBag.ErrorMessage = "Please provide an email address.";
                return View("EmailSubmit");
            }

            if (!IsEmailExists(emailAddress))
            {
                ViewBag.ErrorMessage = "Email does not exist in the database.";
                return View("EmailSubmit");
            }

            try
            {
                var fromAddress = new MailAddress("nguyenbacuong161103@gmail.com");
                const string fromPassword = "ccuvaurhunsmovum";
                var toAddress = new MailAddress(emailAddress);
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                const string subject = "Reset Password Link";
                string resetPasswordLink = Url.Action("ChangePasswd", "Account", new { emailAddress = emailAddress }, Request.Url.Scheme);
                string body = "<p>bạn quên mật khẩu </p>";
                body += $"<a href='{resetPasswordLink}'>Vô nhập mật khẩu mới</a>";

                // Tạo đối tượng MailMessage với thông tin từ, tới, tiêu đề và nội dung của email
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Body = body,
                    Subject = subject,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
                TempData["ResetEmail"] = emailAddress;
                ViewBag.SuccessMessage = "OTP đã được gửi qua email";
                return Redirect("mailto:" + emailAddress);
            }
            catch (Exception ex)
            {
                // Gửi thông báo lỗi nếu có lỗi xảy ra
                ViewBag.ErrorMessage = ex.Message;
            }
            return RedirectToAction("Login", "Account");
        }

        public bool IsEmailExists(string email)
        {
            var AppDBContext = new AppDBContext();
            var UserStore = new AppUserStore(AppDBContext);
            var UserManager = new AppUserManager(UserStore);

            // Kiểm tra xem có người dùng nào sử dụng địa chỉ email đã cho không
            var user = UserManager.FindByEmail(email);

            // Nếu có người dùng sử dụng địa chỉ email này, trả về true, ngược lại trả về false
            return user != null;
        }
        
    }

}
