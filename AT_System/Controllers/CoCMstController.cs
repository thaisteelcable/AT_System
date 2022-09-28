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
    public class CoCMstController : Controller
    {
        private SAPEntities db = new SAPEntities();


        //<===================================================== Index()
        public ActionResult Index(string Search, Guid id = new Guid())
        {
            _AT_Master at_Master = new _AT_Master();

            at_Master.AT_RecCC = db.FI03_AT_RecCostCenter.Find(id);
            if (at_Master.AT_RecCC == null)
            {
                return HttpNotFound();
            }

            string strRecCC = at_Master.AT_RecCC.AutoRun.ToString();

            at_Master.AT_CostCenter = (db.FI03_AT_CostCenter.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0 && e.FI03_AT_RecCostCenter == strRecCC)).OrderBy(e => e.AssetClass).ToList();

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
        public ActionResult Create(FI03_AT_CostCenter fi03_at_costcenter, Guid id = new Guid())
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

            ViewBag.RecCC_ID = fi03_at_costcenter.FI03_AT_RecCostCenter;
            ViewBag.RecCC_Name = "";

            ViewBag.Error = "";

            Guid strGuid = new Guid();
            strGuid = new Guid(fi03_at_costcenter.FI03_AT_RecCostCenter);

            //FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_costcenter.FI03_AT_RecCostCenter);
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(strGuid);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }


            if (fi03_at_costcenter.FI03_AT_RecCostCenter == null || fi03_at_costcenter.FI03_AT_RecCostCenter.ToString().Trim() == "" ||
                fi03_at_costcenter.AssetClass == null || fi03_at_costcenter.AssetClass.ToString().Trim() == "" ||
                fi03_at_costcenter.CostCenter == null || fi03_at_costcenter.CostCenter.ToString().Trim() == "" ||
                fi03_at_costcenter.Room == null || fi03_at_costcenter.Room.ToString().Trim() == "")
            {
                ViewBag.Error = "ใส่ข้อมูลไม่ครบ";
                return View(fi03_at_costcenter);
            }

            List<FI03_AT_CostCenter> lt_tmp = (db.FI03_AT_CostCenter.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0
                && e.FI03_AT_RecCostCenter == fi03_at_costcenter.FI03_AT_RecCostCenter && e.AssetClass == fi03_at_costcenter.AssetClass).ToList());
            if (lt_tmp.Count() != 0)
            {
                ViewBag.Error = "ใส่ข้อมูล Cost Center ซ้ำ";
                return View(fi03_at_costcenter);
            }

            fi03_at_costcenter.AutoRun = Guid.NewGuid();

            if (ModelState.IsValid)
            {
                fi03_at_costcenter.Client = Properties.Settings.Default.Client;
                fi03_at_costcenter.Status = 0;

                db.FI03_AT_CostCenter.Add(fi03_at_costcenter);
                db.SaveChanges();

                return RedirectToAction("Index", new { id = fi03_at_costcenter.FI03_AT_RecCostCenter });
            }

            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_costcenter);
        }


        //<===================================================== Edit()
        public ActionResult Edit(Guid id)
        {
            FI03_AT_CostCenter fi03_at_costcenter = db.FI03_AT_CostCenter.Find(id);
            if (fi03_at_costcenter == null)
            {
                return HttpNotFound();
            }

            ViewBag.RecCC_Name = "";

            Guid strGuid = new Guid();
            strGuid = new Guid(fi03_at_costcenter.FI03_AT_RecCostCenter);
            //FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_costcenter.FI03_AT_RecCostCenter);
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(strGuid);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            return View(fi03_at_costcenter);
        }

        [HttpPost]
        public ActionResult Edit(FI03_AT_CostCenter fi03_at_costcenter)
        {
            ViewBag.RecCC_Name = "";
            ViewBag.Error = "";

            Guid strGuid = new Guid();
            strGuid = new Guid(fi03_at_costcenter.FI03_AT_RecCostCenter);
            //FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_costcenter.FI03_AT_RecCostCenter);
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(strGuid);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            if (fi03_at_costcenter.FI03_AT_RecCostCenter == null || fi03_at_costcenter.FI03_AT_RecCostCenter.ToString().Trim() == "" ||
                fi03_at_costcenter.AssetClass == null || fi03_at_costcenter.AssetClass.ToString().Trim() == "" ||
                fi03_at_costcenter.CostCenter == null || fi03_at_costcenter.CostCenter.ToString().Trim() == "" ||
                fi03_at_costcenter.Room == null || fi03_at_costcenter.Room.ToString().Trim() == "")
            {
                ViewBag.Error = "ใส่ข้อมูลไม่ครบ";
                return View(fi03_at_costcenter);
            }

            //List<FI03_AT_CostCenter> lt_tmp = (db.FI03_AT_CostCenter.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0
            //    && e.FI03_AT_RecCostCenter == fi03_at_costcenter.FI03_AT_RecCostCenter && e.AssetClass == fi03_at_costcenter.AssetClass
            //    && e.AutoRun != fi03_at_costcenter.AutoRun).ToList());
            //if (lt_tmp.Count() != 0)
            //{
            //    return View(fi03_at_costcenter);
            //}

            //List<FI03_AT_CostCenter> lt_tmp = (db.FI03_AT_CostCenter.Where(e => e.Client == Properties.Settings.Default.Client && e.Status == 0
            //    && e.FI03_AT_RecCostCenter == fi03_at_costcenter.FI03_AT_RecCostCenter && e.AssetClass == fi03_at_costcenter.AssetClass
            //    && e.AutoRun == fi03_at_costcenter.AutoRun).ToList());
            //if (lt_tmp.Count() == 0)
            //{
            //    return View(fi03_at_costcenter);
            //}

            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_costcenter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = fi03_at_costcenter.FI03_AT_RecCostCenter });
            }
            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_costcenter);
        }


        //<===================================================== Delete()
        public ActionResult Delete(Guid id)
        {
            FI03_AT_CostCenter fi03_at_costcenter = db.FI03_AT_CostCenter.Find(id);
            if (fi03_at_costcenter == null)
            {
                return HttpNotFound();
            }

            ViewBag.RecCC_Name = "";

            Guid strGuid = new Guid();
            strGuid = new Guid(fi03_at_costcenter.FI03_AT_RecCostCenter);
            //FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_costcenter.FI03_AT_RecCostCenter);
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(strGuid);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            fi03_at_costcenter.Status = 1;

            return View(fi03_at_costcenter);
        }

        [HttpPost]
        public ActionResult Delete(FI03_AT_CostCenter fi03_at_costcenter)
        {
            ViewBag.RecCC_Name = "";
            ViewBag.Error = "";

            Guid strGuid = new Guid();
            strGuid = new Guid(fi03_at_costcenter.FI03_AT_RecCostCenter);
            //FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(fi03_at_costcenter.FI03_AT_RecCostCenter);
            FI03_AT_RecCostCenter fi03_at_reccc = db.FI03_AT_RecCostCenter.Find(strGuid);
            if (fi03_at_reccc != null)
            {
                ViewBag.RecCC_Name = fi03_at_reccc.ReceiverCC;
            }

            //เช็คเงื่อนไขก่อนลบ
            //

            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_costcenter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = fi03_at_costcenter.FI03_AT_RecCostCenter });
            }
            ViewBag.Error = "ไม่สามารถบันทึกได้";
            return View(fi03_at_costcenter);
        }



        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}



    }
}
