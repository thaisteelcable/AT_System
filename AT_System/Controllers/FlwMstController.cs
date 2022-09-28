using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AT_System.Models;

namespace AT_System.Controllers
{
    public class FlwMstController : Controller
    {
        private SAPEntities db = new SAPEntities();


        //<===================================================== Index()
        public ActionResult Index(string Search, Guid id = new Guid())
        {
            //_AT_FlwMst2 at_FlwMst = new _AT_FlwMst2();
            _AT_Master at_Master = new _AT_Master();

            at_Master.AT_RecCC = db.FI03_AT_RecCostCenter.Find(id);
            if (at_Master.AT_RecCC == null)
            {
                return HttpNotFound();
            }
            at_Master.AT_FlwMst = (db.FI03_AT_FlowMaster.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0 && e.FI03_AT_RecCostCenter == at_Master.AT_RecCC.AutoRun)).OrderBy(e => e.StepNo).ToList();

            //////////if (at_Master.AT_FlwEmp == null) at_Master.AT_FlwEmp = new List<_AT_FlwEmp>();
            //////////foreach (FI03_AT_FlowMaster item in at_Master.AT_FlwMst)
            //////////{
            //////////    _AT_FlwEmp flwemp = new _AT_FlwEmp();
            //////////    flwemp.AT_FlwMst= item;
            //////////    flwemp.AT_Emp = (db.HR01_Employee.Where(e => e.Client == Properties.Settings.Default.Client && e.EmpStatus != "X" && e.EmpID == item.EmpID)).FirstOrDefault();
            //////////    at_Master.AT_FlwEmp.Add(flwemp);
            //////////}

            return View(at_Master);
        }

        //<===================================================== Detail()
        //public ActionResult Details(Guid id = null)
        //{
        //    FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
        //    if (fi03_at_flowmaster == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(fi03_at_flowmaster);
        //}


        //<===================================================== Create()
        public ActionResult Create(Guid id = new Guid())
        {
            ViewBag.RecCC_ID = id;
            ViewBag.RecCC_Name = "";
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(id);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Create(FI03_AT_FlowMaster fi03_at_flowmaster, Guid id = new Guid())
        {
            //var tbRecCC = db.FI03_AT_RecCostCenter.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0).OrderBy(e => e.ReceiverCC).ToList();
            //IEnumerable<SelectListItem> listRecCC = from s in tbRecCC
            //                                        where s.Client == Properties.Settings.Default.Client && s.Status == 0
            //                                        select new SelectListItem
            //                                        {
            //                                            Value = s.AutoRun.ToString(),
            //                                            Text = s.ReceiverCC.Trim()
            //                                        };
            //ViewBag.RecCC = listRecCC.ToList();

            ViewBag.RecCC_ID = fi03_at_flowmaster.FI03_AT_RecCostCenter;
            ViewBag.RecCC_Name = "";
            ViewBag.Error = "";
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_flowmaster.FI03_AT_RecCostCenter);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }


            if (fi03_at_flowmaster.FI03_AT_RecCostCenter == null || fi03_at_flowmaster.FI03_AT_RecCostCenter.ToString().Trim() == "" ||
                fi03_at_flowmaster.StepNo == null || fi03_at_flowmaster.StepNo.ToString().Trim() == "")
            {
                ViewBag.Error = "ใส่ข้อมูลไม่ครบ";
                return View(fi03_at_flowmaster);
            }

            List<HR01_Employee> emp = (db.HR01_Employee.Where(e => e.Client == Properties.Settings.Default.Client && e.EmpStatus != "X" && e.EmpID == fi03_at_flowmaster.EmpID)).ToList();
            if (emp.Count() == 0)
            {
                ViewBag.Error = "รหัสพนักงานไม่ถูกต้อง";
                return View(fi03_at_flowmaster);
            }

