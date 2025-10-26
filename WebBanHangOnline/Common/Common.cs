using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace WebBanHangOnline.Common
{
    public class Common
    {
        private static string password = ConfigurationManager.AppSettings["PasswordEmail"];
        private static string Email = ConfigurationManager.AppSettings["Email"];
        public static bool SendMail(string name, string subject, string content,
            string toMail)
        {
        bool rs = false;
            try
            {
                MailMessage message = new MailMessage();
                var smtp = new SmtpClient();
                {
                    smtp.Host = "smtp.gmail.com"; //host name
                    smtp.Port = 587; //port number
                    smtp.EnableSsl = true; //whether your smtp server requires SSL
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential() { 
                        UserName=Email,
                        Password=password
                    };
                }
                MailAddress fromAddress = new MailAddress(Email, name);
                message.From = fromAddress;
                message.To.Add(toMail);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = content;
                smtp.Send(message);
                rs = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                rs = false;
            }
            return rs;
        }

        public static string FormatNumber(object value, int SoSauDauPhay = 0, bool useFixedDecimals = false)
        {
            if (value == null)
                return "0";

            decimal number;

            // Parse mềm dẻo: chấp nhận mọi kiểu số, kể cả string chứa dấu phẩy hoặc chấm
            if (value is decimal d)
                number = d;
            else if (value is double db)
                number = (decimal)db;
            else if (value is float f)
                number = (decimal)f;
            else if (decimal.TryParse(Convert.ToString(value), NumberStyles.Any, new CultureInfo("vi-VN"), out var parsed))
                number = parsed;
            else
                return "0";

            // pattern format
            string decimals = new string(useFixedDecimals ? '0' : '#', Math.Max(0, SoSauDauPhay));
            string pattern = SoSauDauPhay > 0 ? $"#,##0.{decimals}" : "#,##0";

            // format theo chuẩn Việt Nam
            return number.ToString(pattern, new CultureInfo("vi-VN"));
        }

        //public static string FormatNumber(object value, int SoSauDauPhay = 2)
        //{
        //    bool isNumber = IsNumeric(value);
        //    decimal GT = 0;
        //    if (isNumber)
        //    {
        //        GT = Convert.ToDecimal(value);
        //    }
        //    string str = "";
        //    string thapPhan = "";
        //    for (int i = 0; i < SoSauDauPhay; i++)
        //    {
        //        thapPhan += "#";
        //    }
        //    if (thapPhan.Length > 0) thapPhan = "." + thapPhan;
        //    string snumformat = string.Format("0:#,##0{0}", thapPhan);
        //    str = String.Format("{" + snumformat + "}", GT);

        //    return str;
        //}

        private static bool IsNumeric(object value)
        {
            return value is sbyte
                       || value is byte
                       || value is short
                       || value is ushort
                       || value is int
                       || value is uint
                       || value is long
                       || value is ulong
                       || value is float
                       || value is double
                       || value is decimal;
        }

        public static string HtmlRate(int rate)
        {
            rate = Math.Max(0, Math.Min(5, rate)); // đảm bảo 0..5

            var sb = new StringBuilder(5 * 55); // tối ưu nhẹ dung lượng dự kiến
            for (int i = 1; i <= 5; i++)
            {
                string icon = i <= rate ? "fa-star" : "fa-star-o";
                sb.Append("<li><i class='fa ").Append(icon)
                  .Append("' aria-hidden='true'></i></li>");
            }
            return sb.ToString();
        }
        //public static string HtmlRate(int rate)
        //{
        //    var str = "";
        //    if (rate == 1)
        //    {
        //        str = @"<li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>";
        //    }
        //    if (rate == 2)
        //    {
        //        str = @"<li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>";
        //    }
        //    if (rate == 3)
        //    {
        //        str = @"<li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>";
        //    }
        //    if (rate == 4)
        //    {
        //        str = @"<li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star-o' aria-hidden='true'></i></li>";
        //    }
        //    if (rate == 5)
        //    {
        //        str = @"<li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>
        //            <li><i class='fa fa-star' aria-hidden='true'></i></li>";
        //    }
        //    return str;
        //}
    }
}