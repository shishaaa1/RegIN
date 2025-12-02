using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RegIN.Classes
{
    public class SendMail
    {
        public static bool SendMessage(string message, string to, string subject = "Проект RegIn")
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.yandex.ru")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("gaschevdanil@yandex.ru", "fodhwxmjrpxnwakx"),
                    EnableSsl = true,
                };

                smtpClient.Send("gaschevdanil@yandex.ru", to, subject, message);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки письма: {ex.Message}");
                return false;
            }
        }
    }
}
