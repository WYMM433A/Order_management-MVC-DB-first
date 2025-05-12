using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;          // strongly-typed Include / AsNoTracking
using Order_Web.Models;

namespace Order_Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderManagementEntities _db = new OrderManagementEntities();

        /* ─────────────────────────── ADD / EDIT ─────────────────────────── */

        public ActionResult AddOrder(int? orderId)
        {
            Order order = orderId.HasValue
                ? _db.Orders
                     .Include(o => o.OrderDetails)
                     .FirstOrDefault(o => o.OrderID == orderId.Value)
                : new Order();

            if (orderId.HasValue && order == null) return HttpNotFound();

            ViewBag.AgentID = new SelectList(_db.Agents.AsNoTracking(),
                                             "AgentID", "AgentName", order.AgentID);

            ViewBag.ItemID = new SelectList(_db.Items.AsNoTracking(),
                                             "ItemID", "ItemName");

            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddOrder(Order order, int[] ItemID, int[] Quantity)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AgentID = new SelectList(_db.Agents.AsNoTracking(),
                                                 "AgentID", "AgentName", order.AgentID);
                ViewBag.ItemID = new SelectList(_db.Items.AsNoTracking(),
                                                 "ItemID", "ItemName");
                return View(order);
            }

            if (order.OrderID > 0)                    /* EDIT */
            {
                var dbOrder = _db.Orders
                                 .Include(o => o.OrderDetails)
                                 .FirstOrDefault(o => o.OrderID == order.OrderID);

                if (dbOrder == null) return HttpNotFound();

                dbOrder.AgentID = order.AgentID;
                dbOrder.OrderDate = order.OrderDate ?? DateTime.Now;

                _db.OrderDetails.RemoveRange(dbOrder.OrderDetails);
                for (int i = 0; i < ItemID.Length; i++)
                {
                    dbOrder.OrderDetails.Add(new OrderDetail
                    {
                        ItemID = ItemID[i],
                        Quantity = Quantity[i]
                    });
                }
            }
            else                                       /* ADD */
            {
                order.OrderDate = DateTime.Now;
                _db.Orders.Add(order);
                _db.SaveChanges();                     // get new OrderID

                for (int i = 0; i < ItemID.Length; i++)
                {
                    _db.OrderDetails.Add(new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ItemID = ItemID[i],
                        Quantity = Quantity[i]
                    });
                }
            }

            _db.SaveChanges();
            return RedirectToAction("DisplayOrders");
        }

        /* ─────────────────────────── LIST ─────────────────────────── */

        public ActionResult DisplayOrders(string agentNameFilter = null)
        {
            var query = _db.Orders
                           .Include(o => o.Agent)
                           .Include(o => o.OrderDetails.Select(d => d.Item))
                           .Where(o => o.OrderDate != null);

            if (!string.IsNullOrWhiteSpace(agentNameFilter))
                query = query.Where(o => o.Agent.AgentName.Contains(agentNameFilter));

            var orders = query
                         .OrderByDescending(o => o.OrderDate)
                         .AsNoTracking()               // read-only → faster
                         .ToList();

            return View(orders);                       // view now shows AgentName
        }

        /* ─────────────────────────── DELETE ─────────────────────────── */

        [HttpGet]
        public ActionResult Delete(int orderId)
        {
            var order = _db.Orders
                           .Include(o => o.OrderDetails)
                           .FirstOrDefault(o => o.OrderID == orderId);
            return order == null ? (ActionResult)HttpNotFound() : View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int orderId)
        {
            var order = _db.Orders
                           .Include(o => o.OrderDetails)
                           .FirstOrDefault(o => o.OrderID == orderId);

            if (order != null)
            {
                _db.OrderDetails.RemoveRange(order.OrderDetails);
                _db.Orders.Remove(order);
                _db.SaveChanges();
            }
            return RedirectToAction("DisplayOrders");
        }

        /* ─────────────────────────── CLEANUP ─────────────────────────── */

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
