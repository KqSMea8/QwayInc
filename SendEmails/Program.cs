using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace SendEmails
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Start info
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            #endregion

            testEmails();

            #region End Info
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine(String.Format("\n{0}{1}{0}", new String('=', 5), DateTime.Now));
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            Console.ReadLine();
            #endregion
        }
        static void test()
        {
            //ExchangeService service = new ExchangeService();
            //service.AutodiscoverUrl("youremailaddress@yourdomain.com");

            //EmailMessage message = new EmailMessage(service);
            //message.Subject = subjectTextbox.Text;
            //message.Body = bodyTextbox.Text;
            //message.ToRecipients.Add(recipientTextbox.Text);
            //message.Save();

            //message.SendAndSaveCopy();

            //System.Windows.MessageBox.Show("Message sent!");

            //recipientTextbox.Text = "";
            //subjectTextbox.Text = "";
            //bodyTextbox.Text = "";
        }
        static void testEmails()
        {
            try
            {
                SmtpClient smtpClient = getSmtpClient();
                File.WriteAllText(Properties.Settings.Default.EmailLogFileName, String.Empty);
                using (StreamWriter logFile = System.IO.File.AppendText(Properties.Settings.Default.EmailLogFileName))
                {
                    foreach (String line in System.IO.File.ReadAllLines(Properties.Settings.Default.EmailListFileName))
                    {
                        Receiver receiver = new Receiver(line);
                        sendEmail(smtpClient, receiver);
                        try
                        {
                            logFile.WriteLine(receiver);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("LOG ERROR: {0}", ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOG ERROR: {0}", ex.Message);
            }
            Console.WriteLine("Sending completed.");
        }
        private static void sendEmail(SmtpClient smtpClient, Receiver receiver)
        {
            try
            {
                Console.WriteLine("[{0}] sending ...", receiver);
                MailMessage mailMessage = getMailMessage(receiver);
                //smtpClient.SendAsync(mailMessage, receiver);
                smtpClient.Send(mailMessage);
                //await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("[{0}] sent.", receiver);
            }
            catch (Exception ex)
            {
                receiver.UpdateException(ex);
                Console.WriteLine("ERROR: {0}", ex.Message);
            }

        }
        private static MailMessage getMailMessage(Receiver receiver)
        {
            String html = Properties.Resources.Email;
            html = html.Replace("{Company}", receiver.Company);
            html = html.Replace("{Receiver}", receiver.FirstName);
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(getLinkedResource(Properties.Resources.Logo, "Logo"));
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("qway.inc@hotmail.com");
            mailMessage.To.Add(receiver.Email);
            mailMessage.Subject = "Qway can promote your product to Canada Market";
            mailMessage.IsBodyHtml = true;
            mailMessage.AlternateViews.Add(alternateView);
            return mailMessage;
        }
        private static SmtpClient getSmtpClient()
        {

            SmtpClient smtpClient = new SmtpClient("smtp.live.com");
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential("qway.inc@hotmail.com", "Ah630615");
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.SendCompleted += SmtpClient_SendCompleted;
            return smtpClient;
        }

        private static void SmtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Receiver receiver = (Receiver)e.UserState;
            String message = "";
            if (e.Cancelled)
            {
                message = String.Format("[{0}] Send canceled.", receiver);
            }
            if (e.Error != null)
            {
                message = String.Format("[{0}] Send terminated.\n{1}", receiver, e.Error.ToString());
            }
            else
            {
                message = String.Format("[{0}] sent successfully.", receiver);
            }
            Console.WriteLine(message);
        }

        private static LinkedResource getLinkedResource(System.Drawing.Bitmap image, String contentId)
        {
            // Create a LinkedResource object for each embedded image
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            LinkedResource linkedResource = new LinkedResource(stream, MediaTypeNames.Image.Jpeg);
            stream.Position = 0;
            linkedResource.ContentId = contentId;
            return linkedResource;
        }
    }
    public class Receiver
    {
        public Int32 No { get; set; }
        public String Company { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String FullName { get; set; }
        public String Email { get; set; }
        public String Error { get; set; } = String.Empty;
        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4:yyyy.MM.dd HH:mm:ss.ttt},{5},{6}", this.No, this.Company, this.FullName, this.Email, DateTime.Now, String.IsNullOrEmpty(this.Error) ? "X" : "", this.Error);
        }
        public Receiver(String line)
        {
            String[] items = line.Split(',');
            this.No = Convert.ToInt32(items[0]);
            this.Company = items[1];
            this.FullName = items[2];
            this.Email = items[3];
            try
            {
                String[] names = this.FullName.Split(' ');
                this.FirstName = names[0];
                this.LastName = names[1];
            }
            catch (Exception ex)
            {
                this.FirstName = this.FullName;
            }
        }
        public void UpdateException(Exception e)
        {
            this.Error = e.Message;
        }
    }
}
