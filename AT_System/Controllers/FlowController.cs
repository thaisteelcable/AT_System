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
    public class FlowController : Controller
    {
        private SAPEntities db = new SAPEntities();

        //
        // GET: /Flow/

        public ActionResult Index()
        {
            return View(db.FI03_AT_FlowMaster.ToList());
        }

        //
        // GET: /Flow/Details/5

        public ActionResult Details(Guid id)
        {
            FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
            if (fi03_at_flowmaster == null)
            {
                return HttpNotFound();
            }
            return View(fi03_at_flowmaster);
        }

        //
        // GET: /Flow/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Flow/Create

        [HttpPost]
        public ActionResult Create(FI03_AT_FlowMaster fi03_at_flowmaster)
        {
            if (ModelState.IsValid)
            {
                fi03_at_flowmaster.AutoRun = Guid.NewGuid();
                db.FI03_AT_FlowMaster.Add(fi03_at_flowmaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fi03_at_flowmaster);
        }

        //
        // GET: /Flow/Edit/5

        public ActionResult Edit(Guid id)
        {
            FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
            if (fi03_at_flowmaster == null)
            {
                return HttpNotFound();
            }
            return View(fi03_at_flowmaster);
        }

        //
        // POST: /Flow/Edit/5

        [HttpPost]
        public ActionResult Edit(FI03_AT_FlowMaster fi03_at_flowmaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fi03_at_flowmaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fi03_at_flowmaster);
        }

        //
        // GET: /Flow/Delete/5

        public ActionResult Delete(Guid id)
        {
            FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
            if (fi03_at_flowmaster == null)
            {
                return HttpNotFound();
            }
            return View(fi03_at_flowmaster);
        }

        //
        // POST: /Flow/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            FI03_AT_FlowMaster fi03_at_flowmaster = db.FI03_AT_FlowMaster.Find(id);
            db.FI03_AT_FlowMaster.Remove(fi03_at_flowmaster);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}