using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using BiblioPlus.Models;

namespace ProyectoBiblioPlus.Controllers
{
    public class CuentaController : Controller
    {
        private DB_BIBLIOTECAEntities db = new DB_BIBLIOTECAEntities();

        // GET: Cuenta/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string correo, string clave)
        {
            var usuario = db.Personas.FirstOrDefault(u => u.Correo == correo && u.Clave == clave && u.Estado == 1);

            if (usuario != null)
            {
                Session["Usuario"] = usuario;

                Session["IdTipoPersona"] = usuario.IdTipoPersona;
                return RedirectToAction("Index", "Home");

            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        // GET: Cuenta/Registro
        public ActionResult Registro()
        {
            ViewBag.IdTipoPersona = new SelectList(db.TIPO_PERSONA, "IdTipoPersona", "Descripcion");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(Persona persona)
        {
            if (ModelState.IsValid)
            {
                persona.FechaCreacion = DateTime.Now;
                persona.Estado = 1;

                db.Personas.Add(persona);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            ViewBag.IdTipoPersona = new SelectList(db.TIPO_PERSONA, "IdTipoPersona", "Descripcion", persona.IdTipoPersona);
            return View(persona);
        }

        public ActionResult Logout()
        {
            Session["Usuario"] = null;
            return RedirectToAction("Login");
        }
        public ActionResult Conocenos()
        {
            return View();
        }
    }
}