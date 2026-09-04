using BiblioPlus.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BiblioPlus.Controllers
{
    public class OrdenCompraController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        private bool IsAdmin()
        {
            // Asumo que IdTipoPersona 1 es para Administradores
            return Session["IdTipoPersona"] != null && (int)Session["IdTipoPersona"] == 1;
        }

        // Método auxiliar para poblar los estados de la orden de compra
        // NOTA: Este método ahora almacena List<SelectListItem> directamente en ViewBag
        private void PopulateEstadoOrdenCompraDropdown(string selectedEstado = null)
        {
            var estadosOrden = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pendiente", Text = "Pendiente", Selected = (selectedEstado == "Pendiente") },
                new SelectListItem { Value = "Completada", Text = "Completada", Selected = (selectedEstado == "Completada") },
                new SelectListItem { Value = "Cancelada", Text = "Cancelada", Selected = (selectedEstado == "Cancelada") }
                // Puedes agregar más estados si tu modelo de negocio los contempla
            };
            ViewBag.EstadosOrdenCompra = estadosOrden;
        }

        public ActionResult Index(string searchString) // Agregado searchString para la funcionalidad de búsqueda
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            var ordenes = db.ORDEN_COMPRA.Include(o => o.LIBRO).Include(o => o.Persona);

            if (!String.IsNullOrEmpty(searchString))
            {
                // Filtro por nombre de la persona o apellido, ajusta según tus campos de Persona
                ordenes = ordenes.Where(s => s.Persona.Nombre.Contains(searchString) || s.Persona.Apellido.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString; // Para mantener el texto en el campo de búsqueda
            return View(ordenes.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var orden = db.ORDEN_COMPRA.Include(o => o.LIBRO).Include(o => o.Persona).SingleOrDefault(o => o.IdOrdenCompra == id);
            if (orden == null) return HttpNotFound();

            return View(orden);
        }

        public ActionResult Create()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.IdLibro = new SelectList(db.LIBROes, "IdLibro", "Titulo");
            // Asumo que IdTipoPersona 1 es para "Administradores" o "Usuarios" que pueden realizar órdenes, ajusta si es otro rol
            ViewBag.IdPersona = new SelectList(db.Personas.Where(p => p.IdTipoPersona == 1), "IdPersona", "Nombre");
            PopulateEstadoOrdenCompraDropdown(); // Se agrega aquí para que la vista Create también lo tenga si se decide usar
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdPersona,IdLibro,Cantidad")] ORDEN_COMPRA orden)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                orden.FechaCompra = DateTime.Now;
                orden.Estado = "Pendiente"; // Estado inicial para una nueva orden
                orden.FechaCreacion = DateTime.Now;

                var libro = db.LIBROes.Find(orden.IdLibro);
                if (libro != null)
                {
                    libro.StockActual = (libro.StockActual ?? 0) + orden.Cantidad;
                    db.Entry(libro).State = EntityState.Modified;
                }

                db.ORDEN_COMPRA.Add(orden);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Orden de compra creada exitosamente.";
                return RedirectToAction("Index");
            }

            ViewBag.IdLibro = new SelectList(db.LIBROes, "IdLibro", "Titulo", orden.IdLibro);
            ViewBag.IdPersona = new SelectList(db.Personas.Where(p => p.IdTipoPersona == 1), "IdPersona", "Nombre", orden.IdPersona);
            PopulateEstadoOrdenCompraDropdown(orden.Estado); // Vuelve a poblar si hay errores
            return View(orden);
        }

        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var orden = db.ORDEN_COMPRA.Find(id);
            if (orden == null) return HttpNotFound();

            ViewBag.IdLibro = new SelectList(db.LIBROes, "IdLibro", "Titulo", orden.IdLibro);
            ViewBag.IdPersona = new SelectList(db.Personas.Where(p => p.IdTipoPersona == 1), "IdPersona", "Nombre", orden.IdPersona);
            PopulateEstadoOrdenCompraDropdown(orden.Estado); // ¡Aquí se pobla el ViewBag.EstadosOrdenCompra!
            return View(orden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdOrdenCompra,IdPersona,IdLibro,Cantidad,FechaCompra,Estado,FechaCreacion")] ORDEN_COMPRA orden)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                var original = db.ORDEN_COMPRA.AsNoTracking().FirstOrDefault(o => o.IdOrdenCompra == orden.IdOrdenCompra);

                if (original != null)
                {
                    // Si cambia el libro o la cantidad, ajustar el stock
                    if (original.IdLibro != orden.IdLibro || original.Cantidad != orden.Cantidad)
                    {
                        // Devolver stock del libro original
                        var libroOriginal = db.LIBROes.Find(original.IdLibro);
                        if (libroOriginal != null)
                        {
                            libroOriginal.StockActual = (libroOriginal.StockActual ?? 0) - original.Cantidad;
                            db.Entry(libroOriginal).State = EntityState.Modified;
                        }

                        // Añadir stock al nuevo libro o ajustar el actual
                        var libroNuevo = db.LIBROes.Find(orden.IdLibro);
                        if (libroNuevo != null)
                        {
                            libroNuevo.StockActual = (libroNuevo.StockActual ?? 0) + orden.Cantidad;
                            db.Entry(libroNuevo).State = EntityState.Modified;
                        }
                    }
                    // Si solo cambia el estado o cualquier otro campo no relacionado con el stock, el stock no se ajusta aquí
                }

                db.Entry(orden).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Orden de compra actualizada exitosamente.";
                return RedirectToAction("Index");
            }

            ViewBag.IdLibro = new SelectList(db.LIBROes, "IdLibro", "Titulo", orden.IdLibro);
            ViewBag.IdPersona = new SelectList(db.Personas.Where(p => p.IdTipoPersona == 1), "IdPersona", "Nombre", orden.IdPersona);
            PopulateEstadoOrdenCompraDropdown(orden.Estado); // ¡Se repuebla si el modelo no es válido!
            TempData["ErrorMessage"] = "Hubo un error al actualizar la orden de compra. Por favor, revisa los datos.";
            return View(orden);
        }

        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var orden = db.ORDEN_COMPRA.Include(o => o.LIBRO).Include(o => o.Persona).SingleOrDefault(o => o.IdOrdenCompra == id);
            if (orden == null) return HttpNotFound();

            return View(orden);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToAction("Index", "Home");
            }

            var orden = db.ORDEN_COMPRA.Find(id);

            if (orden != null)
            {
                // Antes de eliminar la orden, ajustar el stock del libro
                var libro = db.LIBROes.Find(orden.IdLibro);
                if (libro != null)
                {
                    libro.StockActual = (libro.StockActual ?? 0) - orden.Cantidad;
                    db.Entry(libro).State = EntityState.Modified;
                }

                db.ORDEN_COMPRA.Remove(orden);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Orden de compra eliminada exitosamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult GetStock(int idLibro)
        {
            var libro = db.LIBROes.Find(idLibro);
            return Json(new { stock = libro?.StockActual ?? 0 }, JsonRequestBehavior.AllowGet);
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