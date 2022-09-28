using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using AT_System.Models;
using System.Xml;
using System.Transactions;
using System.Net.Mail;
using System.Text;
using System.DirectoryServices;
using System.IO;

namespace AT_System.Controllers
{
    public class ATController : Controller
    {
        private SAPEntities db = new SAPEntities();
        private _ATForm AT = new _ATForm();
        
        public ActionResult Index(string searchString="",string keydateb = "", string keydatee = "")
        {
            //Check Login Session
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            DateTime dateb;
            DateTime datee;
            DateTime today = DateTime.Today;

            string strUserAD = Session["UserAD"].ToString();
            string strEmail = Session["Email"].ToString();
            string strPermission = Session["chkPermission"].ToString();
            
            int dd, mm, yy;

            if (keydateb == "" && keydatee == "")
            {
                dateb = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                datee = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }
            else if (keydateb == "")
            {
                dd = Int32.Parse(keydatee.Substring(0, 2));
                mm = Int32.Parse(keydatee.Substring(3, 2));
                yy = Int32.Parse(keydatee.Substring(6, 4));
                datee = new DateTime(yy, mm, dd);
                dateb = datee;
            }
            else if (keydatee == "")
            {
                dd = Int32.Parse(keydateb.Substring(0, 2));
                mm = Int32.Parse(keydateb.Substring(3, 2));
                yy = Int32.Parse(keydateb.Substring(6, 4));
                dateb = new DateTime(yy, mm, dd);
                datee = dateb;
            }
            else
            {
                dd = Int32.Parse(keydateb.Substring(0, 2));
                mm = Int32.Parse(keydateb.Substring(3, 2));
                yy = Int32.Parse(keydateb.Substring(6, 4));
                dateb = new DateTime(yy, mm, dd);

                dd = Int32.Parse(keydatee.Substring(0, 2));
                mm = Int32.Parse(keydatee.Substring(3, 2));
                yy = Int32.Parse(keydatee.Substring(6, 4));
                datee = new DateTime(yy, mm, dd);
            }

            ViewBag.dateb = dateb.ToString("dd/MM/yyyy");
            ViewBag.datee = datee.ToString("dd/MM/yyyy");


            dateb = dateb.AddDays(-1);
            keydateb = dateb.ToString("yyyyMMdd");
            datee = datee.AddDays(1);
            keydatee = datee.ToString("yyyyMMdd");

            var vUserAD = User.Identity.Name.Split('_');

            List<vAssetTransfer_Doc> vAsset = db.vAssetTransfer_Doc.Where(v => ((String.Compare(v.CreateDate, keydateb) > 0) && (String.Compare(v.CreateDate, keydatee) < 0))
                       && v.Client == Properties.Settings.Default.Client).ToList();
            
            
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                vAsset = vAsset.Where(s => (s.DocNo.ToLower().Contains(searchString)) || (s.CostCenter.ToLower().Contains(searchString)) || (s.CostCenter2.ToLower().Contains(searchString)) || (s.Asset.ToLower().Contains(searchString)) || (s.AssetClass.ToLower().Contains(searchString))).ToList();
            }

            if (strPermission == "Operator")
            {
                //Set filter list by user ad
                vAsset = vAsset.Where(s => s.FlowStatus.Contains(vUserAD[0].ToString())).ToList();
            }

            vAsset = vAsset.OrderBy(s => s.CreateDate).OrderByDescending(s => s.DocNo).ToList();
            ViewBag.ResultRows = vAsset.Count();

            AT.vAT_Doc = vAsset;
            return View(AT);
        }

        public ActionResult Create(string strSendDep = "")
        {

            //Check Login Session
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            DateTime dateIssue;
            DateTime dateTopSec;
            DateTime dateReceiver;
            DateTime dateTopRec;
            DateTime dateAF;
            DateTime vDocDate;
           
            DateTime today = DateTime.Today;

            string strDepartment = Session["DepartmentCode"].ToString();
            string strDivisionCode = Session["DivisionCode"].ToString();
            string strSectionCode = Session["SectionCode"].ToString();
            string strTeamCode = Session["TeamCode"].ToString();
            string strOppFieldCode = "";
            string strOppDepartmentCode = "";
            //string vRecCostCenter = "";

            dateIssue = today;
            dateTopSec = today;
            dateReceiver = today;
            dateTopRec = today;
            dateAF = today;
            vDocDate = today;
            
            //Set Viewbag Value to Html
            ViewBag.dateIssue = dateIssue.ToString("dd/MM/yyyy");
            ViewBag.dateTopSec = dateTopSec.ToString("dd/MM/yyyy");
            ViewBag.dateReceiver = dateReceiver.ToString("dd/MM/yyyy");
            ViewBag.dateTopRec = dateTopRec.ToString("dd/MM/yyyy");
            ViewBag.dateAF = dateAF.ToString("dd/MM/yyyy");
            ViewBag.DocNo = "";
            ViewBag.UserAD = Session["UserAD"].ToString();

            var tbSendDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && (r.Status == 0 || r.Status == 9)).ToList();

