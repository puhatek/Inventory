using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inventory.Models;
using System.Text;
using System.Linq;
using System.Net.Mail;
using System.Web.Configuration;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class SendMessage_UnitTest
    {
        public MagazynDbContext db = new MagazynDbContext();

        [TestMethod]
        public async Task Send_UnitTest()
        {
            var model = new Message()
            {
                MessageText = "Testowa wiadomość",
                Recipient = "cheschire_kotek@hotmail.com",
                Subject = "Unit Test subject",
                SupplierId = 5
                
            };

            StringBuilder textWithoutOrderTable = new StringBuilder(db.Suppliers.Where(s => s.Id == model.SupplierId).Select(s => s.Message).FirstOrDefault().Replace(@"\n", @"<br/>"));

            string sendRespons = await SendMessage(model.Recipient, model.Subject, textWithoutOrderTable.Append(model.MessageText).ToString());


        }

        private async Task<string> SendMessage(string Recipient, string Subject, string MessageText)
        {
            string sendRespons = "Sent";

            MailMessage message = new MailMessage(
                from: "cheschire_kotek@hotmail.com",
                to: Recipient
                );
            message.Subject = Subject;
            message.Body = MessageText;
            message.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient(WebConfigurationManager.AppSettings["host"]);
            smtpClient.EnableSsl = true;

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                sendRespons = ex.Message;
            }
            return sendRespons;
        }
    }
}
