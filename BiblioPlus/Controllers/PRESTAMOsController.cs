using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BiblioPlus.Models;

namespace BiblioPlus.Controllers
{
    public class PRESTAMOsController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        public ActionResult Admin(int? idPrestamo, string nombre, string libro, string estado,
                                DateTime? fechaPrestamoDesde, DateTime? fechaPrestamoHasta,
                                DateTime? fechaDevolucionDesde, DateTime? fechaDevolucionHasta)
        {
            // Verificar y aplicar penalizaciones por retrasos
            VerificarYAplicarPenalizaciones();
            var prestamos = db.PRESTAMOes.Include(p => p.LIBRO)
                                         .Include(p => p.Persona)
                                         .Include(p => p.ESTADO_PRESTAMO)
                                         .Where(p => p.Estado == true);

            if (idPrestamo.HasValue)
                prestamos = prestamos.Where(p => p.IdPrestamo == idPrestamo.Value);

            if (!string.IsNullOrEmpty(nombre))
                prestamos = prestamos.Where(p => p.Persona.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(libro))
                prestamos = prestamos.Where(p => p.LIBRO.Titulo.Contains(libro));

            if (!string.IsNullOrEmpty(estado))
                prestamos = prestamos.Where(p => p.ESTADO_PRESTAMO.Descripcion == estado);

            if (fechaPrestamoDesde.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) >= fechaPrestamoDesde.Value.Date);

