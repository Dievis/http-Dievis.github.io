using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShopBanDoDienTu_Nhom1.Identity;
using ShopBanDoDienTu_Nhom1.ViewModel;

namespace ShopBanDoDienTu_Nhom1.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        // GET: Admin/Users
        public ActionResult Index()
        {
            AppDBContext db = new AppDBContext();
            List<AppUser> users = db.Users.ToList();
            return View(users);
        }

        // GET: Account
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(RegisterVM rvm)
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
                    return RedirectToAction("Index", "Users");
                }
            }
            // Đảm bảo rằng ModelState sẽ được cập nhật đúng nếu có lỗi xảy ra
            ModelState.AddModelError("New Error", "Invalid Data");
            return View();
        }

        // GET: Admin/Users/Delete
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var UserManager = new AppUserManager(new AppUserStore(new AppDBContext()));
            var user = UserManager.FindById(id);

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var UserManager = new AppUserManager(new AppUserStore(new AppDBContext()));
            var user = UserManager.FindById(id);
            UserManager.Delete(user);

            return RedirectToAction("Index");
        }


    }
}