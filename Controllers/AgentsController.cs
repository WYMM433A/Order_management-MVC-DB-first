using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Order_Web.Models;

namespace Order_Web.Controllers
{
    public class AgentsController : Controller
    {
        private readonly OrderManagementEntities _db = new OrderManagementEntities();

        // GET: Agents
        public ActionResult Index()
        {
            var agents = _db.Agents.ToList();
            return View(agents);
        }

        // GET: Agents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return HttpNotFound();
            var agent = _db.Agents.Find(id);
            if (agent == null) return HttpNotFound();
            return View(agent);
        }

        // GET: Agents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Agents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Agent agent)
        {
            if (ModelState.IsValid)
            {
                _db.Agents.Add(agent);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        // GET: Agents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return HttpNotFound();
            var agent = _db.Agents.Find(id);
            if (agent == null) return HttpNotFound();
            return View(agent);
        }

        // POST: Agents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Agent agent)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(agent).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        // GET: Agents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return HttpNotFound();
            var agent = _db.Agents.Find(id);
            if (agent == null) return HttpNotFound();
            return View(agent);
        }

        // POST: Agents/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var agent = _db.Agents.Find(id);
            if (agent != null)
            {
                _db.Agents.Remove(agent);
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