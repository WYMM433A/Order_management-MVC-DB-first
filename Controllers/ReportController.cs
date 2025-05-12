using System;
using System.Data;                 // DataTable
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using Order_Web;
using Order_Web.Models;
using System.Data.Entity;
using System.Linq;// Include

public class ReportsController : Controller
{
    private readonly OrderManagementEntities _db = new OrderManagementEntities();

    public ActionResult PrintOrder(int orderId)
    {
        /* -------- pull data -------- */
        var order = _db.Orders
                       .Include(o => o.Agent)
                       .Include(o => o.OrderDetails.Select(d => d.Item)) // Ensure proper usage of LINQ Select
                       .FirstOrDefault(o => o.OrderID == orderId);
        if (order == null) return HttpNotFound();

        /* -------- fill typed dataset -------- */
        var ds = new OrderReportData();            // generated class
        var header = ds.OrderHeader.NewOrderHeaderRow();
        header.OrderID = order.OrderID;
        header.OrderDate = order.OrderDate ?? DateTime.Now;
        header.AgentName = order.Agent.AgentName;
        header.Total = order.OrderDetails.Sum(d => (d.Quantity ?? 0) * d.Item.UnitPrice);
        ds.OrderHeader.AddOrderHeaderRow(header);

        foreach (var d in order.OrderDetails)
        {
            var row = ds.OrderLines.NewOrderLinesRow();
            row.OrderID = order.OrderID;
            row.ItemName = d.Item.ItemName;
            row.Quantity = d.Quantity ?? 0;
            row.UnitPrice = d.Item.UnitPrice;
            row.LineTotal = row.Quantity * row.UnitPrice;
            ds.OrderLines.AddOrderLinesRow(row);
        }

        /* -------- render RDLC to PDF -------- */
        LocalReport report = new LocalReport
        {
            ReportPath = Server.MapPath("~/Order.rdlc")
        };

        // Explicitly cast the data source to DataTable to resolve ambiguity
        report.DataSources.Add(new ReportDataSource("OrderHeader", (DataTable)ds.OrderHeader));
        report.DataSources.Add(new ReportDataSource("OrderLines", (DataTable)ds.OrderLines));

        string mimeType, encoding, fileExt;
        Warning[] warnings; string[] streams;
        byte[] bytes = report.Render(
            "PDF", null, out mimeType, out encoding, out fileExt, out streams, out warnings);

        return File(bytes, mimeType, $"Order_{orderId}.pdf");
    }
}
