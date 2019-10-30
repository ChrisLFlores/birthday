using Birthday.DataAccess.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Birthday
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger _logger;
        private readonly MailDetails _mailDetails;
        private Timer _timer;

        public TimedHostedService(IServiceScopeFactory scopeFactory, ILogger<TimedHostedService> logger, IOptions<MailDetails> mailDetailing)
        {
            this.scopeFactory = scopeFactory;
            _logger = logger;
            _mailDetails = mailDetailing.Value;
        }

        

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");
            var tomorrow = DateTime.Today.AddDays(1);
            using (var scope = scopeFactory.CreateScope())
            {
                
                var dbContext = scope.ServiceProvider.GetRequiredService<BirthdayContext>();
                var MonthDay = new int[] { tomorrow.Month, tomorrow.Day };
                var Bday = await PullAsync(dbContext, MonthDay);
                if (Bday.Any())
                {
                    SendMail(Bday, _mailDetails);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task <IEnumerable<Employees>> PullAsync(BirthdayContext dbContext, int[] monthDay)
        {
            try
            {
                var Bday = dbContext.Employees.Where(x => x.Birthday.Value.Month == monthDay[0] && x.Birthday.Value.Day == monthDay[1]);
                return Bday;
            }
            catch (Exception)
            {

                _logger.LogInformation("Pulling Employees, whose birthday is tomorrow, failed.");
                return null;
            }
        }

        private void SendMail(IEnumerable<Employees> bday, MailDetails mailDetails)
        {
            try
            {
                MailMessage msg = new MailMessage();

                msg.From = new MailAddress(mailDetails.User, "Chris");
                msg.To.Add(new MailAddress("Christopher.Flores@wmg.com", "Chris"));
                msg.Subject = "Birthday Reminder";

                var toBday = "Tomorrow's Birthday People: ";
                foreach (var person in bday)
                {
                    toBday = toBday + "\n" + person.FirstName + " " + person.LastName;
                }
                msg.Body = toBday;

                var SenderEmail = mailDetails.User;
                var SenderEmailPassword = mailDetails.Pass;

                StringBuilder str = new StringBuilder();
                str.AppendLine("BEGIN:VCALENDAR");
                str.AppendLine("PRODID:-//Schedule a Meeting");
                str.AppendLine("VERSION:2.0");
                str.AppendLine("METHOD:REQUEST");
                str.AppendLine("BEGIN:VEVENT");
                str.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmss}", DateTime.Today.AddDays(1).ToUniversalTime() - new TimeSpan(0, 0, 1)));
                str.AppendLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmss}", DateTime.Today.ToUniversalTime()));
                str.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmss}", DateTime.Today.AddDays(2).ToUniversalTime()));
                str.AppendLine("LOCATION: " + "Burbank, CA");
                str.AppendLine(string.Format("UID:{0}", Guid.NewGuid()));
                str.AppendLine(string.Format("DESCRIPTION:{0}", msg.Body));
                str.AppendLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", msg.Body));
                str.AppendLine(string.Format("SUMMARY:{0}", msg.Subject));
                str.AppendLine(string.Format("ORGANIZER:MAILTO:{0}", msg.From.Address));

                str.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", msg.To[0].DisplayName, msg.To[0].Address));

                str.AppendLine("BEGIN:VALARM");
                str.AppendLine("TRIGGER:-PT15M");
                str.AppendLine("ACTION:DISPLAY");
                str.AppendLine("DESCRIPTION:Reminder");
                str.AppendLine("END:VALARM");
                str.AppendLine("END:VEVENT");
                str.AppendLine("END:VCALENDAR");

                System.Net.Mail.SmtpClient smtpclient = new System.Net.Mail.SmtpClient();
                smtpclient.Host = mailDetails.Host; //-------this has to given the Mailserver IP
                smtpclient.Port = mailDetails.Port;
                smtpclient.EnableSsl = true;

                smtpclient.Credentials = new System.Net.NetworkCredential(SenderEmail.Trim(), SenderEmailPassword.Trim());
                System.Net.Mime.ContentType contype = new System.Net.Mime.ContentType("text/calendar");
                contype.Parameters.Add("method", "REQUEST");
                contype.Parameters.Add("name", "Meeting.ics");
                AlternateView avCal = AlternateView.CreateAlternateViewFromString(str.ToString(), contype);
                msg.AlternateViews.Add(avCal);
                smtpclient.Send(msg);
            }
            catch (Exception)
            {

                _logger.LogInformation("Send Email Failed.");
            } 
        }
    }
}