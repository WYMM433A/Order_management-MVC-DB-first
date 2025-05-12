using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Order_Web.Models;

namespace Order_Web.Controllers
{
    public class ItemsController : Controller
    {
        private readonly OrderManagementEntities _db = new OrderManagementEntities();

        // GET: Items
        public ActionResult Index()
        {
            var items = _db.Items.ToList();
            return View(items);
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return HttpNotFound();
            var item = _db.Items.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        // GET: Items/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Item item)
        {
            if (ModelState.IsValid)
            {
                _db.Items.Add(item);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: Items/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return HttpNotFound();
            var item = _db.Items.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Item item)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: Items/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return HttpNotFound();
            var item = _db.Items.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var item = _db.Items.Find(id);
            if (item != null)
            {
                _db.Items.Remove(item);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}