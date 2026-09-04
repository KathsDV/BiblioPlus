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

    public class CATEGORIAsController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        // GET: CATEGORIAs
        public ActionResult Index(string searchString)
        {
            var categorias = from c in db.CATEGORIAs
                             select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.CurrentFilter = searchString;
                categorias = categorias.Where(c => c.Descripcion.Contains(searchString));
            }
            return View(categorias.ToList());
        }

        // GET: CATEGORIAs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CATEGORIA cATEGORIA = db.CATEGORIAs.Find(id);
            if (cATEGORIA == null)
            {
                return HttpNotFound();
            }
            return View(cATEGORIA);
        }

        // GET: CATEGORIAs/Create
        public ActionResult Create()
        {
            var newCategoria = new CATEGORIA
            {
                Estado = true,
                FechaCreacion = DateTime.Now
            };
            return View(newCategoria);
        }

        // POST: CATEGORIAs/Create
        // Para protegerse de ataques de tipo Overposting, habilite las propiedades específicas a las que quiere enlazarse. Para obtener
        // más detalles, consulte https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdCategoria,Descripcion,Estado,FechaCreacion")] CATEGORIA cATEGORIA)
        {
            if (!cATEGORIA.Estado.HasValue)
            {
                cATEGORIA.Estado = false;
            }
            if (cATEGORIA.FechaCreacion == null || cATEGORIA.FechaCreacion == DateTime.MinValue)
            {
                cATEGORIA.FechaCreacion = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                db.CATEGORIAs.Add(cATEGORIA);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Categoría creada correctamente.";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Hubo un error al crear la categoría. Por favor, revise los datos.";
            return View(cATEGORIA);
        }

        // GET: CATEGORIAs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CATEGORIA cATEGORIA = db.CATEGORIAs.Find(id);
            if (cATEGORIA == null)
            {
                return HttpNotFound();
            }
            return View(cATEGORIA);
        }

        // POST: CATEGORIAs/Edit/5
        // Para protegerse de ataques de tipo Overposting, habilite las propiedades específicas a las que quiere enlazarse. Para obtener
        // más detalles, consulte https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCategoria,Descripcion,Estado,FechaCreacion")] CATEGORIA cATEGORIA)
        {
            if (!cATEGORIA.Estado.HasValue)
            {
                cATEGORIA.Estado = false;
            }
            // No actualizar FechaCreacion aquí, ya que se supone que es la fecha de creación original
            // A menos que desees permitir la edición, en cuyo caso, la lógica sería diferente.
            // if (cATEGORIA.FechaCreacion == null || cATEGORIA.FechaCreacion == DateTime.MinValue)
            // {
            //     cATEGORIA.FechaCreacion = DateTime.Now; // Esto crearía una nueva fecha de creación en la edición
            // }


            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(cATEGORIA).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Categoría actualizada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                    TempData["ErrorMessage"] = "Error al guardar los cambios de la categoría (DB): " + errorMessage;
                    System.Diagnostics.Debug.WriteLine("DataException during Edit CATEGORIA: " + errorMessage);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar la categoría: " + ex.Message;
                    System.Diagnostics.Debug.WriteLine("General Exception during Edit CATEGORIA: " + ex.Message);
                }
            }
            TempData["ErrorMessage"] = "Hubo un error de validación al actualizar la categoría. Por favor, revise los datos.";
            return View(cATEGORIA);
        }

        // GET: CATEGORIAs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CATEGORIA cATEGORIA = db.CATEGORIAs.Find(id);
            if (cATEGORIA == null)
            {
                return HttpNotFound();
            }
            return View(cATEGORIA);
        }

        // POST: CATEGORIAs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                CATEGORIA cATEGORIA = db.CATEGORIAs.Find(id);
                // Verificar si hay libros asociados antes de eliminar
                var librosAsociados = db.LIBROes.Any(l => l.IdCategoria == id);
                if (librosAsociados)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar esta categoría porque tiene libros asociados. Por favor, desvincule los libros primero.";
                    return RedirectToAction("Index");
                }

                db.CATEGORIAs.Remove(cATEGORIA);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Categoría eliminada correctamente.";
            }
            catch (DataException dex)
            {
                string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                TempData["ErrorMessage"] = "Error al eliminar la categoría (DB): " + errorMessage;
                System.Diagnostics.Debug.WriteLine("DataException during Delete CATEGORIA: " + errorMessage);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al eliminar la categoría: " + ex.Message;
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
