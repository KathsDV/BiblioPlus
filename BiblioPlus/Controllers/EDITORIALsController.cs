using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BiblioPlus;
using BiblioPlus.Models;

namespace BiblioPlus.Controllers
{
    // Nombre del controlador: EDITORIALsController (con 's' minúscula al final)

    public class EDITORIALsController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        // GET: EDITORIALs
        public ActionResult Index(string searchString)
        {
            var editoriales = from e in db.EDITORIALs
                              select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.CurrentFilter = searchString;
                editoriales = editoriales.Where(e => e.Descripcion.Contains(searchString));
            }

            return View(editoriales.ToList());
        }

        // GET: EDITORIALs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EDITORIAL eDITORIAL = db.EDITORIALs.Find(id);
            if (eDITORIAL == null)
            {
                return HttpNotFound();
            }
            return View(eDITORIAL);
        }

        // GET: EDITORIALs/Create
        public ActionResult Create()
        {
            var newEditorial = new EDITORIAL
            {
                Estado = true,
                FechaCreacion = DateTime.Now
            };
            return View(newEditorial);
        }

        // POST: EDITORIALs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdEditorial,Descripcion,Estado,FechaCreacion")] EDITORIAL eDITORIAL)
        {
            if (!eDITORIAL.Estado.HasValue)
            {
                eDITORIAL.Estado = false;
            }
            if (eDITORIAL.FechaCreacion == null || eDITORIAL.FechaCreacion == DateTime.MinValue)
            {
                eDITORIAL.FechaCreacion = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                db.EDITORIALs.Add(eDITORIAL);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Editorial creada correctamente.";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Hubo un error al crear la editorial. Por favor, revise los datos.";
            return View(eDITORIAL);
        }

        // GET: EDITORIALs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EDITORIAL eDITORIAL = db.EDITORIALs.Find(id);
            if (eDITORIAL == null)
            {
                return HttpNotFound();
            }
            return View(eDITORIAL);
        }

        // POST: EDITORIALs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEditorial,Descripcion,Estado,FechaCreacion")] EDITORIAL eDITORIAL)
        {
            if (!eDITORIAL.Estado.HasValue)
            {
                eDITORIAL.Estado = false;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(eDITORIAL).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Editorial actualizada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                    TempData["ErrorMessage"] = "Error al guardar los cambios de la editorial (DB): " + errorMessage;
                    System.Diagnostics.Debug.WriteLine("DataException during Edit EDITORIAL: " + errorMessage);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar la editorial: " + ex.Message;
                    System.Diagnostics.Debug.WriteLine("General Exception during Edit EDITORIAL: " + ex.Message);
                }
            }
            TempData["ErrorMessage"] = "Hubo un error de validación al actualizar la editorial. Por favor, revise los datos.";
            return View(eDITORIAL);
        }

        // GET: EDITORIALs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EDITORIAL eDITORIAL = db.EDITORIALs.Find(id);
            if (eDITORIAL == null)
            {
                return HttpNotFound();
            }
            return View(eDITORIAL);
        }

        // POST: EDITORIALs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                EDITORIAL eDITORIAL = db.EDITORIALs.Find(id);
                // Verificar si hay libros asociados antes de eliminar
                var librosAsociados = db.LIBROes.Any(l => l.IdEditorial == id);
                if (librosAsociados)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar esta editorial porque tiene libros asociados. Por favor, desvincule los libros primero.";
                    return RedirectToAction("Index");
                }

                db.EDITORIALs.Remove(eDITORIAL);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Editorial eliminada correctamente.";
            }
            catch (DataException dex)
            {
                string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                TempData["ErrorMessage"] = "Error al eliminar la editorial (DB): " + errorMessage;
                System.Diagnostics.Debug.WriteLine("DataException during Delete EDITORIAL: " + errorMessage);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al eliminar la editorial: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}