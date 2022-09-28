using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AT_System.Models;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.ComponentModel;

namespace AT_System.Controllers
{
    
    public class ReportController : Controller
    {
        private SAPEntities db = new SAPEntities();

        //
        // GET: /Report/
        [HttpGet]
        public ActionResult Report(string AssetClassFrom = "", string AssetClassTo = "", string AssetNOFrom = "", string AssetNOTo = "", string DocNO = "",
                                    string CreateDateFrom = null, string CreateDateTO = null, string AllOrCompleted = "All", string Excel = "")
        {

            //Check Login Session
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            string filterCreateDateFrom, filterCreateDateTo;
            string filterAssetClassTo = "";
            string filterAssetNOTo = "";

            

            DateTime date = DateTime.Today;
            if (CreateDateFrom == null && CreateDateTO == null)
            {                
                DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                //--- Return datetime with format "dd/MM/yyyy"
                ViewBag.CreateDateFrom = firstDayOfMonth.Day.ToString().PadLeft(2,'0') + "/" + firstDayOfMonth.Month.ToString().PadLeft(2, '0') + "/" + firstDayOfMonth.Year;
                ViewBag.CreateDateTO = lastDayOfMonth.Day.ToString().PadLeft(2, '0') + "/" + lastDayOfMonth.Month.ToString().PadLeft(2, '0') + "/" + lastDayOfMonth.Year;

                filterCreateDateFrom = firstDayOfMonth.Year.ToString() + firstDayOfMonth.Month.ToString().PadLeft(2, '0') + firstDayOfMonth.Day.ToString().PadLeft(2, '0');
                filterCreateDateTo = lastDayOfMonth.Year.ToString() + lastDayOfMonth.Month.ToString().PadLeft(2, '0') + lastDayOfMonth.Day.ToString().PadLeft(2, '0');
                
            }
            else
            {
                ViewBag.CreateDateFrom = CreateDateFrom;
                ViewBag.CreateDateTO = CreateDateTO;

                filterCreateDateFrom = CreateDateFrom.Substring(6, 4) + CreateDateFrom.Substring(3, 2) + CreateDateFrom.Substring(0, 2);
                filterCreateDateTo = CreateDateTO.Substring(6, 4) + CreateDateTO.Substring(3, 2) + CreateDateTO.Substring(0, 2);
            }

            if (AssetClassFrom != "" && AssetClassTo == "") { filterAssetClassTo = AssetClassFrom; }
            if (AssetNOFrom != "" && AssetNOTo == "") { filterAssetNOTo = AssetNOFrom; }


            List<vAssetTransfer_Report> queryATReport = db.vAssetTransfer_Report
                .Where(e => e.Client == Properties.Settings.Default.Client
                    && ((String.Compare(e.CreateDate, filterCreateDateFrom) >= 0) && (String.Compare(e.CreateDate, filterCreateDateTo) <= 0))                    
                 )
                .OrderBy(e => new { e.DocNo, e.ItemNo })
                .ToList();

            if (DocNO != "") {
                queryATReport = queryATReport.Where(e => e.DocNo.Contains(DocNO)).ToList();
            }

            if (AllOrCompleted == "Approved")
            {
                queryATReport = queryATReport.Where(e => e.DocStatus == "Approve").ToList();
            }
            else if (AllOrCompleted == "Rejected") {
                queryATReport = queryATReport.Where(e => e.DocStatus == "Reject").ToList();
            }
            else if (AllOrCompleted == "Issued") {
                queryATReport = queryATReport.Where(e => e.DocStatus == "Issue").ToList();
            }

            if ((AssetClassFrom != "" && AssetClassTo == "") || (AssetClassFrom == "" && AssetClassTo != ""))
            {
                queryATReport = queryATReport.Where(e => e.AssetClass == AssetClassFrom || e.AssetClass == AssetClassTo).ToList();                
            }
            else if(AssetClassFrom != "" && AssetClassTo != "") {
                queryATReport = queryATReport.Where(e => (String.Compare(e.AssetClass, AssetClassFrom) >= 0) && (String.Compare(e.AssetClass, AssetClassTo) <= 0)).ToList();
            }

            if ((AssetNOFrom != "" && AssetNOTo == "") || (AssetNOFrom == "" && AssetNOTo != ""))
            {
                queryATReport = queryATReport.Where(e => e.Asset == AssetNOFrom || e.Asset == AssetNOTo).ToList();
            }
            else if (AssetNOFrom != "" && AssetNOTo != "")
            {
                queryATReport = queryATReport.Where(e => (String.Compare(e.Asset, AssetNOFrom) >= 0) && (String.Compare(e.Asset, AssetNOTo) <= 0)).ToList();
            }



            ViewBag.AssetClassFrom = AssetClassFrom;
            ViewBag.AssetClassTo = AssetClassTo;
            ViewBag.AssetNOFrom = AssetNOFrom;
            ViewBag.AssetNOTo = AssetNOTo;
            ViewBag.DocNO = DocNO;
            ViewBag.AllORCompleted = AllOrCompleted;
            ViewBag.ResultRows = queryATReport.Count();



            if (Excel == "Excel") {
                string hdRptDate = ViewBag.CreateDateFrom + " - " + ViewBag.CreateDateTo;
                string hdCreateOn = date.Day.ToString().PadLeft(2,'0') + "/" + date.Month.ToString().PadLeft(2,'0') + "/" + date.Year.ToString();

                int no = 1;                
                var queryATReportExcel = queryATReport.Select(e => new { No = no++, e.DocNo, e.DocStatus, e.ItemNo, e.AssetClass, e.Asset, e.AssetDescription, e.CostCenter, e.Room , e.Qty, e.RecCostCenter,
                                                                            e.CostCenter2, e.Room2, e.Remark, e.IssuerAppDate,e.IssuerAppTime, e.IssuerName, e.TopIssuerAppDate,e.TopIssuerAppTime, e.TopIssuerName,
                                                                            e.RecieverAppDate,e.RecieverAppTime, e.RecieverName, e.TopRecieverAppDate,e.TopRecieverAppTime, e.TopRecieverName,
                                                                            e.AFAppDate,e.AFAppTime,e.AFName}).ToList();
               
                GridView gv = new GridView();
                gv.DataSource = queryATReportExcel;
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=AssetTransfer_Report.xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "utf-16";
                StringWriter sw = new StringWriter();

                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    sw.Write("<meta charset='utf-8' />");
                    sw.Write("<font size='20pt'><b>Asset Transfer Report</b></font>" + "<br />");
                    sw.Write("Report Date : " + hdRptDate + "<br />");
                    sw.Write("Crate on : " + hdCreateOn + "<br />");

                    sw.Write("<table border='1'>");
                    sw.Write("<tr>");
                    sw.Write("<th rowspan='2' bgcolor='#92CDDC' font-color='white'>NO.</th>");
                    sw.Write("<th rowspan='2' bgcolor='#92CDDC' font-color='white'>Status</th>");
                    sw.Write("<th colspan='9' bgcolor='#92CDDC'>Sender</th>");
                    sw.Write("<th colspan='2' bgcolor='#C4D79B'>Reciever</th>");
                    sw.Write("<th rowspan='2' bgcolor='#D9D9D9'>Remark</th>");
                    sw.Write("<th colspan='3' bgcolor='#FDE9D9'>Issuer</th>");
                    sw.Write("<th colspan='3' bgcolor='#FDE9D9'>Top Section</th>");
                    sw.Write("<th colspan='3' bgcolor='#FDE9D9'>Reciever</th>");
                    sw.Write("<th colspan='3' bgcolor='#FDE9D9'>Top Section</th>");
                    sw.Write("<th colspan='3' bgcolor='#FDE9D9'>A&F</th>");
                    sw.Write("</tr>");

                    sw.Write("<tr>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Doc NO.</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Item NO.</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Asset Class</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Asset</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Asset Description</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Cost Center</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Room</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Qty</th>");
                    sw.Write("<th bgcolor='#92CDDC' font-color='white'>Department</th>");

                    sw.Write("<th bgcolor='#C4D79B'>Cost Center(SAP)</th>");
                    sw.Write("<th bgcolor='#C4D79B'>Room</th>");

                    sw.Write("<th bgcolor='#FDE9D9'>Date</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Time</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Name</th>");

                    sw.Write("<th bgcolor='#FDE9D9'>Date</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Time</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Name</th>");

                    sw.Write("<th bgcolor='#FDE9D9'>Date</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Time</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Name</th>");

                    sw.Write("<th bgcolor='#FDE9D9'>Date</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Time</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Name</th>");

                    sw.Write("<th bgcolor='#FDE9D9'>Date</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Time</th>");
                    sw.Write("<th bgcolor='#FDE9D9'>Name</th>");
                    sw.Write("</tr>");

                    string bgcolor = "#EEECE1";
                    string statusColor = "blue";
                    foreach (var item in queryATReportExcel) {

                        if (item.ItemNo == 1) {
                            bgcolor = (bgcolor == "white" ? "#EEECE1" : "white");
                        }

                        if (item.DocStatus == "Reject") { statusColor = "red"; }
                        else if (item.DocStatus == "Approve") { statusColor = "green"; }
                        else if (item.DocStatus == "Issue") { statusColor = "blue"; }

                        sw.Write("<tr valign='top'>");

                        sw.Write("<td>" + item.No + "</td>");
                        sw.Write("<td style='color:" + statusColor + "'>" + item.DocStatus + "</td>");

                        sw.Write("<td>" + item.DocNo + "</td>");
                        sw.Write("<td>" + item.ItemNo + "</td>");
                        sw.Write("<td>" + item.AssetClass + "</td>");
                        sw.Write("<td>" + item.Asset + "</td>");
                        sw.Write("<td>" + item.AssetDescription + "</td>");
                        sw.Write("<td>" + item.CostCenter + "</td>");
                        sw.Write("<td>" + item.Room + "</td>");
                        sw.Write("<td>" + item.Qty + "</td>");
                        sw.Write("<td>" + item.RecCostCenter + "</td>");

                        sw.Write("<td>" + item.CostCenter2 + "</td>");
                        sw.Write("<td>" + item.Room2 + "</td>");

                        sw.Write("<td>" + item.Remark + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.No + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "' style='color:" + statusColor + "'>" + item.DocStatus + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.DocNo + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.ItemNo + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.AssetClass + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.Asset + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.AssetDescription + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.CostCenter + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.Room + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.Qty + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.RecCostCenter + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.CostCenter2 + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.Room2 + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + item.Remark + "</td>");

                        string IssuerAppDate = "", TopIssuerAppDate = "", RecieverAppDate = "", TopRecieverAppDate = "", AFAppDate = "";
                        string IssuerAppTime = "", TopIssuerAppTime = "", RecieverAppTime = "", TopRecieverAppTime = "", AFAppTime = "";
                        string IssuerName = "", TopIssuerName = "", RecieverName = "", TopRecieverName = "", AFName = "";
                        if (item.IssuerAppDate != null) {
                            IssuerAppDate = DateTime.ParseExact(item.IssuerAppDate, "yyyymmdd", null).ToString("dd.mm.yyyy");
                            IssuerAppTime = DateTime.ParseExact(item.IssuerAppTime, "HHmmss", null).ToString("HH.mm.ss");
                            IssuerName = item.IssuerName;
                        }
                        if (item.TopIssuerAppDate != null) { 
                            TopIssuerAppDate = DateTime.ParseExact(item.TopIssuerAppDate, "yyyymmdd", null).ToString("dd.mm.yyyy");
                            TopIssuerAppTime = DateTime.ParseExact(item.TopIssuerAppTime, "HHmmss", null).ToString("HH.mm.ss");
                            TopIssuerName = item.TopIssuerName;
                        }
                        if (item.RecieverAppDate != null) { 
                            RecieverAppDate = DateTime.ParseExact(item.RecieverAppDate, "yyyymmdd", null).ToString("dd.mm.yyyy");
                            RecieverAppTime = DateTime.ParseExact(item.RecieverAppTime, "HHmmss", null).ToString("HH.mm.ss");
                            RecieverName = item.RecieverName;
                        }
                        if (item.TopRecieverAppDate != null) { 
                            TopRecieverAppDate = DateTime.ParseExact(item.TopRecieverAppDate, "yyyymmdd", null).ToString("dd.mm.yyyy");
                            TopRecieverAppTime = DateTime.ParseExact(item.TopRecieverAppTime, "HHmmss", null).ToString("HH.mm.ss");
                            TopRecieverName = item.TopRecieverName;
                        }
                        if (item.AFAppDate != null)
                        {
                            AFAppDate = DateTime.ParseExact(item.AFAppDate, "yyyymmdd", null).ToString("dd.mm.yyyy");
                            AFAppTime = DateTime.ParseExact(item.AFAppTime, "HHmmss", null).ToString("HH.mm.ss");
                            AFName = item.AFName;
                        }

                        sw.Write("<td>" + IssuerAppDate + "</td>");
                        sw.Write("<td>" + IssuerAppTime + "</td>");
                        sw.Write("<td>" + IssuerName + "</td>");

                        sw.Write("<td>" + TopIssuerAppDate + "</td>");
                        sw.Write("<td>" + TopIssuerAppTime + "</td>");
                        sw.Write("<td>" + TopIssuerName + "</td>");

                        sw.Write("<td>" + RecieverAppDate + "</td>");
                        sw.Write("<td>" + RecieverAppTime + "</td>");
                        sw.Write("<td>" + RecieverName + "</td>");

                        sw.Write("<td>" + TopRecieverAppDate + "</td>");
                        sw.Write("<td>" + TopRecieverAppTime + "</td>");
                        sw.Write("<td>" + TopRecieverName + "</td>");

                        sw.Write("<td>" + AFAppDate + "</td>");
                        sw.Write("<td>" + AFAppTime + "</td>");
                        sw.Write("<td>" + AFName + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + IssuerAppDate + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + IssuerAppTime + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + IssuerName + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + TopIssuerAppDate + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + TopIssuerAppTime + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + TopIssuerName + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + RecieverAppDate + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + RecieverAppTime + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + RecieverName + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + TopRecieverAppDate + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + TopRecieverAppTime + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + TopRecieverName + "</td>");

                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + AFAppDate + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + AFAppTime + "</td>");
                        //sw.Write("<td bgcolor='" + bgcolor + "'>" + AFName + "</td>");
                        sw.Write("</tr>");
                    }

                    sw.Write("</table>");

                }
               
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

            }


            return View(queryATReport);
        }

        public ActionResult ReportHistory(string DocNo = "")
        {

            List<vAssetTransfer_ReportHistory> queryATReport = db.vAssetTransfer_ReportHistory
                .Where(e => e.Client == Properties.Settings.Default.Client && e.DocNo == DocNo)
                .OrderBy(e => e.DocNo).OrderBy(e => e.CreateDate).OrderBy(e => e.CreateTime).OrderBy(e => e.Asset).Distinct()
                .ToList();

            return View(queryATReport);
        }

    }
}
