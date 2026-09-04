using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiblioPlus.Models
{
	public class DestacadosDelMesViewModel
	{
        public LIBRO Libro { get; set; }
        public Persona Persona { get; set; }
        public int TotalPrestamosPersona { get; set; }
        public int TotalPrestamosLibros { get; set; }

    }
}