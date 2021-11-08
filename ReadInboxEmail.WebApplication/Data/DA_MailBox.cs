using AE.Net.Mail;
using OpenPop.Common.Logging;
using ReadInboxEmail.WebApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using Quartz;

namespace ReadInboxEmail.WebApplication.Data
{
    public class DA_MailBox : IJob
    {


        //public void ejecutar()
        //{
        //    var a = new System.Timers.Timer(60000);
        //    a.Elapsed += new ElapsedEventHandler(RunThis);
        //    a.AutoReset = true;
        //    a.Enabled = true;
        //}

        //public void RunThis(object source, ElapsedEventArgs e)
        //{

        //    DashBoardMailBoxJob model = new DashBoardMailBoxJob();

        //    model = ReceiveMails();
        //    model.data = "";



        //    foreach (var item in model.Inbox)
        //    {
        //        if (DateTime.Now.Date == item.sendDate.Date)
        //        {
        //            ProcTable(item.UID, item.subject, item.sender, 0, item.sendDate, "");
        //        }

        //    }

        //}

        public void ProcTable(string cont, string subject, string sender, int nomessage, DateTime sendFecha, string cuerpo)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString))
            {
                try
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand("USP_SOLICITUD_INGREASR", cn);
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

                    var ex = e.Message;
                }



            }
        }


        public DashBoardMailBoxJob ReceiveMails()
        {
            DashBoardMailBoxJob model = new DashBoardMailBoxJob();
            model.Inbox = new List<MailMessege>();

            try
            {
                EmailConfiguration email = new EmailConfiguration();
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
                int start = msgcount - 1;
                // Note that you must specify that headersonly = false
                // when using GetMesssages().
                MailMessage[] mm = ic.GetMessages(start, end, false);
                foreach (var item in mm)
                {
                    MailMessege obj = new MailMessege();
                    try
                    {

                        obj.UID = item.Uid;
                        obj.subject = item.Subject;
                        obj.sender = item.From.ToString();
                        obj.sendDate = item.Date;
                       

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

        public void Execute(IJobExecutionContext context)
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
    }
}