            List<FI03_AT_FlowMaster> lt_tmp = (db.FI03_AT_FlowMaster.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0 
                && e.FI03_AT_RecCostCenter == fi03_at_flowmaster.FI03_AT_RecCostCenter && e.StepNo == fi03_at_flowmaster.StepNo).ToList());
            if (lt_tmp.Count() != 0)
            {
                ViewBag.Error = "ใส่เลขที่ Step ซ้ำ";
                return View(fi03_at_flowmaster);
            }

            fi03_at_flowmaster.AutoRun = Guid.NewGuid();

            if (ModelState.IsValid)
            {
                fi03_at_flowmaster.Client = Properties.Settings.Default.Client;
                fi03_at_flowmaster.Status = 0;

                db.FI03_AT_FlowMaster.Add(fi03_at_flowmaster);
                db.SaveChanges();

                return RedirectToAction("Index", new { id = fi03_at_flowmaster.FI03_AT_RecCostCenter });
            }

            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_flowmaster);
        }


        //<===================================================== Edit()
        public ActionResult Edit(Guid id)
        {
            FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
            if (fi03_at_flowmaster == null)
            {
                return HttpNotFound();
            }

            ViewBag.RecCC_Name = "";
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_flowmaster.FI03_AT_RecCostCenter);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            return View(fi03_at_flowmaster);
        }

        [HttpPost]
        public ActionResult Edit(FI03_AT_FlowMaster fi03_at_flowmaster)
        {
            ViewBag.RecCC_Name = "";
            ViewBag.Error = "";
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_flowmaster.FI03_AT_RecCostCenter);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            if (fi03_at_flowmaster.FI03_AT_RecCostCenter == null || fi03_at_flowmaster.FI03_AT_RecCostCenter.ToString().Trim() == "" ||
                fi03_at_flowmaster.StepNo == null || fi03_at_flowmaster.StepNo.ToString().Trim() == "")
            {
                ViewBag.Error = "ใส่ข้อมูลไม่ครบ";
                return View(fi03_at_flowmaster);
            }

            List<HR01_Employee> emp = (db.HR01_Employee.Where(e => e.Client == Properties.Settings.Default.Client && e.EmpStatus != "X" && e.EmpID == fi03_at_flowmaster.EmpID)).ToList();
            if (emp.Count() == 0)
            {
                ViewBag.Error = "รหัสพนักงานไม่ถูกต้อง";
                return View(fi03_at_flowmaster);
            }

            List<FI03_AT_FlowMaster> lt_tmp = (db.FI03_AT_FlowMaster.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0
                && e.FI03_AT_RecCostCenter == fi03_at_flowmaster.FI03_AT_RecCostCenter && e.StepNo == fi03_at_flowmaster.StepNo
                && e.AutoRun != fi03_at_flowmaster.AutoRun).ToList());
            if (lt_tmp.Count() != 0)
            {
                ViewBag.Error = "ใส่เลขที่ Step ซ้ำ";
                return View(fi03_at_flowmaster);
            }

            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_flowmaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = fi03_at_flowmaster.FI03_AT_RecCostCenter });
            }
            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_flowmaster);
        }


        //<===================================================== Delete()
        public ActionResult Delete(Guid id)
        {
            FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
            if (fi03_at_flowmaster == null)
            {
                return HttpNotFound();
            }

            ViewBag.RecCC_Name = "";
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_flowmaster.FI03_AT_RecCostCenter);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            fi03_at_flowmaster.Status = 1;

            return View(fi03_at_flowmaster);
        }

        [HttpPost]
        public ActionResult Delete(FI03_AT_FlowMaster fi03_at_flowmaster)
        {
            ViewBag.RecCC_Name = "";
            ViewBag.Error = "";
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_flowmaster.FI03_AT_RecCostCenter);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            //เช็คเงื่อนไขก่อนลบ
            //

            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_flowmaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = fi03_at_flowmaster.FI03_AT_RecCostCenter });
            }
            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_flowmaster);
        }


        [HttpPost]
        public ActionResult ListEmail()
        {
            string[] mails = db.HR01_Employee.Where(e => e.Client == Properties.Settings.Default.Client && e.EmpStatus == "")
                            .Select(e => e.Email).Distinct().ToArray();

            var autocomplete = db.HR01_Employee.Where(e => e.Client == Properties.Settings.Default.Client && e.EmpStatus == "")
                            .Select(e => new
                            {
                                value = e.EmpID,
                                label = e.Email,
                                desc = e.FirstNameEN + "|" + e.LastNameEN + "|" + e.DivisionCode + "|" + e.DepartmentCode + "|" + e.PositionTitleCode
                            }).ToList();

            return Json(autocomplete);
        }


        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}



    }
}