            //Logic filter dropdown issue costcenter
            if (strDepartment != "" && strDepartment == "GM") 
            {
                tbSendDepartment = tbSendDepartment.Where(r => r.FieldCode == "DepartmentCode" && r.DepartmentCode == "GM" && r.ReceiverCC.Contains(strDivisionCode)).ToList();
                strOppFieldCode = "DepartmentCode";
                strOppDepartmentCode = "GM";
            }else if (strTeamCode != "")
            {
                tbSendDepartment = tbSendDepartment.Where(r => r.FieldCode == "TeamCode" && r.DepartmentCode == strTeamCode).ToList();
                strOppFieldCode = "TeamCode";
                strOppDepartmentCode = strTeamCode;
            }
            else {
                tbSendDepartment = tbSendDepartment.Where(r => r.FieldCode == "SectionCode" && r.DepartmentCode == strSectionCode).ToList();
                strOppFieldCode = "SectionCode";
                strOppDepartmentCode = strSectionCode;


                if (tbSendDepartment == null || tbSendDepartment.Count == 0)
                {
                    tbSendDepartment = (from RecCost in db.FI03_AT_RecCostCenter.Where(e => e.Client == Properties.Settings.Default.Client)
                                        join RecCosts in db.FI03_AT_RecCostCenter_Special on new { RecCost.FieldCode, RecCost.DepartmentCode } equals new { RecCosts.FieldCode, RecCosts.DepartmentCode }
                                        where RecCost.Client == RecCosts.Client && RecCost.Client == Properties.Settings.Default.Client && RecCosts.SectionCode == strSectionCode
                                        select RecCost).OrderBy(e => e.DepartmentCode).ToList();
                }

                if (tbSendDepartment == null || tbSendDepartment.Count == 0)
                {
                    tbSendDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && r.DepartmentCode == strDepartment && (r.Status == 0 || r.Status == 9)).ToList();
                    strOppFieldCode = "DepartmentCode";
                    strOppDepartmentCode = strDepartment;
                }
            }
           
            IEnumerable<SelectListItem> listSendDepartment = (from s in tbSendDepartment
                                                             select new SelectListItem
                                                             {
                                                                 Value = s.AutoRun.ToString(),
                                                                 Text = s.ReceiverCC
                                                             }).OrderBy(s => s.Text).ToList();

            if (strSendDep != "") { listSendDepartment = listSendDepartment.Where(d => d.Value == strSendDep); }

            ViewBag.tbSendDepartment = listSendDepartment.ToList();

            var tbRecDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && r.Status == 0 ).ToList();
            //&& (r.FieldCode != strOppFieldCode && r.DepartmentCode != strOppDepartmentCode)

            IEnumerable<SelectListItem> listRecDepartment = (from s in tbRecDepartment
                                                             select new SelectListItem
                                                             {
                                                                 Value = s.AutoRun.ToString(),
                                                                 Text = s.ReceiverCC
                                                             }).OrderBy(s => s.Text).ToList();
            ViewBag.tbRecDepartment = listRecDepartment.ToList();

            List<FI03_AT_FlowMaster> listAF = (from vFlow in db.FI03_AT_FlowMaster
                                               join vRec in db.FI03_AT_RecCostCenter on vFlow.FI03_AT_RecCostCenter equals vRec.AutoRun
                                               where vFlow.Client == Properties.Settings.Default.Client && vRec.FieldCode == "AF_Update" && vRec.DepartmentCode == "A&F" && vRec.Status == 9
                                               select vFlow).ToList();
            ViewBag.tbAF = listAF;

            return View();
        }

        [HttpPost]
        public ActionResult Create(_ATForm at, Guid guid)
        {
            //Check Login Session
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            string userLogin = System.Web.HttpContext.Current.Session["UserAD"].ToString();
            try
            { 
                string gId = "";
                using (TransactionScope trans = new TransactionScope())
                {
                    DateTime vDocDate = DateTime.Today;
                    string vNextflow_Name = "";

                    //AT Item Transaction
                    int cntItem = 1;
                    foreach (FI03_AT_DocItem item in at.AT_Item)
                    {
                        item.Client = Properties.Settings.Default.Client;
                        item.AutoRun = Guid.NewGuid();
                        item.FI03_AT_Doc = guid;
                        item.ItemNo = cntItem;

                        db.FI03_AT_DocItem.Add(item);
                        db.SaveChanges();
                        cntItem++;
                    }

                    int cntFlow = 1;
                    //Boolean chkUpdateFlow = false;
                    // Assign Send Cost Center 
                    foreach (FI03_AT_FlowApprove flow in at.AT_Flow) {
                        flow.Client = Properties.Settings.Default.Client;
                        flow.AutoRun = Guid.NewGuid();
                        flow.FI03_AT_Doc = guid;
                        flow.FI03_AT_FlowMaster = flow.FI03_AT_FlowMaster;
                        flow.EmpID = flow.EmpID;
                        flow.EmpFirstName = flow.EmpFirstName.ToLower();
                        flow.EmpLastName = flow.EmpLastName.ToLower();
                        flow.EmpEmail = flow.EmpEmail;
                        flow.UserCreate = userLogin;
                        flow.DateCreate = DateTime.Now.ToString("yyyyMMdd");
                        flow.FlowState = cntFlow.ToString();

                        if (at.AT_Header.FI03_AT_RecCostCenter_From == flow.FI03_AT_RecCostCenter) {
                            if (flow.FlowState == "1") { 
                                flow.ApproveStatus = "Wait Approve..";
                                vNextflow_Name = flow.EmpFirstName.ToString().ToLower();
                            }
                        }

                        db.FI03_AT_FlowApprove.Add(flow);
                        db.SaveChanges();
                        cntFlow++;
                    }

                    //Generate Doc No
                    int length = 4; //fixed prefix zero on at item
                    string strDocDate = vDocDate.ToString("yyyyMM");
                    strDocDate = "TSC-" + strDocDate;

                    //Count Doc No
                    var countDocNo = db.FI03_AT_Doc.Where(v => v.Client == Properties.Settings.Default.Client && v.DocNo.Contains(strDocDate)).Count();
                    countDocNo += 1;
                    at.AT_Header.DocNo = strDocDate + "-" + countDocNo.ToString("D" + length);
                    //End Count Doc No

                    //AT Header Transection
                    at.AT_Header.Client = Properties.Settings.Default.Client;
                    at.AT_Header.AutoRun = guid;
                    at.AT_Header.CreateBy = userLogin;
                    at.AT_Header.CreateDate = DateTime.Now.ToString("yyyyMMdd");
                    at.AT_Header.CreateTime = DateTime.Now.ToString("HHmmss");
                    //at.AT_Header.IssueDate = DateTime.Now.ToString("dd/MM/yyyy");
                    //at.AT_Header.IssueTime = DateTime.Now.ToString("HH:mm:ss");
                    //at.AT_Header.IssueDateTop = DateTime.Now.ToString("dd/MM/yyyy");
                    //at.AT_Header.IssueTimeTop = DateTime.Now.ToString("HH:mm:ss");
                    //at.AT_Header.ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy");
                    //at.AT_Header.ReceiveTime = DateTime.Now.ToString("HH:mm:ss");
                    //at.AT_Header.ReceiveDateTop = DateTime.Now.ToString("dd/MM/yyyy");
                    //at.AT_Header.ReceiveTimeTop = DateTime.Now.ToString("HH:mm:ss");
                    //at.AT_Header.AFAppDate = DateTime.Now.ToString("dd/MM/yyyy");
                    //at.AT_Header.AFAppTime = DateTime.Now.ToString("HH:mm:ss");
                    at.AT_Header.DocStatus = "Issue";
                    at.AT_Header.FlowStatus = vNextflow_Name;

                    db.FI03_AT_Doc.Add(at.AT_Header);
                    db.SaveChanges();

                    trans.Complete();                  
                }

                gId = at.AT_Header.AutoRun.ToString();

                SendMail(at.AT_Header.AutoRun);

                TempData["MessageStatus"] = "X";
                TempData["Message"] = "Process Success.";
                return RedirectToAction("Preview", "AT", new { Id = gId });
            }
            catch
            {
                TempData["MessageStatus"] = "";
                TempData["Message"] = "ERROR-Create Failed.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Preview(string Id)
        {
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            Guid gID = new Guid();
            gID = new Guid(Id);

            FI03_AT_Doc at_header = db.FI03_AT_Doc.Where(e => e.Client == Properties.Settings.Default.Client && e.AutoRun == gID).SingleOrDefault();
            AT.AT_Header = at_header;

            List<FI03_AT_DocItem> at_item = db.FI03_AT_DocItem.Where(e => e.Client == Properties.Settings.Default.Client && e.FI03_AT_Doc == gID).OrderBy(e => e.ItemNo).ToList();
            AT.AT_Item = at_item;


            var tbSendDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && (r.Status == 0 || r.Status == 9)).ToList();
            IEnumerable<SelectListItem> listSendDepartment = (from s in tbSendDepartment
                                                              select new SelectListItem
                                                              {
                                                                  Value = s.AutoRun.ToString(),
                                                                  Text = s.ReceiverCC
                                                              }).OrderBy(s => s.Text).ToList();
            ViewBag.tbSendDepartment = listSendDepartment.ToList();

            var tbRecDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && r.Status == 0).ToList();
            IEnumerable<SelectListItem> listRecDepartment = (from s in tbRecDepartment
                                                             select new SelectListItem
                                                             {
                                                                 Value = s.AutoRun.ToString(),
                                                                 Text = s.ReceiverCC
                                                             }).OrderBy(s => s.Text).ToList();
            ViewBag.tbRecDepartment = listRecDepartment.ToList();

            //------- List of Attached files ------
            string pathfile = Server.MapPath("~/ATFiles/" + Id);
            Stack<string> x;
            List<string> listFilename = new List<string>();
            if (System.IO.Directory.Exists(pathfile))
            {
                var files = System.IO.Directory.GetFiles(pathfile);
                foreach (var file in files)
                {
                    x = new Stack<string>(file.ToString().Split('\\'));
                    listFilename.Add(x.Pop().ToString());
                }
            }
            ViewBag.listFilename = listFilename;
            //----------------------------------------

            //Set Value Check Authorize UserAD == User Current Flow
            var vUserAD = User.Identity.Name.Split('_');
            ViewBag.vUserAD = vUserAD[0].ToString();
            ViewBag.vNameCurrentflow = db.FI03_AT_FlowApprove.Where(e => e.Client == Properties.Settings.Default.Client && e.FI03_AT_Doc == gID && (e.ApproveStatus == null || e.ApproveStatus == "Wait Approve..")).OrderBy(e => e.FlowState).FirstOrDefault();

            List<FI03_AT_FlowApprove> at_flow = db.FI03_AT_FlowApprove.Where(e => e.Client == Properties.Settings.Default.Client && e.FI03_AT_Doc == gID).OrderBy(e => e.FlowState).ToList();
            AT.AT_Flow = at_flow;

            return View(AT);
        }

        [HttpPost]
        public ActionResult Preview(_ATForm at, string txtNote = "", Boolean chkApprove = false)
        {
            //Check Login Session
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            string userLogin = System.Web.HttpContext.Current.Session["UserAD"].ToString();
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    FI03_AT_Doc at_header = db.FI03_AT_Doc.Where(e => e.AutoRun == at.AT_Header.AutoRun).SingleOrDefault();
                    Guid strID = at_header.AutoRun;
                    string strDocNo = at_header.DocNo;
                    string strCostCenter = at.AT_Header.FI03_AT_RecCostCenter_From;
                    string strRecCostCenter = at.AT_Header.FI03_AT_RecCostCenter_To;
                    string strCreateDate = at_header.CreateDate;
                    string strCreateTime = at_header.CreateTime;
                    string strCreateBy = at_header.CreateBy;

                    db.FI03_AT_Doc.Remove(at_header);
                    db.SaveChanges();

                    List<FI03_AT_DocItem> at_item = db.FI03_AT_DocItem.Where(e => e.FI03_AT_Doc == at.AT_Header.AutoRun).ToList();
                    foreach (FI03_AT_DocItem item in at_item)
                    {
                        db.FI03_AT_DocItem.Remove(item);
                        db.SaveChanges();
                    }

                    List<FI03_AT_FlowApprove> at_flow = db.FI03_AT_FlowApprove.Where(e => e.FI03_AT_Doc == at.AT_Header.AutoRun).ToList();
                    foreach (FI03_AT_FlowApprove itemFlow in at_flow)
                    {
                        db.FI03_AT_FlowApprove.Remove(itemFlow);
                        db.SaveChanges();
                    }

                   

                    //AT Item Transection
                    int cntItem = 1;
                    foreach (FI03_AT_DocItem item in at.AT_Item)
                    {
                        item.Client = Properties.Settings.Default.Client;

                        db.FI03_AT_DocItem.Add(item);
                        db.SaveChanges();
                        cntItem++;
                    }

                    //AT Flow Transection
                    Boolean chkUpdateFlow = false;
                    string chkSignature = "";
                    string chkNameSignature = "";
                    string vNextflow_Name = "";
                    //Set Current Status Flow Approve
                    string currentFlow = at.AT_Flow.Where(e => (e.ApproveStatus == null || e.ApproveStatus == "Wait Approve..")).OrderBy(e => e.FlowState).Select(e => new { e.FlowState }).FirstOrDefault().FlowState.ToString();

                    foreach (FI03_AT_FlowApprove flow in at.AT_Flow)
                    {
                        flow.Client = Properties.Settings.Default.Client;
                        flow.FI03_AT_Doc = strID;
                        flow.UserCreate = strCreateBy;
                        flow.DateCreate = strCreateDate;
                        flow.DateApprove = at_flow.Where(e => e.FlowState == flow.FlowState).Select(e => e.DateApprove).SingleOrDefault();
                        flow.TimeApprove = at_flow.Where(e => e.FlowState == flow.FlowState).Select(e => e.TimeApprove).SingleOrDefault();

                        if ((flow.ApproveStatus == null || flow.ApproveStatus == "Wait Approve..") && chkUpdateFlow == false)
                        {
                            chkUpdateFlow = true;
                            if (chkApprove == true) { flow.ApproveStatus = "Approve"; } else { flow.ApproveStatus = "Reject"; flow.Remark = txtNote; }
                            flow.DateApprove = DateTime.Now.ToString("yyyyMMdd");
                            flow.TimeApprove = DateTime.Now.ToString("HHmmss");
                            
                            switch(flow.FlowState){
                                case "1":
                                    chkSignature = "Issue";
                                    chkNameSignature = flow.EmpFirstName.ToLower() + " " + flow.EmpLastName.ToLower();
                                    break;
                                case "2":
                                    chkSignature = "Top Issue";
                                    chkNameSignature = flow.EmpFirstName.ToLower() + " " + flow.EmpLastName.ToLower();
                                    break;
                                case "3":
                                    chkSignature = "Receive";
                                    chkNameSignature = flow.EmpFirstName.ToLower() + " " + flow.EmpLastName.ToLower();
                                    break;
                                case "4":
                                    chkSignature = "Top Receive";
                                    chkNameSignature = flow.EmpFirstName.ToLower() + " " + flow.EmpLastName.ToLower();
                                    break;
                                default:
                                    chkSignature = "A&F";
                                    chkNameSignature = flow.EmpFirstName.ToLower() + " " + flow.EmpLastName.ToLower();
                                    break;
                            }                
                        }

                        // Set work process user
                        if (currentFlow != null && (Convert.ToInt32(currentFlow)+1) <= 5)
                        {
                            if ((Convert.ToInt32(currentFlow) + 1) == Convert.ToInt32(flow.FlowState))
                            {
                                vNextflow_Name = flow.EmpFirstName.ToLower();
                            }
                        }
                        else { vNextflow_Name = "Approve"; }

                        if (flow.EmpEmail != null)
                        {
                            db.FI03_AT_FlowApprove.Add(flow);
                            db.SaveChanges();
                        }
                    }

                    
                    //AT Header Transection
                    at.AT_Header.Client = Properties.Settings.Default.Client;
                    at.AT_Header.AutoRun = strID;
                    at.AT_Header.CreateBy = strCreateBy;
                    at.AT_Header.CreateDate = strCreateDate;
                    at.AT_Header.CreateTime = strCreateTime;
                    at.AT_Header.UpdateBy = userLogin;
                    at.AT_Header.UpdateDate = DateTime.Now.ToString("yyyyMMdd");
                    at.AT_Header.UpdateTime = DateTime.Now.ToString("HHmmss");
                    

                    if (chkApprove)
                    {
                        if (Int32.Parse(currentFlow) == at.AT_Flow.Count())
                        {
                            at.AT_Header.DocStatus = "Approve"; // IF Approve and last flow, change doc status to Release
                        }
                        else
                        {
                            at.AT_Header.DocStatus = "Issue"; // IF Approve but not last flow, doc status still Issue
                        }
                        at.AT_Header.FlowStatus = vNextflow_Name;
                    }
                    else
                    {
                        //
                        at.AT_Header.DocStatus = "Reject"; //If Reject, Return to issue.

                        var vRejflow_Name = strCreateBy.Split('_');
                        at.AT_Header.FlowStatus = vRejflow_Name[0].ToString().ToLower();
                    }

                    // Stamp User Approve Flow
                    switch (chkSignature){
                        case "Issue":
                            at.AT_Header.IssueBy = chkNameSignature;
                            at.AT_Header.IssueDate = DateTime.Now.ToString("dd/MM/yyyy");
                            at.AT_Header.IssueTime = DateTime.Now.ToString("HH:mm:ss");
                        break;
                        case "Top Issue":
                            at.AT_Header.IssueByTop = chkNameSignature;
                            at.AT_Header.IssueDateTop = DateTime.Now.ToString("dd/MM/yyyy");
                            at.AT_Header.IssueTimeTop = DateTime.Now.ToString("HH:mm:ss");
                        break;
                        case "Receive":
                            at.AT_Header.ReceiveBy = chkNameSignature;
                            at.AT_Header.ReceiveDate = DateTime.Now.ToString("dd/MM/yyyy");
                            at.AT_Header.ReceiveTime = DateTime.Now.ToString("HH:mm:ss");
                        break;
                        case "Top Receive":
                            at.AT_Header.ReceiveByTop = chkNameSignature;
                            at.AT_Header.ReceiveDateTop = DateTime.Now.ToString("dd/MM/yyyy");
                            at.AT_Header.ReceiveTimeTop = DateTime.Now.ToString("HH:mm:ss");
                        break;
                        default:
                            at.AT_Header.AFApp = chkNameSignature;
                            at.AT_Header.AFAppDate = DateTime.Now.ToString("dd/MM/yyyy");
                            at.AT_Header.AFAppTime = DateTime.Now.ToString("HH:mm:ss");
                        break;
                    }

                    db.FI03_AT_Doc.Add(at.AT_Header);
                    db.SaveChanges();

                    

                    trans.Complete();        
                }
                string gId = Convert.ToString(at.AT_Header.AutoRun);

                if (chkApprove == true) { SendMail(at.AT_Header.AutoRun); } else { SendMailReject(at.AT_Header.AutoRun, txtNote); }
                TempData["MessageStatus"] = "X";
                TempData["Message"] = "Process Success.";

                if (at.AT_Header.DocStatus == "Reject") { return RedirectToAction("Reset", "AT", new { Id = gId }); } else { return RedirectToAction("Preview", "AT", new { Id = gId }); }
                //return RedirectToAction("Preview", "AT", new { Id = gId });
            }
            catch (Exception e)
            {
                TempData["MessageStatus"] = "";
                TempData["Message"] = e;
                return RedirectToAction("Index");
            }
            
        }

        public ActionResult Reset(string Id)
        {
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            Guid gID = new Guid();
            gID = new Guid(Id);

            FI03_AT_Doc at_header = db.FI03_AT_Doc.Where(e => e.Client == Properties.Settings.Default.Client && e.AutoRun == gID).SingleOrDefault();
            AT.AT_Header = at_header;

            List<FI03_AT_DocItem> at_item = db.FI03_AT_DocItem.Where(e => e.Client == Properties.Settings.Default.Client && e.FI03_AT_Doc == gID).OrderBy(e => e.ItemNo).ToList();
            AT.AT_Item = at_item;

            //// Get Count Item /////
            var vAssetClass = (from dbAssetClassItem in db.FI03_AT_DocItem
                               where dbAssetClassItem.Client == Properties.Settings.Default.Client && dbAssetClassItem.FI03_AT_Doc == gID && dbAssetClassItem.ItemNo == 1
                               select new { dbAssetClassItem.AssetClass }).FirstOrDefault().AssetClass.ToString();
            ViewBag.vAssetClass = vAssetClass;

            var cntitem = (from dbflow in db.FI03_AT_DocItem
                           where dbflow.Client == Properties.Settings.Default.Client && dbflow.FI03_AT_Doc == gID 
                           select dbflow).Count();
            ViewBag.vCountItem = cntitem;

            var tbSendDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && (r.Status == 0 || r.Status == 9)).ToList();
            IEnumerable<SelectListItem> listSendDepartment = (from s in tbSendDepartment
                                                              select new SelectListItem
                                                              {
                                                                  Value = s.AutoRun.ToString(),
                                                                  Text = s.ReceiverCC
                                                              }).OrderBy(s => s.Text).ToList();
            ViewBag.tbSendDepartment = listSendDepartment.ToList();

            var tbRecDepartment = db.FI03_AT_RecCostCenter.Where(r => r.Client == Properties.Settings.Default.Client && r.Status == 0).ToList();
            IEnumerable<SelectListItem> listRecDepartment = (from s in tbRecDepartment
                                                             select new SelectListItem
                                                             {
                                                                 Value = s.AutoRun.ToString(),
                                                                 Text = s.ReceiverCC
                                                             }).OrderBy(s => s.Text).ToList();
            ViewBag.tbRecDepartment = listRecDepartment.ToList();

            //------- List of Attached files ------
            string pathfile = Server.MapPath("~/ATFiles/" + Id);
            Stack<string> x;
            List<string> listFilename = new List<string>();
            if (System.IO.Directory.Exists(pathfile))
            {
                var files = System.IO.Directory.GetFiles(pathfile);
                foreach (var file in files)
                {
                    x = new Stack<string>(file.ToString().Split('\\'));
                    listFilename.Add(x.Pop().ToString());
                }
            }
            ViewBag.listFilename = listFilename;
            //----------------------------------------

            //Set Value Check Authorize UserAD == User Current Flow
            ViewBag.vUserAD = User.Identity.Name.ToString();
            //ViewBag.vNameCurrentflow = db.FI03_AT_FlowApprove.Where(e => e.Client == Properties.Settings.Default.Client && e.FI03_AT_Doc == gID && e.ApproveStatus == null).OrderBy(e => e.FlowState).FirstOrDefault();

            List<FI03_AT_FlowApprove> at_flow = db.FI03_AT_FlowApprove.Where(e => e.Client == Properties.Settings.Default.Client && e.FI03_AT_Doc == gID).OrderBy(e => e.FlowState).ToList();
            AT.AT_Flow = at_flow;

            return View(AT);
        }

        [HttpPost]
        public ActionResult Reset(_ATForm at)
        {

            //Check Login Session
            if (System.Web.HttpContext.Current.Session["UserAD"] == null || System.Web.HttpContext.Current.Session["UserAD"].ToString() == "")
            {
                return RedirectToAction("Login", "Account", new { returnURL = Request.Url.ToString() });
            }

            string userLogin = System.Web.HttpContext.Current.Session["UserAD"].ToString();

            try {
                using (TransactionScope trans = new TransactionScope())
                {
                    FI03_AT_Doc at_header = db.FI03_AT_Doc.Where(e => e.AutoRun == at.AT_Header.AutoRun).SingleOrDefault();
                    Guid strID = at_header.AutoRun;
                    string strDocNo = at_header.DocNo;
                    string strCostCenter = at.AT_Header.FI03_AT_RecCostCenter_From;
                    string strRecCostCenter = at.AT_Header.FI03_AT_RecCostCenter_To;
                    string strCreateDate = at_header.CreateDate;
                    string strCreateTime = at_header.CreateTime;
                    string strCreateBy = at_header.CreateBy;

                    db.FI03_AT_Doc.Remove(at_header);
                    db.SaveChanges();

                    List<FI03_AT_DocItem> at_item = db.FI03_AT_DocItem.Where(e => e.FI03_AT_Doc == at.AT_Header.AutoRun).OrderBy(e => e.ItemNo).ToList();
                    foreach (FI03_AT_DocItem item in at_item)
                    {
                        db.FI03_AT_DocItem.Remove(item);
                        db.SaveChanges();
                    }

                    List<FI03_AT_FlowApprove> at_flow = db.FI03_AT_FlowApprove.Where(e => e.FI03_AT_Doc == at.AT_Header.AutoRun).OrderBy(e => e.FlowState).ToList();
                    foreach (FI03_AT_FlowApprove itemFlow in at_flow)
                    {
                        db.FI03_AT_FlowApprove.Remove(itemFlow);
                        db.SaveChanges();
                    }

                    
                    //AT Item Transaction
                    int cntItem = 1;
                    foreach (FI03_AT_DocItem item in at.AT_Item)
                    {
                        item.Client = Properties.Settings.Default.Client;
                        item.AutoRun = Guid.NewGuid();
                        item.FI03_AT_Doc = strID;
                        item.ItemNo = cntItem;
                        db.FI03_AT_DocItem.Add(item);
                        db.SaveChanges();
                        cntItem++;
                    }

                    int cntFlow = 1;
                    string vNextflow_Name = "";
                    //AT Flow Transaction
                    foreach (FI03_AT_FlowApprove flow in at.AT_Flow)
                    {
                        flow.Client = Properties.Settings.Default.Client;
                        flow.AutoRun = at_flow[cntFlow - 1].AutoRun;
                        flow.FI03_AT_Doc = at_flow[cntFlow - 1].FI03_AT_Doc;
                        flow.FI03_AT_FlowMaster = flow.FI03_AT_FlowMaster;
                        flow.FI03_AT_RecCostCenter = flow.FI03_AT_RecCostCenter;
                        flow.EmpID = flow.EmpID;
                        flow.EmpFirstName = flow.EmpFirstName.ToString().ToLower();
                        flow.EmpLastName = flow.EmpLastName.ToString().ToLower();
                        flow.EmpEmail = flow.EmpEmail.ToString().ToLower();

                        //flow.AutoRun = at_flow[cntFlow - 1].AutoRun;
                        //flow.FI03_AT_Doc = at_flow[cntFlow - 1].FI03_AT_Doc;
                        //flow.FI03_AT_FlowMaster = at_flow[cntFlow - 1].FI03_AT_FlowMaster;
                        //flow.FI03_AT_RecCostCenter = at_flow[cntFlow - 1].FI03_AT_RecCostCenter;
                        //flow.EmpID = at_flow[cntFlow - 1].EmpID;
                        //flow.EmpFirstName = at_flow[cntFlow - 1].EmpFirstName;
                        //flow.EmpLastName = at_flow[cntFlow - 1].EmpLastName;
                        //flow.EmpEmail = at_flow[cntFlow - 1].EmpEmail;


                        flow.UserCreate = userLogin;
                        flow.DateCreate = DateTime.Now.ToString("yyyyMMdd");
                        flow.FlowState = cntFlow.ToString();

                        if (at.AT_Header.FI03_AT_RecCostCenter_From == flow.FI03_AT_RecCostCenter)
                        {
                            if (flow.FlowState == "1") 
                            { 
                                flow.ApproveStatus = "Wait Approve..";
                                vNextflow_Name = flow.EmpFirstName.ToString().ToLower();
                            }
                        }
                        flow.ApproveStatus = null;
                        flow.FlowStatus = null;
                        flow.DateApprove = null;
                        flow.TimeApprove = null;
                        flow.Remark = null;
                        flow.Flag = null;

                        db.FI03_AT_FlowApprove.Add(flow);
                        db.SaveChanges();
                        cntFlow++;
                    }

                    //Set Defalut Value on AT Header 
                    at.AT_Header.Client = Properties.Settings.Default.Client;
                    at.AT_Header.AutoRun = at_header.AutoRun;
                    at.AT_Header.DocNo = at_header.DocNo;
                    at.AT_Header.CreateBy = userLogin;
                    at.AT_Header.CreateDate = DateTime.Now.ToString("yyyyMMdd");
                    at.AT_Header.CreateTime = DateTime.Now.ToString("HHmmss");
                    at.AT_Header.FI03_AT_RecCostCenter_From = at_header.FI03_AT_RecCostCenter_From;
                    at.AT_Header.FI03_AT_RecCostCenter_To = at_header.FI03_AT_RecCostCenter_To;
                    at.AT_Header.IssueBy = null;
                    at.AT_Header.IssueDate = null;
                    at.AT_Header.IssueTime = null;
                    at.AT_Header.IssueByTop = null;
                    at.AT_Header.IssueDateTop = null;
                    at.AT_Header.IssueTimeTop = null;
                    at.AT_Header.ReceiveBy = null;
                    at.AT_Header.ReceiveDate = null;
                    at.AT_Header.ReceiveTime = null;
                    at.AT_Header.ReceiveByTop = null;
                    at.AT_Header.ReceiveDateTop = null;
                    at.AT_Header.ReceiveTimeTop = null;
                    at.AT_Header.AFApp = null;
                    at.AT_Header.AFAppDate = null;
                    at.AT_Header.AFAppTime = null;
                    at.AT_Header.UpdateBy = null;
                    at.AT_Header.UpdateDate = null;
                    at.AT_Header.UpdateTime = null;
                    at.AT_Header.DocStatus = "Issue";
                    at.AT_Header.FlowStatus = vNextflow_Name;

                    db.FI03_AT_Doc.Add(at.AT_Header);
                    db.SaveChanges();


                    trans.Complete();                   
                }

                string gId = Convert.ToString(at.AT_Header.AutoRun);

                SendMail(at.AT_Header.AutoRun);
                TempData["MessageStatus"] = "X";
                TempData["Message"] = "Process Success.";
                return RedirectToAction("Preview", "AT", new { Id = gId }); 
            }
            catch (Exception e)
            {
                TempData["MessageStatus"] = "";
                TempData["Message"] = e;
                return RedirectToAction("Index");
            }
        }


        public ActionResult GetCostcenterSAP(string costcenter, string assetclass) {
            Guid strCostcenter = new Guid(costcenter);
            List<FI03_AT_CostCenter> result = (from item in db.FI03_AT_CostCenter
                                               where item.Client == Properties.Settings.Default.Client && item.FI03_AT_RecCostCenter == costcenter && item.AssetClass == assetclass && item.Status == 0
                                               select item).OrderBy(item => item.AssetClass).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAsset(string costcenter, string asset)
        {
            var vWSTSC = new WSTSC.Service1();
            var result =  vWSTSC.GetAsset(Properties.Settings.Default.Client,"",asset);

            return Json(result.InnerXml, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFlowApprove(string costcenter)
        {
            var result = GetFlowApproveMaster(costcenter);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public List<FI03_AT_FlowMaster> GetFlowApproveMaster(string costcenter)
        {

            List<FI03_AT_FlowMaster> result = (from item in db.FI03_AT_FlowMaster
                                               where item.Client == Properties.Settings.Default.Client && item.Status == 0
                                               select item).OrderBy(item => item.StepNo).ToList();

            //var result = (from vflow in db.FI03_AT_FlowMaster
            //              join vHr in db.HR01_Employee on vflow.EmpID equals vHr.EmpID
            //              where vflow.Client == Properties.Settings.Default.Client
            //              select new { vflow.StepNo, vflow.AutoRun,vflow.FI03_AT_RecCostCenter,vflow.EmpID,vHr.FirstNameEN,vHr.LastNameEN,vflow.Email}).ToList();

            if (costcenter != "")
            {
                Guid strCostcenter = new Guid(costcenter);
                result = result.Where(item => item.FI03_AT_RecCostCenter == strCostcenter).ToList();
            }


            return result;
        }

        public void SendMail(Guid Id)
        {
            /////// ----Config Server Smtp---- //
            SmtpClient SmtpServer = new SmtpClient();
            //SmtpServer.Credentials = new System.Net.NetworkCredential("info@tscpcl.com", "info@1");
            //SmtpServer.Port = 25;            //"192.168.0.197"
            //SmtpServer.Host = "CAS01.thaisteelcable.com";
            SmtpServer.Credentials = new System.Net.NetworkCredential("info@tscpcl.com", "Tscpass9");
            SmtpServer.Port = 25;
            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.EnableSsl = true;
            //------------------------------

            //// Get Data from Database /////
            var cntFlow = (from dbflow in db.FI03_AT_FlowApprove
                               where dbflow.Client == Properties.Settings.Default.Client && dbflow.FI03_AT_Doc == Id
                               && (dbflow.ApproveStatus == null || dbflow.ApproveStatus == "Wait Approve..")
                               select dbflow).Count();

            if (Convert.ToInt32(cntFlow) > 0)
            {
                FI03_AT_FlowApprove flow = db.FI03_AT_FlowApprove.Where(item => item.Client == Properties.Settings.Default.Client && item.FI03_AT_Doc == Id && (item.ApproveStatus == null || item.ApproveStatus == "Wait Approve..")).Select(item => item).OrderBy(item => item.FlowState).FirstOrDefault();

                string MailApprover = flow.EmpEmail;
                //string MailApprover = "itsared.h@thaisteelcable.com"; //เมลล์สำหรับเทสข้อมูล

                var getName = (from flowItem in db.FI03_AT_FlowApprove.Where(e => e.Client == Properties.Settings.Default.Client)
                               join empItem in db.HR01_Employee on new { flowItem.EmpID, flowItem.Client } equals new { empItem.EmpID, empItem.Client }
                               where flowItem.EmpEmail == flow.EmpEmail && flowItem.Client == Properties.Settings.Default.Client
                               select new { empItem.FirstName, empItem.LastName }
                             ).FirstOrDefault();
                string NameApprover = "คุณ " + getName.FirstName + " " + getName.LastName;

                var result = db.FI03_AT_Doc.Where(item => item.Client == Properties.Settings.Default.Client && item.AutoRun == Id).Select(item => item).FirstOrDefault();

                string MailSubject = "For Approve : Asset Transfer - " + DateTime.Now.ToString("dd/MM/yyyy");

                string DocNo = result.DocNo;
                string CreateDate = result.CreateDate;
                string CreateBy = result.CreateBy;

                string LinkMail = db.MM01_PR_Config.Where(e => e.Client == Properties.Settings.Default.Client && e.Movement == "LinkMailAT" && e.Active == "1")
                                    .Select(e => e.Description).SingleOrDefault().ToString();

                //string LinkMail = "http://localhost:54752";

                List<FI03_AT_DocItem> prItem = db.FI03_AT_DocItem.Where(item => item.Client == Properties.Settings.Default.Client && item.FI03_AT_Doc == Id && item.ItemNo == 1).Select(item => item).ToList();

                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.Append("<HEAD>");
                sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=UTF-8'> ");
                sb.Append("</HEAD>");
                sb.Append("<body style='font-family: Tahoma,Cordia New,Times New Roman;font-size:12px'>");

                sb.Append("เรียน " + NameApprover + "<br /><br />");
                sb.Append("แจ้ง โอนย้ายทรัพย์สิน Asset Transfer ขณะนี้ขั้นตอนอนุมัติมาถึงท่านแล้ว กรุณา Click เพื่อเข้าระบบทำการอนุมัติ<br />");
                sb.Append("<br /><br />");

                sb.Append("<table border='1' style='font-family: Tahoma,Cordia New,Times New Roman;font-size:12px;'>");
                sb.Append("<tr align='center' style='background-color:blue'>");
                sb.Append("<td width=50>NO.</td>");
                sb.Append("<td width=130>Document NO.</td>");
                sb.Append("<td width=80>Create Date</td>");
                sb.Append("<td width=60>Create By</td>");
                sb.Append("<td width=120>Link to Web site</td>");
                sb.Append("</tr>");

                int no = 1;
                foreach (var item in prItem)
                {
                    sb.Append("<tr align='center'>");
                    sb.Append("<td valign='top'>" + (no++) + "</td>");
                    sb.Append("<td valign='top'>" + DocNo + "</td>");
                    sb.Append("<td valign='top'>" + DateTime.ParseExact(CreateDate, "yyyyMMdd", null).ToString("dd.MM.yyyy") + "</td>");
                    sb.Append("<td valign='top'>" + CreateBy + "</td>");
                    sb.Append("<td valign='top'><a href='" + LinkMail + "/AT/Preview/" + item.FI03_AT_Doc + "'>Click!</a></td>");
                    //sb.Append("<td valign='top'><a href='http://localhost:54752/AT/Preview/" + item.FI03_AT_Doc + "'>Click!</a></td>");
                    sb.Append("</tr>");

                    break;
                }

                sb.Append("</table>");
                sb.Append("<br /><br /><br />");
                sb.Append("&nbsp;&nbsp;จึงเรียนมาเพื่อทราบ");
                sb.Append("</body>");
                sb.Append("</html>");


                //-------------------------------
                MailMessage mail = new MailMessage();
                mail.IsBodyHtml = true;

                mail.From = new MailAddress("info@tscpcl.com");

                mail.To.Add(MailApprover);
                mail.Subject = MailSubject;
                mail.Body = sb.ToString();
                SmtpServer.Send(mail);

            }
            // Approve Success Flow CC Mail All Flow
            else {
                FI03_AT_FlowApprove flow = db.FI03_AT_FlowApprove.Where(item => item.Client == Properties.Settings.Default.Client && item.FI03_AT_Doc == Id && item.FlowState == "1" && item.ApproveStatus == "Approve").Select(item => item).OrderBy(item => item.FlowState).FirstOrDefault();
                string MailApprover = flow.EmpEmail;

                var getName = (from flowItem in db.FI03_AT_FlowApprove.Where(e => e.Client == Properties.Settings.Default.Client)
                               join empItem in db.HR01_Employee on new { flowItem.EmpID, flowItem.Client } equals new { empItem.EmpID, empItem.Client }
                               where flowItem.EmpEmail == flow.EmpEmail && flowItem.Client == Properties.Settings.Default.Client
                               select new { empItem.FirstName, empItem.LastName }
                             ).FirstOrDefault();
                string NameApprover = "คุณ " + getName.FirstName + " " + getName.LastName;

                var result = db.FI03_AT_Doc.Where(item => item.Client == Properties.Settings.Default.Client && item.AutoRun == Id).Select(item => item).FirstOrDefault();

                string MailSubject = "For Approve : Asset Transfer - " + DateTime.Now.ToString("dd/MM/yyyy");

                string DocNo = result.DocNo;
                string CreateDate = result.CreateDate;
                string CreateBy = result.CreateBy;
                string DocStatus = result.DocStatus;

                List<FI03_AT_DocItem> prItem = db.FI03_AT_DocItem.Where(item => item.Client == Properties.Settings.Default.Client && item.FI03_AT_Doc == Id && item.ItemNo == 1).Select(item => item).ToList();

                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.Append("<HEAD>");
                sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=UTF-8'> ");
                sb.Append("</HEAD>");
                sb.Append("<body style='font-family: Tahoma,Cordia New,Times New Roman;font-size:12px'>");

                sb.Append("เรียน " + NameApprover + "<br /><br />");

                sb.Append("แจ้ง โอนย้ายทรัพย์สิน Asset Transfer ทำรายการเรียบร้อย<br />");
                sb.Append("<br /><br />");

                sb.Append("<table border='1' style='font-family: Tahoma,Cordia New,Times New Roman;font-size:12px;'>");
                sb.Append("<tr align='center' style='background-color:blue'>");
                sb.Append("<td width=50>NO.</td>");
                sb.Append("<td width=130>Document NO.</td>");
                sb.Append("<td width=80>Create Date</td>");
                sb.Append("<td width=60>Create By</td>");
                sb.Append("<td width=120>Status</td>");
                sb.Append("</tr>");

                int no = 1;
                foreach (var item in prItem)
                {
                    sb.Append("<tr align='center'>");
                    sb.Append("<td valign='top'>" + (no++) + "</td>");
                    sb.Append("<td valign='top'>" + DocNo + "</td>");
                    sb.Append("<td valign='top'>" + DateTime.ParseExact(CreateDate, "yyyyMMdd", null).ToString("dd.MM.yyyy") + "</td>");
                    sb.Append("<td valign='top'>" + CreateBy + "</td>");
                    sb.Append("<td valign='top'>" + DocStatus + "</td>");
                    sb.Append("</tr>");

                    break;
                }

                sb.Append("</table>");
                sb.Append("<br /><br /><br />");
                sb.Append("&nbsp;&nbsp;จึงเรียนมาเพื่อทราบ");
                sb.Append("</body>");
                sb.Append("</html>");

                List<FI03_AT_FlowApprove> ccflow = db.FI03_AT_FlowApprove.Where(item => item.Client == Properties.Settings.Default.Client && item.FI03_AT_Doc == Id).OrderBy(item => item.EmpEmail).ToList();

                //-------------------------------
                MailMessage mail = new MailMessage();
                mail.IsBodyHtml = true;

                mail.From = new MailAddress("info@tscpcl.com");

                mail.To.Add(MailApprover);

                string chkAlreadyMail = "";
                foreach (var item in ccflow) 
                {
                    if (item.EmpEmail == chkAlreadyMail)
                    {
                        chkAlreadyMail = item.EmpEmail;
                    }
                    else {
                        mail.CC.Add(item.EmpEmail);
                        chkAlreadyMail = item.EmpEmail;
                    }
                    
                }

                mail.Subject = MailSubject;
                mail.Body = sb.ToString();
                SmtpServer.Send(mail);
            }

        }

        public void SendMailReject(Guid Id, string txtNote) {
            if (Id == Guid.Empty) { return; }

            /////// ----Config Server Smtp---- //
            SmtpClient SmtpServer = new SmtpClient();
            //SmtpServer.Credentials = new System.Net.NetworkCredential("info@tscpcl.com", "info@1");
            //SmtpServer.Port = 25;            //"192.168.0.197"
            //SmtpServer.Host = "CAS01.thaisteelcable.com";
            SmtpServer.Credentials = new System.Net.NetworkCredential("info@tscpcl.com", "Tscpass9");
            SmtpServer.Port = 25;
            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.EnableSsl = true;
            //------------------------------

            var result = db.FI03_AT_Doc.Where(item => item.Client == Properties.Settings.Default.Client && item.AutoRun == Id).Select(item => item).FirstOrDefault();

            string UserAD = db.MM01_PR_Config.Where(item => item.Client == Properties.Settings.Default.Client && item.Movement == "UserAD").Select(item => item.Description).SingleOrDefault().ToString();
            string PassAD = db.MM01_PR_Config.Where(item => item.Client == Properties.Settings.Default.Client && item.Movement == "PassAD").Select(item => item.Description).SingleOrDefault().ToString();

            string strDomain = "LDAP://thaisteelcable.com";
            DirectoryEntry de = new DirectoryEntry(strDomain, UserAD, PassAD);
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(SAMAccountName=" + result.CreateBy + ")";
            DirectoryEntry de1 = ds.FindOne().GetDirectoryEntry();

            string MailApprover = de1.Properties["mail"].Value.ToString();
            //string MailApprover = "itsared.h@thaisteelcable.com"; //เมลล์สำหรับเทสข้อมูล

            string EmpID = de1.Properties["wWWHomePage"].Value.ToString();

            var getName = db.HR01_Employee.Where(x => x.Client == Properties.Settings.Default.Client && x.EmpID == EmpID).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
            string NameApprover = "คุณ " + getName.FirstName + " " + getName.LastName;

            string MailSubject = "Rejected : Asset Transfer - " + DateTime.Now.ToString("dd/MM/yyyy");

            string DocNo = result.DocNo;
            string CreateDate = result.CreateDate;
            string CreateBy = result.CreateBy;


            string LinkMail = db.MM01_PR_Config.Where(e => e.Client == Properties.Settings.Default.Client && e.Movement == "LinkMailAT" && e.Active == "1")
                                .Select(e => e.Description).SingleOrDefault().ToString();
            //string LinkMail = "http://localhost:54752";

            List<FI03_AT_DocItem> prItem = db.FI03_AT_DocItem.Where(item => item.Client == Properties.Settings.Default.Client && item.FI03_AT_Doc == Id && item.ItemNo == 1).Select(item => item).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<HEAD>");
            sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=UTF-8'> ");
            sb.Append("</HEAD>");
            sb.Append("<body style='font-family: Tahoma,Cordia New,Times New Roman;font-size:12px'>");

            sb.Append("เรียน " + NameApprover + "<br /><br />");
            sb.Append("แจ้ง โอนย้ายทรัพย์สิน Asset Transfer <br />");
            sb.Append("<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ผู้อนุมัติได้ทำการ Rejected Asset Transfer กลับมายังท่าน เนื่องจาก <u>" + txtNote + "</u><br />");
            sb.Append("<br /><br />");

            sb.Append("<table border='1' style='font-family: Tahoma,Cordia New,Times New Roman;font-size:12px;'>");
            sb.Append("<tr align='center' style='background-color:blue'>");
            sb.Append("<td width=50>NO.</td>");
            sb.Append("<td width=130>Document NO.</td>");
            sb.Append("<td width=80>Create Date</td>");
            sb.Append("<td width=60>Create By</td>");
            sb.Append("<td width=120>Link to Web site</td>");
            sb.Append("</tr>");

            int no = 1;
            foreach (var item in prItem)
            {
                sb.Append("<tr align='center'>");
                sb.Append("<td valign='top'>" + (no++) + "</td>");
                sb.Append("<td valign='top'>" + DocNo + "</td>");
                sb.Append("<td valign='top'>" + DateTime.ParseExact(CreateDate, "yyyyMMdd", null).ToString("dd.MM.yyyy") + "</td>");
                sb.Append("<td valign='top'>" + CreateBy + "</td>");
                sb.Append("<td valign='top'><a href='" + LinkMail + "/AT/Reset/" + item.FI03_AT_Doc + "'>Click!</a></td>");
                //sb.Append("<td valign='top'><a href='http://localhost:54752/AT/Reset/" + item.FI03_AT_Doc + "'>Click!</a></td>");
                sb.Append("</tr>");

                break;
            }

            sb.Append("</table>");
            sb.Append("<br /><br /><br />");
            sb.Append("&nbsp;&nbsp;จึงเรียนมาเพื่อทราบ");
            sb.Append("</body>");
            sb.Append("</html>");


            //-------------------------------
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            mail.From = new MailAddress("info@tscpcl.com");

            mail.To.Add(MailApprover);
            mail.Subject = MailSubject;
            mail.Body = sb.ToString();
            SmtpServer.Send(mail);

        }


        public ActionResult UploadFile(string guid)
        {

            if (guid == null || guid == "")
            {
                return Json(new { pathfile = "", filename = "", Error = "Guid Error!!" }, JsonRequestBehavior.AllowGet);
            }

            string pathfile = "", filename = "";

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["files[0]"];

                filename = file.FileName;

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileContentType = file.ContentType;
                    string extension = Path.GetExtension(file.FileName);
                    pathfile = Server.MapPath("~/ATFiles/" + guid);

                    if (!System.IO.Directory.Exists(pathfile))
                    {
                        System.IO.Directory.CreateDirectory(pathfile);
                    }

                    pathfile = Path.Combine(pathfile, file.FileName);
                    if (System.IO.File.Exists(pathfile))
                    {
                        System.IO.File.Delete(pathfile);
                        return Json(new { pathfile = "", filename = "", Error = "ชื่อไฟล์นี้ได้ทำการอัพโหลดไปแล้ว กรูณาเปลี่ยนชื่อไฟล์ใหม่" }, JsonRequestBehavior.AllowGet);
                    }

                    try
                    {
                        file.SaveAs(pathfile);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { pathfile = pathfile, filename = file.FileName, Error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
                    }

                }

            }

            return Json(new { pathfile = "../ATFiles/" + guid, filename = filename, Error = "OK" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult RemoveUploadFile(string guid, string filename)
        {
            string result = "Done";
            try
            {
                string pathfile = Server.MapPath("~/ATFiles/" + guid.ToString() + "/" + filename);

                if (System.IO.File.Exists(pathfile))
                {
                    System.IO.File.Delete(pathfile);
                }
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.Message.ToString();
            }

            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }

       
    }
}