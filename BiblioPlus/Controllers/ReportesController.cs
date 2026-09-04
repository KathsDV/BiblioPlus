using System;
using System.Linq;
using System.Web.Mvc;
using BiblioPlus.Models; // Reemplaza con el espacio de nombres real

public class ReportesController : Controller
{
    private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

    public ActionResult LibrosMasPrestados()
    {
        return View();
    }

    [HttpPost]
    public ActionResult LoadLibrosMasPrestados()
    {
        var draw = Request.Form["draw"];
        var start = Convert.ToInt32(Request.Form["start"]);
        var length = Convert.ToInt32(Request.Form["length"]);
        var search = Request.Form["search[value]"];

        var query = db.PRESTAMOes
            .Where(p => p.IdEstadoPrestamo == 3 || p.IdEstadoPrestamo == 2 || p.IdEstadoPrestamo == 6) // 2 = Entregado
            .GroupBy(p => p.IdLibro)
            .Select(g => new {
                Titulo = g.FirstOrDefault().LIBRO.Titulo,
                Autor = g.FirstOrDefault().LIBRO.AUTOR.Descripcion,
                Categoria = g.FirstOrDefault().LIBRO.CATEGORIA.Descripcion,
                Editorial = g.FirstOrDefault().LIBRO.EDITORIAL.Descripcion,
                VecesPrestado = g.Count()
            });

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x => x.Titulo.Contains(search));
        }

        var total = query.Count();

        var data = query
            .OrderByDescending(x => x.VecesPrestado)
            .Skip(start)
            .Take(length)
            .ToList();

        return Json(new
        {
            draw,
            recordsTotal = total,
            recordsFiltered = total,
            data
        }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult DestacadosDelMes()
    {
        var fechaInicio = DateTime.Now.AddDays(-30);

        // Lector del Mes
        var lector = db.PRESTAMOes
            .Where(p => p.FechaEntregado >= fechaInicio)
            .GroupBy(p => p.Persona)
            .Select(g => new
            {
                Persona = g.Key,
                TotalPrestamos = g.Count()
            })
            .OrderByDescending(x => x.TotalPrestamos)
            .FirstOrDefault();

        // Libro del Mes
        var libro = db.PRESTAMOes
            .Where(p => p.FechaEntregado >= fechaInicio && p.LIBRO != null)
            .GroupBy(p => p.LIBRO)
            .Select(g => new
            {
                Libro = g.Key,
                TotalPrestamos = g.Count()
            })
            .OrderByDescending(x => x.TotalPrestamos)
            .FirstOrDefault();

        var model = new DestacadosDelMesViewModel
        {
            Persona = lector?.Persona,
            TotalPrestamosPersona = lector?.TotalPrestamos ?? 0,
            Libro = libro?.Libro,
            TotalPrestamosLibros = libro?.TotalPrestamos ?? 0
        };

        return View(model);
    }

    public ActionResult PrestamosRecientes()
    {
        var prestamos = db.vw_PrestamosRecientes.ToList();
        return View(prestamos);
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing) db.Dispose();
        base.Dispose(disposing);
    }
}