using AE.Net.Mail;
using OpenPop.Common.Logging;
using ReadInboxEmail.WebApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using IronPdf;

namespace ReadInboxEmail.WebApplication.Controllers
{
    public class MailBoxController : Controller
    {
        // GET: MailBox
        public ActionResult Index()
        {

          //  ejecutar();
            DashBoardMailBoxJob model = new DashBoardMailBoxJob();
            model = ReceiveMails();
            model.data = "";
            return View(model);
        }


        public void ejecutar()
        {
            DashBoardMailBoxJob model = new DashBoardMailBoxJob();
            model = ReceiveMails();
            model.data = "";



            foreach (var item in model.Inbox)
            {
                if (DateTime.Now.Date == item.sendDate.Date)
                {



                    ProcTable(item.UID, item.subject, item.sender, 0, item.sendDate, "");                  

                }
            }
        }

        public string ProcTable(string cont, string subject, string sender, int nomessage, DateTime sendFecha, string cuerpo)
        {
            var ex = "ok";

            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString))
            {
                try
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand("USP_SOLICITUD_INGREASR2", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UID", cont);
                    cmd.Parameters.AddWithValue("@subject", subject);
                    cmd.Parameters.AddWithValue("@sender", sender);
                    cmd.Parameters.AddWithValue("@MessegeNo", nomessage);
                    cmd.Parameters.AddWithValue("@sendDate", sendFecha);
                    cmd.Parameters.AddWithValue("@Body", cuerpo);

                    var a = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {

                     ex = e.Message;
                }
                return ex;


            }
        }


