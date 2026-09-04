using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BiblioPlus.Models;

namespace BiblioPlus.Controllers
{
    public class HomeController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        public ActionResult Index()
        {
            // Trae los libros con los datos relacionados (AUTOR, CATEGORIA, EDITORIAL)
            var libros = db.LIBROes
                .Include(l => l.AUTOR)
                .Include(l => l.CATEGORIA)
                .Include(l => l.EDITORIAL)
                .ToList();

            var sugerencias = db.LIBROes
                .Include(l => l.AUTOR)
                .OrderBy(x => System.Guid.NewGuid())
                .Take(5)
                .ToList();

            ViewBag.Sugerencias = sugerencias;

            return View(libros);
        }
    }
}