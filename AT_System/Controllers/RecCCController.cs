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

    public class RecCCController : Controller
    {
        private SAPEntities db = new SAPEntities();


        public ActionResult Index(string Search, string rec_cc = "")
        {

            if (!string.IsNullOrEmpty(Search) && rec_cc != "")
            {
                return View(db.FI03_AT_RecCostCenter.Where(v => v.Client == Properties.Settings.Default.Client && (v.Status == 0 || v.Status == 9) && v.ReceiverCC.Contains(rec_cc)).OrderBy(e => new { e.ReceiverCC, e.DepartmentCode }).ToList());
            }
            else
            {
                return View(db.FI03_AT_RecCostCenter.Where(v => v.Client == Properties.Settings.Default.Client && (v.Status == 0 || v.Status == 9)).OrderBy(e => new { e.ReceiverCC, e.DepartmentCode }).ToList());
            }

        }


        //<========================================================== Details()
        //public ActionResult Details(Guid id = null)       
        //{
        //    FI03_AT_RecCostCenter fi03_at_reccostcenter = db.FI03_AT_RecCostCenter.Find(id);
        //    if (fi03_at_reccostcenter == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(fi03_at_reccostcenter);
        //}


        //<========================================================== Create()
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FI03_AT_RecCostCenter fi03_at_reccostcenter)
        {
            ViewBag.Error = "";
            if (fi03_at_reccostcenter.ReceiverCC == null || fi03_at_reccostcenter.ReceiverCC.ToString().Trim() == "" ||
                fi03_at_reccostcenter.FieldCode == null || fi03_at_reccostcenter.FieldCode.ToString().Trim() == "" ||
                fi03_at_reccostcenter.DepartmentCode == null || fi03_at_reccostcenter.DepartmentCode.ToString().Trim() == "")
            {
                ViewBag.Error = "ใส่ข้อมูลไม่ครบ";
                return View(fi03_at_reccostcenter);
            }

            List<FI03_AT_RecCostCenter> lt_tmp = (db.FI03_AT_RecCostCenter.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0 && e.ReceiverCC == fi03_at_reccostcenter.ReceiverCC).ToList());
            if (lt_tmp.Count() != 0)
            {
                ViewBag.Error = "ใส่ข้อมูล ReceiverCC ซ้ำ ";
                return View(fi03_at_reccostcenter);
            }

            fi03_at_reccostcenter.AutoRun = Guid.NewGuid();
            if (ModelState.IsValid)
            {
                fi03_at_reccostcenter.Client = Properties.Settings.Default.Client;
                fi03_at_reccostcenter.DepartmentCode = fi03_at_reccostcenter.DepartmentCode;
                fi03_at_reccostcenter.FieldCode = fi03_at_reccostcenter.FieldCode;
                fi03_at_reccostcenter.Status = 0;

                db.FI03_AT_RecCostCenter.Add(fi03_at_reccostcenter);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_reccostcenter);
        }


        //<========================================================== Edit()
        public ActionResult Edit(Guid id)
        {
            FI03_AT_RecCostCenter fi03_at_reccostcenter = db.FI03_AT_RecCostCenter.Find(id);
            if (fi03_at_reccostcenter == null)
            {
                return HttpNotFound();
            }
            return View(fi03_at_reccostcenter);
        }

        [HttpPost]
        public ActionResult Edit(FI03_AT_RecCostCenter fi03_at_reccostcenter)
        {
            ViewBag.Error = "";
            if (fi03_at_reccostcenter.ReceiverCC == null || fi03_at_reccostcenter.ReceiverCC.ToString().Trim() == "" ||
                fi03_at_reccostcenter.FieldCode == null || fi03_at_reccostcenter.FieldCode.ToString().Trim() == "" ||
                fi03_at_reccostcenter.DepartmentCode == null || fi03_at_reccostcenter.DepartmentCode.ToString().Trim() == "")
            {
                ViewBag.Error = "ใส่ข้อมูลไม่ครบ";
                return View(fi03_at_reccostcenter);
            }

            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_reccostcenter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_reccostcenter);
        }


        //<========================================================== Delete()
        public ActionResult Delete(Guid id)
        {
            FI03_AT_RecCostCenter fi03_at_reccostcenter = db.FI03_AT_RecCostCenter.Find(id);
            if (fi03_at_reccostcenter == null)
            {
                return HttpNotFound();
            }
            fi03_at_reccostcenter.Status = 1;
            return View(fi03_at_reccostcenter);
        }

        [HttpPost]
        public ActionResult Delete(FI03_AT_RecCostCenter fi03_at_reccostcenter)
        {

            //เช็คเงื่อนไขก่อนลบ
            //
            ViewBag.Error = "";
            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_reccostcenter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Error = "ไม่สามารถลบได้";
            return View(fi03_at_reccostcenter);
        }


        //<========================================================== ???()
        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}


    }
}


//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        //<========================================================== ACFlowMaster()
        //public ActionResult Flow_Index(Guid id)
        //{
        //    _AT_FlwMst2 at_FlwMst = new _AT_FlwMst2();

        //    at_FlwMst.AT_RecCC = db.FI03_AT_RecCostCenter.Find(id);
        //    if (at_FlwMst.AT_RecCC == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    at_FlwMst.AT_FlwMst = (db.FI03_AT_FlowMaster.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0 && e.FI03_AT_RecCostCenter == at_FlwMst.AT_RecCC.AutoRun)).OrderBy(e => e.StepNo).ToList();

        //    return View(at_FlwMst);
        //}

        //[HttpPost]
        //public ActionResult Flow_Index(FI03_AT_RecCostCenter fi03_at_reccostcenter)
        //{
        //    return View();
        //}


        //<========================================================== ACCostCenter()
        //public ActionResult ACCostCenter(Guid id)
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult ACCostCenter(FI03_AT_RecCostCenter fi03_at_reccostcenter)
        //{
        //    return View();
        //}