        public string RegistrarEnlace(string dni, string nombre, string fecemi, DateTime fecha, string contrato, string nombres, string tmoneda, string tdoc)
        {
            var ex = "ok";

            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString))
            {
                try
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand("USP_INSERTENLACE2", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@TIPOD", tdoc);
                    cmd.Parameters.AddWithValue("@DNI", dni);
                    cmd.Parameters.AddWithValue("@NOMBRE", nombre);
                    cmd.Parameters.AddWithValue("@FECEMI", fecemi);
                    cmd.Parameters.AddWithValue("@FEC", fecha);
                    cmd.Parameters.AddWithValue("@NOMBRESAPE", nombres);
                    cmd.Parameters.AddWithValue("@CONTRATO", contrato);
                    cmd.Parameters.AddWithValue("@TIPOMONEDA", tmoneda);

                    var a = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {

                    ex = e.Message;
                }
                return ex;


            }
        }



        public ActionResult imprimir(string dni, string num)
        {
           
            List<DETALLESCOMPROBANTES> data = new List<DETALLESCOMPROBANTES>();

            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString))
            {
                try
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT * FROM DETALLESCOMPROBANTES2018 WHERE NUMERODOCUMENTO = @DNI AND NUMERACION = @NUM", cn);
                    cmd.Parameters.AddWithValue("@DNI", dni);
                    cmd.Parameters.AddWithValue("@NUM", num);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                        DETALLESCOMPROBANTES x = new DETALLESCOMPROBANTES();

                        x.ID = reader.GetInt32(0);
                        x.CODIGO_RELACIONADO = reader.GetString(1);
                        x.Numeracion = reader.GetString(2);
                        x.TIPODOCUMENTO = reader.GetString(3);
                        x.NUMERODOCUMENTO = reader.GetString(4);
                        x.Apellidos_nombres = reader.GetString(5);
                        x.Numero_RUC = reader.GetString(6);
                        x.Razon_social = reader.GetString(7);
                        x.Fecha_cobro = reader.GetString(8);
                        x.Domicilio_fiscal = reader.GetString(9);
                        x.Descripcion_producto = reader.GetString(10);
                        x.Descripcion_subproducto = reader.GetString(11);
                        x.CAPITAL = reader.GetDecimal(12);
                        x.INTERES = reader.GetDecimal(13);
                        x.COMISION = reader.GetDecimal(14);
                        x.OTROS = reader.GetDecimal(15);
                        x.IMPORTEGRAVADO = reader.GetDecimal(16);
                        x.Tipo_moneda = reader.GetString(17);
                        x.Fecha_emision = reader.GetString(18);

                        data.Add(x);

                    }




                }
                catch (Exception e)
                {

                   var  ex = e.Message;
                }
                


            }


            return View(data);
        }


        public DashBoardMailBoxJob ReceiveMails()
        {

            
            DashBoardMailBoxJob model = new DashBoardMailBoxJob();
            model.Inbox = new List<MailMessege>();

            try
            {
                EmailConfiguration email = new EmailConfiguration ();
                email.POPServer = "imap.gmail.com"; // type your popserver
                email.POPUsername = "comprobanteshipotecario@gmail.com"; // type your username credential
                email.POPpassword = "Opplus00"; // type your password credential
                email.IncomingPort = "993";
                email.IsPOPssl = true;

                

                int success = 0;
                int fail = 0;
                ImapClient ic = new ImapClient(email.POPServer, email.POPUsername, email.POPpassword, AuthMethods.Login, Convert.ToInt32(email.IncomingPort), (bool)email.IsPOPssl);
                // Select a mailbox. Case-insensitive
                ic.SelectMailbox("INBOX");
                int i = 1;
                int msgcount = ic.GetMessageCount("INBOX");
                int end = msgcount - 1;
                int start = msgcount - 30;
                // Note that you must specify that headersonly = false
                // when using GetMesssages().
                MailMessage[] mm = ic.GetMessages(start, end, false);
                foreach (var item in mm)
                {
                    MailMessege obj = new MailMessege();
                    try
                    {
                        if (DateTime.Now.Date == item.Date.Date) //&& item.Subject.Trim()== "17430700"
                        {
                            obj.UID = item.Uid;
                            obj.subject = item.Subject;
                            obj.sender = item.From.ToString();
                            obj.sendDate = item.Date;
                            if (item.Attachments == null) { }
                            else obj.Attachments = item.Attachments;


                            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString))
                            {
                                try
                                {
                                    cn.Open();

                                    //var query = "SELECT * FROM RELACION2 WHERE NUMERODOCUMENTO = @NUM";

                                    SqlCommand cmd = new SqlCommand("SELECT * FROM ENLACE2018 WHERE DNI = @NUM", cn);
                                    cmd.Parameters.AddWithValue("@NUM", item.Subject);

                                    SqlDataReader reader = cmd.ExecuteReader();

                                    if (!reader.HasRows)
                                    {
                                        generarPDF(item.Subject);

                                    }

                                    var mendaje = ProcTable(item.Uid, item.Subject, item.From.ToString(), 0, item.Date, "");

                                        int a = 0;
                                        foreach (var item2 in item.Cc)
                                        {
                                            a++;
                                            var mendaje2 = ProcTable(item.Uid + a, item.Subject, item2.ToString(), 0, item.Date, "");

                                        }
                                           

                                }
                                catch (NullReferenceException e)
                                {

                                    var ex = e.Message;
                                }

                            }



                            if (item.Attachments == null) { }
                            else obj.Attachments = item.Attachments;

                    }




                        model.Inbox.Add(obj);
                        success++;
                    }
                    catch (Exception e)
                    {
                        DefaultLogger.Log.LogError(
                            "TestForm: Message fetching failed: " + e.Message + "\r\n" +
                            "Stack trace:\r\n" +
                            e.StackTrace);
                        fail++;
                    }
                    i++;

                }
                ic.Dispose();
                model.Inbox = model.Inbox.OrderByDescending(m => m.sendDate).ToList();
                model.mess = "Mail received!\nSuccesses: " + success + "\nFailed: " + fail + "\nMessage fetching done";

                if (fail > 0)
                {
                    model.mess = "Since some of the emails were not parsed correctly (exceptions were thrown)\r\n" +
                                    "please consider sending your log file to the developer for fixing.\r\n" +
                                    "If you are able to include any extra information, please do so.";
                }
            }

            catch (Exception e)
            {
                model.mess = "Error occurred retrieving mail. " + e.Message;
            }
            finally
            {

            }
            return model;
        }

        public ActionResult GetMessegeBody(string id)
        {
            JsonResult result = new JsonResult();

            EmailConfiguration email = new EmailConfiguration();
            email.POPServer = "imap.gmail.com";
            email.POPUsername = "fhguevara@gmail.com"; // type your username credential
            email.POPpassword = "Olvide99"; // type your password credential
            email.IncomingPort = "993";
            email.IsPOPssl = true;

            ImapClient ic = new ImapClient(email.POPServer, email.POPUsername, email.POPpassword, AuthMethods.Login, Convert.ToInt32(email.IncomingPort), (bool)email.IsPOPssl);
            // Select a mailbox. Case-insensitive
            ic.SelectMailbox("INBOX");

            int msgcount = ic.GetMessageCount("INBOX");
            MailMessage mm = ic.GetMessage(id, false);

            if (mm.Attachments.Count() > 0)
            {
                foreach (var att in mm.Attachments)
                {
                    string fName;
                    fName = att.Filename;
                }
            }
            StringBuilder builder = new StringBuilder();

            builder.Append(mm.Body);
            string sm = builder.ToString();

            CustomerEmailDetails model = new CustomerEmailDetails();
            model.UID = mm.Uid;
            model.subject = mm.Subject;
            model.sender = mm.From.ToString();
            model.sendDate = mm.Date;
            model.Body = sm;
            if (mm.Attachments == null) { }
            else model.Attachments = mm.Attachments;
             
            return View("CreateNewCustomer", model);
        }

        [HttpGet]
        public ActionResult CreateNewCustomer(CustomerEmailDetails model)
        {
             
                if (ModelState.ContainsKey("{key}"))
                    ModelState["{key}"].Errors.Clear();
                return View(model);
            
        }



        public void generarPDF(string dni)
        {

            List<RELACION2> data = new List<RELACION2>();

            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString))
            {
                try
                {
                    cn.Open();

                    //var query = "SELECT * FROM RELACION2 WHERE NUMERODOCUMENTO = @NUM";




                    SqlCommand cmd = new SqlCommand("SELECT * FROM RELACION2 WHERE numerodocumento = @NUM", cn);
                    cmd.Parameters.AddWithValue("@NUM", dni);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                        RELACION2 x = new RELACION2();

                        x.Numeracion = reader.GetString(0);
                        x.NUMERODOCUMENTO = reader.GetString(1);
                        x.Tipo_moneda = reader.GetString(2);
                        x.Apellidos_nombres = reader.GetString(3);
                        x.TIPODOCUMENTO = reader.GetString(4);

                        data.Add(x);

                    }

                }
                catch (Exception e)
                {

                    var ex = e.Message;
                }



            }



            foreach (var item2 in data)
            {
                string folderName = @"\\172.17.1.51";


                var uploads = System.IO.Path.Combine(folderName, "mp");



                var reporte2 = "Reporte" + item2.NUMERODOCUMENTO.Trim() + "-" + item2.Numeracion.Trim() + "-" + item2.Tipo_moneda.Trim() + ".pdf";


                IronPdf.HtmlToPdf Renderer = new IronPdf.HtmlToPdf();
                HtmlToPdf HtmlToPdf = new IronPdf.HtmlToPdf();
                HtmlToPdf.PrintOptions.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
                HtmlToPdf.PrintOptions.DPI = 400;
                Renderer.PrintOptions.MarginLeft = 15;
                Renderer.PrintOptions.MarginRight = 15;
                Renderer.PrintOptions.MarginTop = 5;
                Renderer.PrintOptions.MarginBottom = 5;
                Renderer.PrintOptions.CssMediaType = PdfPrintOptions.PdfCssMediaType.Screen;
                Renderer.PrintOptions.InputEncoding = Encoding.UTF8;

                Renderer.RenderUrlAsPdf("http://172.17.1.38:8041/MailBox/imprimir?dni=" + item2.NUMERODOCUMENTO.Trim() + "&num=" + item2.Numeracion.Trim()).SaveAs(Path.Combine(uploads, reporte2));

               // Renderer.RenderUrlAsPdf("http://localhost:50563/MailBox/imprimir?dni=" + item2.NUMERODOCUMENTO.Trim() + "&num=" + item2.Numeracion.Trim()).SaveAs(Path.Combine(uploads, reporte2));


     


                    //----------------------------


                    //  var reporte = "Reporte" + año + mes2 + "-" + obj.subject + "-" + item.CODIGO_RELACIONADO.Trim() + "-" + item.Tipo_moneda + ".pdf";
                    var res = RegistrarEnlace(dni, reporte2, "", DateTime.Now, item2.Numeracion.Trim(), item2.Apellidos_nombres.Trim(), item2.Tipo_moneda.Trim(), item2.TIPODOCUMENTO.Trim());



            }
        }


    }
}