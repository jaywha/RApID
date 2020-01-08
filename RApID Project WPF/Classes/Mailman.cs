using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Net;

namespace RApID_Project_WPF
{
    public static class Mailman
    {
        private static csSQL.csSQLClass sqlClass = new csSQL.csSQLClass();
        private static readonly List<Exception> ReportedExceptions = new List<Exception>();

        /// <summary>
        /// Static class to contain Firebase Token Strings for push notifications
        /// </summary>
        public static class FireTokens
        {
            /// <summary> Software Engineer part of the Test Engineering team </summary>
            public static string Jay { get; set; } 
                = "fGaGwvh6EDE:APA91bG0mr3kNHuMItXz_C8DxZbBQFIgC7ADOOXLNEluAkwO9l-7Md-xYLWsiyy_4jiKyiGbwojjPhneL2Wf-AlpzJ5_IPhiQqwddaff11_Y5zTDtJ3WeO5h3kcBJ07sfj5xzKiJwOAE";
        }

        public static void AssignEmailRecipients(ref MailMessage _emailMessage)
        {
            //TODO: Pull from a DB mailing list?
            StringBuilder recipientList = new StringBuilder();
            recipientList.Append("jay.whaley@johnsonoutdoors.com,");
            recipientList.Append("dexter.glanton@johnsonoutdoors.com,");
            recipientList.Append("julie.shirley@johnsonoutdoors.com,");
            recipientList.Append("blayde.dill@johnsonoutdoors.com,");
            recipientList.Append("charles.harding@johnsonoutdoors.com");

            _emailMessage.To.Add(recipientList.ToString());
        }

        public static bool SendEmail(string subject = "", string body = "", Exception exception = null)
        {
            ReportedExceptions.Add(exception);

            string sUserName = Environment.UserName;
            //try
            //{
                MailMessage _emailMessage = new MailMessage();

                AssignEmailRecipients(ref _emailMessage);

                #region assign who email is from
                string emailQuery = "SELECT EmailAddress FROM UserLogin WHERE [Username] = '" + sUserName + "'";
                string email = sqlClass.SQLGet_String(emailQuery, csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString);

                if (string.IsNullOrEmpty(email))
                    throw new Exception("User (" + sUserName + ") does not have a valid email address or username in the UserLogin table.");

                _emailMessage.From = new MailAddress(email);
                #endregion

                _emailMessage.IsBodyHtml = true;
                if(exception != null) _emailMessage.Priority = MailPriority.High;

                _emailMessage.Subject = string.IsNullOrWhiteSpace(subject) ?
                    $"RApID Exception - {Environment.MachineName} under {Environment.UserName}"
                : subject;

#if DEBUG
                var initialPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
#else
                var initialPath = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main";
#endif

                _emailMessage.Body = string.IsNullOrWhiteSpace(body) ?
                    File.ReadAllText($@"{initialPath}\Resources\ExceptionEmailTemplate.html")
                    .Replace("[ExceptionType]",WebUtility.HtmlEncode(exception?.GetType().ToString() ?? ""))
                    .Replace("[ExceptionMessage]",WebUtility.HtmlEncode(exception?.Message.ToString() ?? ""))
                    .Replace("[ExceptionStackTrace]",WebUtility.HtmlEncode(exception?.StackTrace.ToString() ?? ""))
                : body;

                #region Get Smtp thing
                string eeptConnString = csObjectHolder.csObjectHolder.ObjectHolderInstance().EEPTConnectionString;
                string query = "SELECT Name FROM EmailServers";
                string smtpClient = sqlClass.SQLGet_String(query, eeptConnString);

                SmtpClient smtp = new SmtpClient(smtpClient); ;
                //System.Net.ServicePointManager.MaxServicePointIdleTime = 1;

                string sExMessage = "";
                int iTries = 0;

                while (true)
                {
                    try
                    {
                        smtp = new SmtpClient(smtpClient) { Timeout = 3000 };
                        smtp.Send(_emailMessage);
                        break;
                    }
                    catch (Exception ex)
                    {
                        sExMessage = ex.Message;
                    }
                    finally
                    {
                        smtp.Dispose();
                    }

                    iTries++;

                    if (iTries == 3)
                        throw new Exception(sExMessage);
                }
                #endregion
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Couldn't send error email...\nPlease contact Jay Whaley or Dexter Glanton.", 
            //        "Critical Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            //    csExceptionLogger.csExceptionLogger.Write("Mailman_Error", ex);
            //    return false;
            //}
            return true;
        }
    }
}
