using System.Net.Mail;

namespace Manage.Data.Public
{
    //services.AddSingleton<ISendMail>(provider => new SendMail(clientCredentials,clientAddress,clientHost,clientName)); 
    public interface ISendMail
    {
        void Send(string[] emails, string subject, string body);
    }
    public class SendMail : ISendMail
    {
        private readonly string clientHost;
        private readonly string clientAddress;
        private readonly string clientName;
        private readonly string clientCredentials;
        public SendMail(string clientCredentials, string clientAddress, string clientHost, string clientName)
        {
            this.clientCredentials = clientCredentials;
            this.clientAddress = clientAddress;
            this.clientHost = clientHost;
            this.clientName = clientName;
        }
        public void Send(string[] emails, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(clientHost);
            mail.From = new MailAddress(clientAddress, clientName);

            foreach (string mailAddress in emails)
            {
                mail.To.Add(mailAddress);
            }
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;


            SmtpServer.Port = 587;

            SmtpServer.UseDefaultCredentials = false;

            SmtpServer.Credentials = new System.Net.NetworkCredential(clientAddress, clientCredentials);

            SmtpServer.EnableSsl = false;

            SmtpServer.Send(mail);
            SmtpServer.Dispose();
            mail.Dispose();

        }
    }
}
