using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RegIN.Classes
{
    public class SendMail
    {
        public static void SendMessage(string Message, string To)
        {
            var smtpClient = new SmtpClient("smtp.yandex.ru")
            {
                Port = 587,
                Credentials = new NetworkCredential("gaschevdanil@yandex.ru", "umvsjkokwnwohksi"),
                EnableSsl = true,
            };
            smtpClient.Send("landaxer@yandex.ru", To, "Проект RegIn", Message);
        }
    }
}
