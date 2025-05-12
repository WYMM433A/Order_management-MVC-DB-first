using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Order_Web.Models;
using System.Collections.Generic;

namespace Order_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly OrderManagementEntities _db = new OrderManagementEntities();

        /* ───────────────────────────  DASHBOARD  ─────────────────────────── */

        public ActionResult Index(int? agentId = null, int? itemId = null)
        {
            /* 1) Best-selling item of the *current* month -------------------- */
            DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = start.AddMonths(1);

            var bestItem = _db.OrderDetails
                              .Include(od => od.Item)
                              .Include(od => od.Order)
                              .Where(od => od.Order.OrderDate >= start &&
                                           od.Order.OrderDate < end)
                              .GroupBy(od => new
                              {
                                  od.ItemID,
                                  od.Item.ItemName,
                                  od.Item.UnitPrice               // decimal  ✔
                              })
                              .Select(g => new BestItemVm
                              {
                                  ItemID = g.Key.ItemID ?? 0,
                                  ItemName = g.Key.ItemName,
                                  UnitPrice = g.Key.UnitPrice,
                                  Quantity = g.Sum(d => d.Quantity ?? 0),
                                  Total = g.Sum(d => (d.Quantity ?? 0) *
                                                         g.Key.UnitPrice) // no “??”
                              })
                              .OrderByDescending(b => b.Quantity)
                              .FirstOrDefault();

            /* 2) Items purchased by a chosen *agent* ------------------------ */
            IList<ItemsByAgentVm> itemsByAgent = new List<ItemsByAgentVm>();
            if (agentId.HasValue)
            {
                itemsByAgent = _db.OrderDetails
                                  .Include(od => od.Item)
                                  .Include(od => od.Order)
                                  .Where(od => od.Order.AgentID == agentId.Value)
                                  .GroupBy(od => new
                                  {
                                      od.Item.ItemName,
                                      od.Item.UnitPrice
                                  })
                                  .Select(g => new ItemsByAgentVm
                                  {
                                      ItemName = g.Key.ItemName,
                                      UnitPrice = g.Key.UnitPrice,
                                      Quantity = g.Sum(d => d.Quantity ?? 0),
                                      Total = g.Sum(d => (d.Quantity ?? 0) *
                                                             g.Key.UnitPrice)
                                  })
                                  .OrderByDescending(r => r.Quantity)
                                  .ToList();
            }

            /* 3) Agents who bought a chosen *item* -------------------------- */
            IList<AgentsByItemVm> agentsByItem = new List<AgentsByItemVm>();
            if (itemId.HasValue)
            {
                agentsByItem = _db.OrderDetails
                                  .Include(od => od.Order.Agent)
                                  .Include(od => od.Order)
                                  .Where(od => od.ItemID == itemId.Value)
                                  .Select(od => new AgentsByItemVm
                                  {
                                      AgentName = od.Order.Agent.AgentName,
                                      OrderDate = od.Order.OrderDate,
                                      Quantity = od.Quantity ?? 0
                                  })
                                  .OrderByDescending(r => r.OrderDate)
                                  .ToList();
            }

            /* dropdown sources */
            var agents = new SelectList(_db.Agents.AsNoTracking()
                                                  .OrderBy(a => a.AgentName),
                                        "AgentID", "AgentName", agentId);

            var items = new SelectList(_db.Items.AsNoTracking()
                                                 .OrderBy(i => i.ItemName),
                                        "ItemID", "ItemName", itemId);

            var vm = new HomeDashboardVm
            {
                BestItem = bestItem,
                ItemsByAgent = itemsByAgent,
                AgentsByItem = agentsByItem,
                AgentsList = agents,
                ItemsList = items,
                SelectedAgentId = agentId,
                SelectedItemId = itemId
            };

            return View(vm);
        }

        /* scaffold pages unchanged */
        public ActionResult About() { ViewBag.Message = "Your application description page."; return View(); }
        public ActionResult Contact() { ViewBag.Message = "Your contact page."; return View(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }

    /* ──────────────  tiny dashboard DTOs  ────────────── */
    public class BestItemVm
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
    public class ItemsByAgentVm
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
    public class AgentsByItemVm
    {
        public string AgentName { get; set; }
        public DateTime? OrderDate { get; set; }
        public int Quantity { get; set; }
    }
    public class HomeDashboardVm
    {
        public BestItemVm BestItem { get; set; }
        public IList<ItemsByAgentVm> ItemsByAgent { get; set; }
        public IList<AgentsByItemVm> AgentsByItem { get; set; }
        public SelectList AgentsList { get; set; }
        public SelectList ItemsList { get; set; }
        public int? SelectedAgentId { get; set; }
        public int? SelectedItemId { get; set; }
    }
}
