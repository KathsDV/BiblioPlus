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

    public class AUTORsController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        // GET: AUTORs
        public ActionResult Index(string searchString)
        {
            var autores = from a in db.AUTORs
                          select a;

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.CurrentFilter = searchString;
                autores = autores.Where(a => a.Descripcion.Contains(searchString));
            }

            return View(autores.ToList());
        }

        // GET: AUTORs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AUTOR aUTOR = db.AUTORs.Find(id);
            if (aUTOR == null)
            {
                return HttpNotFound();
            }
            return View(aUTOR);
        }

        // GET: AUTORs/Create
        public ActionResult Create()
        {
            // Opcional: Inicializar valores por defecto al cargar el formulario de creación
            var newAutor = new AUTOR
            {
                Estado = true, // Por defecto, Activo
                FechaCreacion = DateTime.Now // Por defecto, fecha actual
            };
            return View(newAutor);
        }

        // POST: AUTORs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdAutor,Descripcion,Estado,FechaCreacion")] AUTOR aUTOR)
        {
            // ************ CORRECCIÓN PARA ASEGURAR VALORES ************
            if (!aUTOR.Estado.HasValue) // Si el checkbox no fue marcado (o llegó null)
            {
                aUTOR.Estado = false; // Asignamos false explícitamente si es null (no marcado)
            }
            if (aUTOR.FechaCreacion == null || aUTOR.FechaCreacion == DateTime.MinValue) // Si la fecha no se seleccionó
            {
                aUTOR.FechaCreacion = DateTime.Now; // Asignamos la fecha actual
            }
            // **********************************************************

            if (ModelState.IsValid)
            {
                db.AUTORs.Add(aUTOR);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Autor creado correctamente.";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Hubo un error al crear el autor. Por favor, revise los datos.";
            return View(aUTOR);
        }

        // GET: AUTORs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AUTOR aUTOR = db.AUTORs.Find(id);
            if (aUTOR == null)
            {
                return HttpNotFound();
            }
            return View(aUTOR);
        }

        // POST: AUTORs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdAutor,Descripcion,Estado,FechaCreacion")] AUTOR aUTOR)
        {
            // ************ CORRECCIÓN PARA ASEGURAR VALORES ************
            if (!aUTOR.Estado.HasValue) // Si el checkbox no fue marcado en el POST
            {
                aUTOR.Estado = false; // Asignamos false explícitamente
            }


            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(aUTOR).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Autor actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                    TempData["ErrorMessage"] = "Error al guardar los cambios del autor (DB): " + errorMessage;
                    System.Diagnostics.Debug.WriteLine("DataException during Edit AUTOR: " + errorMessage);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar el autor: " + ex.Message;
                    System.Diagnostics.Debug.WriteLine("General Exception during Edit AUTOR: " + ex.Message);
                }
            }
            TempData["ErrorMessage"] = "Hubo un error de validación al actualizar el autor. Por favor, revise los datos.";
            return View(aUTOR);
        }

        // GET: AUTORs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AUTOR aUTOR = db.AUTORs.Find(id);
            if (aUTOR == null)
            {
                return HttpNotFound();
            }
            return View(aUTOR);
        }

        // POST: AUTORs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                AUTOR aUTOR = db.AUTORs.Find(id);
                var librosAsociados = db.LIBROes.Any(l => l.IdAutor == id);
                if (librosAsociados)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar este autor porque tiene libros asociados. Por favor, desvincule los libros primero.";
                    return RedirectToAction("Index");
                }

                db.AUTORs.Remove(aUTOR);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Autor eliminado correctamente.";
            }
            catch (DataException dex)
            {
                string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                TempData["ErrorMessage"] = "Error al eliminar el autor (DB): " + errorMessage;
                System.Diagnostics.Debug.WriteLine("DataException during Delete AUTOR: " + errorMessage);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al eliminar el autor: " + ex.Message;
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