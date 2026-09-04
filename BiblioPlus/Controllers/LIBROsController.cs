using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BiblioPlus.Models; // Asegúrate de que este namespace sea el correcto para tu modelo

namespace BiblioPlus.Controllers
{

    public class LIBROsController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        // Método auxiliar para obtener la lista de estados del libro
        private List<SelectListItem> GetEstadosLibroList(string selectedEstado = null)
        {
            var estados = new List<SelectListItem>
            {
                new SelectListItem { Value = "Libre", Text = "Libre" },
                new SelectListItem { Value = "Prestado", Text = "Prestado" },
                new SelectListItem { Value = "En Reparacion", Text = "En Reparación" },
                new SelectListItem { Value = "Extraviado", Text = "Extraviado" }
            };

            if (!string.IsNullOrEmpty(selectedEstado))
            {
                var selectedItem = estados.FirstOrDefault(e => e.Value == selectedEstado);
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            return estados;
        }

        // Métodos auxiliares para Categorías, Editoriales, Autores
        private List<SelectListItem> GetCategoriasList(int? selectedId = null)
        {
            var categorias = db.CATEGORIAs.Select(c => new SelectListItem
            {
                Value = c.IdCategoria.ToString(),
                Text = c.Descripcion
            }).ToList();

            if (selectedId.HasValue)
            {
                var selectedItem = categorias.FirstOrDefault(c => c.Value == selectedId.Value.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            return categorias;
        }

        private List<SelectListItem> GetEditorialesList(int? selectedId = null)
        {
            var editoriales = db.EDITORIALs.Select(e => new SelectListItem
            {
                Value = e.IdEditorial.ToString(),
                Text = e.Descripcion
            }).ToList();

            if (selectedId.HasValue)
            {
                var selectedItem = editoriales.FirstOrDefault(e => e.Value == selectedId.Value.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            return editoriales;
        }

        private List<SelectListItem> GetAutoresList(int? selectedId = null)
        {
            var autores = db.AUTORs.Select(a => new SelectListItem
            {
                Value = a.IdAutor.ToString(),
                Text = a.Descripcion
            }).ToList();

            if (selectedId.HasValue)
            {
                var selectedItem = autores.FirstOrDefault(a => a.Value == selectedId.Value.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            return autores;
        }


        // GET: LIBROs
        public ActionResult Index(string searchString)
        {
            var libros = db.LIBROes.Include(l => l.AUTOR)
                                   .Include(l => l.CATEGORIA)
                                   .Include(l => l.EDITORIAL);

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.CurrentFilter = searchString;
                libros = libros.Where(l => l.Titulo.Contains(searchString) ||
                                           l.AUTOR.Descripcion.Contains(searchString) ||
                                           l.EDITORIAL.Descripcion.Contains(searchString) ||
                                           l.CATEGORIA.Descripcion.Contains(searchString));
            }

            // Aquí podrías añadir un filtro por stock si lo necesitaras
            // Por ejemplo: libros = libros.Where(l => l.StockActual > 0);

            return View(libros.ToList());
        }

        // GET: LIBROs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LIBRO lIBRO = db.LIBROes.Include(l => l.AUTOR)
                                     .Include(l => l.CATEGORIA)
                                     .Include(l => l.EDITORIAL)
                                     .FirstOrDefault(l => l.IdLibro == id);
            if (lIBRO == null)
            {
                return HttpNotFound();
            }
            return View(lIBRO);
        }

        // GET: LIBROs/Create
        public ActionResult Create()
        {
            ViewBag.EstadosLibro = GetEstadosLibroList();
            ViewBag.IdCategoria = GetCategoriasList();
            ViewBag.IdEditorial = GetEditorialesList();
            ViewBag.IdAutor = GetAutoresList();
            // No necesitamos ViewBag para StockActual aquí, el campo en la vista lo manejará
            return View();
        }

        // POST: LIBROs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Incluir StockActual en el Bind para que se reciba desde el formulario
        public ActionResult Create([Bind(Include = "IdLibro,Titulo,Edicion,AnioPublicacion,IdCategoria,IdEditorial,IdAutor,Estado,StockActual")] LIBRO lIBRO)
        {
            if (ModelState.IsValid)
            {
                lIBRO.FechaCreacion = DateTime.Now;
                // Asegúrate de que el estado inicial de un libro nuevo sea "Libre"
                // y que el stock sea el que se ingresó.
                if (string.IsNullOrEmpty(lIBRO.Estado))
                {
                    lIBRO.Estado = "Libre"; // Valor por defecto si no se selecciona
                }
                if (!lIBRO.StockActual.HasValue) // Si no se envía un valor para StockActual
                {
                    lIBRO.StockActual = 0; // O el valor que consideres por defecto
                }

                db.LIBROes.Add(lIBRO);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Libro creado correctamente.";
                return RedirectToAction("Index");
            }

            string errors = string.Join("; ", ModelState.Values
                                                .SelectMany(x => x.Errors)
                                                .Select(x => x.ErrorMessage));
            TempData["ErrorMessage"] = "Hubo un error al crear el libro: " + errors + ". Por favor, revise los datos.";

            // Recargar ViewBags si hay errores de validación
            ViewBag.EstadosLibro = GetEstadosLibroList(lIBRO.Estado);
            ViewBag.IdCategoria = GetCategoriasList(lIBRO.IdCategoria);
            ViewBag.IdEditorial = GetEditorialesList(lIBRO.IdEditorial);
            ViewBag.IdAutor = GetAutoresList(lIBRO.IdAutor);
            return View(lIBRO);
        }

        // GET: LIBROs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LIBRO lIBRO = db.LIBROes.Find(id);
            if (lIBRO == null)
            {
                return HttpNotFound();
            }

            ViewBag.EstadosLibro = GetEstadosLibroList(lIBRO.Estado);
            ViewBag.IdCategoria = GetCategoriasList(lIBRO.IdCategoria);
            ViewBag.IdEditorial = GetEditorialesList(lIBRO.IdEditorial);
            ViewBag.IdAutor = GetAutoresList(lIBRO.IdAutor);
            // StockActual ya viene en el modelo lIBRO, no necesita ViewBag
            return View(lIBRO);
        }

        // POST: LIBROs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Incluir StockActual en el Bind para que se reciba desde el formulario
        public ActionResult Edit([Bind(Include = "IdLibro,Titulo,Edicion,AnioPublicacion,IdCategoria,IdEditorial,IdAutor,Estado,FechaCreacion,StockActual")] LIBRO lIBRO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingLibro = db.LIBROes.Find(lIBRO.IdLibro);

                    if (existingLibro == null)
                    {
                        TempData["ErrorMessage"] = "El libro no se encontró en la base de datos.";
                        return RedirectToAction("Index");
                    }

                    // Actualizar todas las propiedades que pueden ser editadas, incluyendo StockActual
                    existingLibro.Titulo = lIBRO.Titulo;
                    existingLibro.Edicion = lIBRO.Edicion;
                    existingLibro.AnioPublicacion = lIBRO.AnioPublicacion;
                    existingLibro.Estado = lIBRO.Estado;
                    existingLibro.IdCategoria = lIBRO.IdCategoria;
                    existingLibro.IdEditorial = lIBRO.IdEditorial;
                    existingLibro.IdAutor = lIBRO.IdAutor;
                    existingLibro.StockActual = lIBRO.StockActual; // ¡IMPORTANTE: Actualizar StockActual!

                    db.Entry(existingLibro).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Libro actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    string errorMessage = dex.InnerException != null ? dex.InnerException.Message : dex.Message;
                    TempData["ErrorMessage"] = "Error al guardar los cambios del libro (DB): " + errorMessage;
                    System.Diagnostics.Debug.WriteLine("DataException during Edit: " + errorMessage);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar el libro: " + ex.Message;
                    System.Diagnostics.Debug.WriteLine("General Exception during Edit: " + ex.Message);
                }
            }
            else // Si ModelState no es válido
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();
                TempData["ErrorMessage"] = "Hubo un error de validación al actualizar el libro: " + string.Join("; ", errors) + ". Por favor, revise los datos.";
                System.Diagnostics.Debug.WriteLine("ModelState Errors during Edit: " + string.Join("; ", errors));
            }

            // Recargar ViewBags si hay errores de validación
            ViewBag.EstadosLibro = GetEstadosLibroList(lIBRO.Estado);
            ViewBag.IdCategoria = GetCategoriasList(lIBRO.IdCategoria);
            ViewBag.IdEditorial = GetEditorialesList(lIBRO.IdEditorial);
            ViewBag.IdAutor = GetAutoresList(lIBRO.IdAutor);
            return View(lIBRO);
        }

        // GET: LIBROs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LIBRO lIBRO = db.LIBROes.Include(l => l.AUTOR)
                                     .Include(l => l.CATEGORIA)
                                     .Include(l => l.EDITORIAL)
                                     .FirstOrDefault(l => l.IdLibro == id);
            if (lIBRO == null)
            {
                return HttpNotFound();
            }
            return View(lIBRO);
        }

        // POST: LIBROs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                LIBRO lIBRO = db.LIBROes.Find(id);
                if (lIBRO == null)
                {
                    return HttpNotFound();
                }

                // En lugar de "eliminar" el libro, lo marcamos como "Extraviado"
                // y podríamos considerar reducir su stock si el libro "extraviado"
                // representa una copia específica que ya no está disponible.
                // Por ahora, solo lo marcamos como extraviado como lo tienes.
                lIBRO.Estado = "Extraviado";
                db.Entry(lIBRO).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Libro marcado como 'Extraviado' correctamente.";
            }
            catch (DataException)
            {
                TempData["ErrorMessage"] = "No se pudo modificar el estado del libro. Verifique las relaciones o inténtelo de nuevo.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al eliminar el libro: " + ex.Message;
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