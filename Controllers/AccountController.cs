using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Order_Web.Models;

namespace Order_Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly OrderManagementEntities _db = new OrderManagementEntities();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == username && u.Password == password && u.Lock != true);
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(username, false);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Invalid login attempt.";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}