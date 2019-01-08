using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadInboxEmail.WebApplication.Models
{
    public class DETALLESCOMPROBANTES
    {
        public int ID { get; set; }
        public string CODIGO_RELACIONADO { get; set; }
        public string Numeracion { get; set; }
        public string TIPODOCUMENTO { get; set; }
        public string NUMERODOCUMENTO { get; set; }
        public string Apellidos_nombres { get; set; }
        public string Numero_RUC { get; set; }
        public string Razon_social { get; set; }
        public string Fecha_cobro { get; set; }
        public string Fecha_emision { get; set; }
        public string Domicilio_fiscal { get; set; }
        public string Descripcion_producto { get; set; }
        public string Descripcion_subproducto { get; set; }
        public Decimal CAPITAL { get; set; }
        public Decimal INTERES { get; set; }
        public Decimal COMISION { get; set; }
        public Decimal OTROS { get; set; }
        public Decimal IMPORTEGRAVADO { get; set; }
        public string Tipo_moneda { get; set; }
    }
}