            if (fechaPrestamoHasta.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) <= fechaPrestamoHasta.Value.Date);

            if (fechaDevolucionDesde.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaDevolucion) >= fechaDevolucionDesde.Value.Date);

            if (fechaDevolucionHasta.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaDevolucion) <= fechaDevolucionHasta.Value.Date);

            // Llenar estados para el dropdown
            var estados = db.ESTADO_PRESTAMO.Select(e => e.Descripcion).Distinct().ToList();
            ViewBag.Estados = estados;

            // Guardar filtros para que se muestren en los campos del modal
            ViewBag.IdPrestamo = idPrestamo;
            ViewBag.Nombre = nombre;
            ViewBag.Libro = libro;
            ViewBag.Estado = estado;
            ViewBag.FechaPrestamoDesde = fechaPrestamoDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaPrestamoHasta = fechaPrestamoHasta?.ToString("yyyy-MM-dd");
            ViewBag.FechaDevolucionDesde = fechaDevolucionDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaDevolucionHasta = fechaDevolucionHasta?.ToString("yyyy-MM-dd");

            return View(prestamos.OrderByDescending(p => p.FechaCreacion).ToList());
        }

        public ActionResult Index(int? idPrestamo, string libro, string estado,
                                 DateTime? fechaPrestamoDesde, DateTime? fechaPrestamoHasta,
                                 DateTime? fechaDevolucionDesde, DateTime? fechaDevolucionHasta)
        {
            var usuario = Session["Usuario"] as Persona;
            if (usuario == null)
                return RedirectToAction("Login", "Cuenta");

            var prestamos = db.PRESTAMOes.Include(p => p.LIBRO)
                                         .Include(p => p.Persona)
                                         .Include(p => p.ESTADO_PRESTAMO)
                                         .Where(p => p.IdPersona == usuario.IdPersona && p.Estado == true);

            if (idPrestamo.HasValue)
                prestamos = prestamos.Where(p => p.IdPrestamo == idPrestamo.Value);

            if (!string.IsNullOrEmpty(libro))
                prestamos = prestamos.Where(p => p.LIBRO.Titulo.Contains(libro));

            if (!string.IsNullOrEmpty(estado))
                prestamos = prestamos.Where(p => p.ESTADO_PRESTAMO.Descripcion == estado);

            if (fechaPrestamoDesde.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) >= fechaPrestamoDesde.Value.Date);

            if (fechaPrestamoHasta.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) <= fechaPrestamoHasta.Value.Date);

            if (fechaDevolucionDesde.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaDevolucion) >= fechaDevolucionDesde.Value.Date);

            if (fechaDevolucionHasta.HasValue)
                prestamos = prestamos.Where(p => DbFunctions.TruncateTime(p.FechaDevolucion) <= fechaDevolucionHasta.Value.Date);

            // Lista de estados para el dropdown
            var estados = db.ESTADO_PRESTAMO.Select(e => e.Descripcion).Distinct().ToList();
            ViewBag.Estados = estados;

            // Mantener valores en el modal
            ViewBag.IdPrestamo = idPrestamo;
            ViewBag.Libro = libro;
            ViewBag.Estado = estado;
            ViewBag.FechaPrestamoDesde = fechaPrestamoDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaPrestamoHasta = fechaPrestamoHasta?.ToString("yyyy-MM-dd");
            ViewBag.FechaDevolucionDesde = fechaDevolucionDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaDevolucionHasta = fechaDevolucionHasta?.ToString("yyyy-MM-dd");

            return View(prestamos.OrderByDescending(p => p.FechaCreacion).ToList());
        }

        // GET: PRESTAMOes/Details/5  <-- ¡ESTE ES EL MÉTODO QUE FALTABA!
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Asegúrate de incluir las propiedades relacionadas si las necesitas en la vista Details
            PRESTAMO pRESTAMO = db.PRESTAMOes
                                 .Include(p => p.LIBRO)
                                 .Include(p => p.Persona)
                                 .Include(p => p.ESTADO_PRESTAMO)
                                 .FirstOrDefault(p => p.IdPrestamo == id);

            if (pRESTAMO == null)
            {
                return HttpNotFound();
            }
            return View(pRESTAMO);
        }

        // GET: PRESTAMOes/Create
        public ActionResult Create()
        {
            ViewBag.IdLibro = new SelectList(db.LIBROes.Where(l => l.Estado == "Libre" && l.StockActual > 0), "IdLibro", "Titulo");
            ViewBag.DiasMaximos = Enumerable.Range(1, 14).Select(x => new SelectListItem { Value = x.ToString(), Text = x + " días" }).ToList();
            return View();
        }

        // POST: PRESTAMOes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PRESTAMO prestamo, int diasPrestamo)
        {
            var persona = Session["Usuario"] as Persona;
            if (persona == null)
                return RedirectToAction("Login", "Cuenta");

            if (ModelState.IsValid && diasPrestamo <= 14)
            {
                prestamo.IdPersona = persona.IdPersona;
                prestamo.FechaCreacion = DateTime.Now;
                prestamo.Estado = true;
                prestamo.EstadoEntregado = "Pendiente";
                prestamo.EstadoRecibido = "Pendiente";
                prestamo.IdEstadoPrestamo = 1;

                prestamo.FechaDevolucion = DateTime.Today.AddDays(diasPrestamo).AddHours(23).AddMinutes(59);

                db.PRESTAMOes.Add(prestamo);

                var libro = db.LIBROes.Find(prestamo.IdLibro);
                if (libro != null)
                {
                    libro.StockActual -= 1;
                    if (libro.StockActual <= 0)
                        libro.Estado = "Prestado";
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Préstamo solicitado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.IdLibro = new SelectList(db.LIBROes.Where(l => l.Estado == "Libre" && l.StockActual > 0), "IdLibro", "Titulo", prestamo.IdLibro);
            ViewBag.DiasMaximos = Enumerable.Range(1, 14).Select(x => new SelectListItem { Value = x.ToString(), Text = x + " días" }).ToList();
            TempData["ErrorMessage"] = "Ocurrió un error. Verifica los campos.";
            return View(prestamo);
        }


        // POST: ConfirmarEntrega (solo admin)
        [HttpPost]
        public ActionResult ConfirmarEntrega(int id)
        {
            var prestamo = db.PRESTAMOes.Find(id);
            if (prestamo == null) return HttpNotFound();

            prestamo.FechaEntregado = DateTime.Now;
            prestamo.EstadoEntregado = "Entregado";
            prestamo.IdEstadoPrestamo = 2; // En préstamo


            if (prestamo.FechaDevolucion.HasValue)
            {
                var fecha = prestamo.FechaDevolucion.Value.Date;
                prestamo.FechaDevolucion = fecha.AddHours(23).AddMinutes(59);
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Entrega confirmada.";
            return RedirectToAction("Admin");
        }

        // POST: ConfirmarDevolucion (solo admin)
        [HttpPost]
        public ActionResult ConfirmarDevolucion(int id)
        {
            var prestamo = db.PRESTAMOes.Find(id);
            if (prestamo == null) return HttpNotFound();

            prestamo.EstadoRecibido = "Recibido";
            prestamo.IdEstadoPrestamo = 3; // Devuelto
            prestamo.FechaConfirmacionDevolucion = DateTime.Now;

            var libro = db.LIBROes.Find(prestamo.IdLibro);
            if (libro != null)
            {
                libro.StockActual += 1;
                libro.Estado = "Libre";
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Devolución confirmada.";
            return RedirectToAction("Admin");
        }

        // POST: Renovar préstamo (solo admin)
        [HttpPost]
        public ActionResult Renovar(int id, int dias)
        {
            if (dias > 14) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var prestamo = db.PRESTAMOes.Find(id);
            if (prestamo == null || prestamo.IdEstadoPrestamo != 2)
                return HttpNotFound();

            if (prestamo.FechaDevolucion.HasValue)
            {
                var nuevaFecha = prestamo.FechaDevolucion.Value.AddDays(dias).Date;
                prestamo.FechaDevolucion = nuevaFecha.AddHours(23).AddMinutes(59);
            }

            db.SaveChanges();

            TempData["SuccessMessage"] = "Préstamo renovado correctamente.";
            return RedirectToAction("Admin");
        }

        // GET: Editar solo si está pendiente
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var prestamo = db.PRESTAMOes.Find(id);
            if (prestamo == null || prestamo.IdEstadoPrestamo != 1) return HttpNotFound();

            ViewBag.IdLibro = new SelectList(db.LIBROes, "IdLibro", "Titulo", prestamo.IdLibro);
            return View(prestamo);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PRESTAMO prestamo)
        {
            var prestamoExistente = db.PRESTAMOes.Find(prestamo.IdPrestamo);
            if (prestamoExistente == null || prestamoExistente.IdEstadoPrestamo != 1)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                // Solo actualizamos lo permitido
                prestamoExistente.IdLibro = prestamo.IdLibro;
                prestamoExistente.FechaDevolucion = prestamo.FechaDevolucion?.Date.AddHours(23).AddMinutes(59);

                db.SaveChanges();
                TempData["SuccessMessage"] = "Préstamo editado correctamente.";
                return RedirectToAction("Admin");
            }

            ViewBag.IdLibro = new SelectList(db.LIBROes, "IdLibro", "Titulo", prestamo.IdLibro);
            return View(prestamo);
        }

        // POST: Cancelar préstamo (solo si pendiente)
        [HttpPost]
        public ActionResult Cancelar(int id)
        {
            var prestamo = db.PRESTAMOes.Find(id);
            if (prestamo == null || prestamo.IdEstadoPrestamo != 1) return HttpNotFound();

            prestamo.Estado = false;
            prestamo.IdEstadoPrestamo = 5;
            // Devolver libro al stock
            var libro = db.LIBROes.Find(prestamo.IdLibro);
            if (libro != null)
            {
                libro.StockActual += 1;
                libro.Estado = "Libre";
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Préstamo cancelado.";
            return RedirectToAction("Admin");
        }
        public ActionResult Ticket(int id)
        {
            var prestamo = db.PRESTAMOes
                .Include(p => p.Persona)
                .Include(p => p.LIBRO)
                .FirstOrDefault(p => p.IdPrestamo == id);

            if (prestamo == null || prestamo.IdEstadoPrestamo != 1) // Solo si está pendiente
            {
                TempData["ErrorMessage"] = "No se puede generar ticket para este préstamo.";
                return RedirectToAction("Index");
            }

            return View(prestamo);
        }

        private void VerificarYAplicarPenalizaciones()
        {
            var hoy = DateTime.Now;

            var prestamosRetrasados = db.PRESTAMOes
                .Include(p => p.Persona)
                .Where(p => p.IdEstadoPrestamo == 2 &&  // En préstamo
                            p.FechaDevolucion < hoy &&
                            p.Estado == true)
                .ToList();

            foreach (var prestamo in prestamosRetrasados)
            {
                if (prestamo.FechaDevolucion == null)
                    continue;

                bool yaPenalizado = db.PENALIZACIONs.Any(x => x.IdPrestamo == prestamo.IdPrestamo && x.Estado == true);
                if (yaPenalizado)
                    continue;

                int diasRetrasados = (hoy.Date - prestamo.FechaDevolucion.Value.Date).Days;

                if (diasRetrasados <= 0)
                    continue;

                decimal monto = diasRetrasados * 5.00m;

                var penalizacion = new PENALIZACION
                {
                    IdPrestamo = prestamo.IdPrestamo,
                    FechaRegistro = DateTime.Now,
                    Estado = true,
                    Motivo = $"Entrega tardía de libro ({diasRetrasados} día(s) de retraso)",
                    Monto = monto
                };
                db.PENALIZACIONs.Add(penalizacion);

                prestamo.IdEstadoPrestamo = 4;
            }


            db.SaveChanges();
        }

        public ActionResult Penalizacion()
        {
            var persona = Session["Usuario"] as Persona;
            if (persona == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var penalizaciones = db.PENALIZACIONs
                .Include(p => p.PRESTAMO.LIBRO)
                .Where(p => p.PRESTAMO.Persona.IdPersona == persona.IdPersona && p.Estado == true)
                .OrderByDescending(p => p.FechaRegistro)
                .ToList();

            return View(penalizaciones);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarPago(int idPenalizacion, int idPrestamo)
        {
            var prestamo = db.PRESTAMOes.Find(idPrestamo);
            var penalizacion = db.PENALIZACIONs.Find(idPenalizacion);

            if (prestamo != null && penalizacion != null && penalizacion.Estado == true)
            {
                // Marcar la penalización como pagada
                penalizacion.Pagado = true;

                // Cambiar el estado del préstamo a 6 (Pagado)
                prestamo.IdEstadoPrestamo = 6;

                db.SaveChanges();
            }

            return RedirectToAction("Admin");
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