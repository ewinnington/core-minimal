using Microsoft.CoreWf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace eric.coreminimal.wf
{
    public interface IEmailSetting
    {
        string GetHost();
        string GetUsername();
        string GetPassword();
        string GetAddress();
    }

    public interface IWfInfoExtension
    {
        string WorkflowName { get; set; }
        long JobId { get; set; }
        Guid WFIdentifier { get; set; }
    }

    public class EmailSetting : IEmailSetting
    {
        public string GetAddress()
        {
            return "admin@example.com";
        }

        public string GetHost()
        {
            return "mail.example.com";
        }

        public string GetPassword()
        {
            return "password";
        }

        public string GetUsername()
        {
            return "username";
        }
    }

    public class FakeSendEmail : CodeActivity
    {
        public InArgument<string> To { get; set; }
        public InArgument<string> Subject { get; set; }
        public InArgument<string> Body { get; set; }
        public InArgument<bool> UseSSL { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            bool useSSL = context.GetValue(UseSSL);

            string[] RecieverList = To.Get(context).Split(';');
            /*var mailMessage = new System.Net.Mail.MailMessage();
            foreach (string address in RecieverList)
            {
                if (address.Trim() != "") mailMessage.To.Add(address.Trim());
            }
            //mailMessage.To.Add(To.Get(context));
            mailMessage.Subject = Subject.Get(context);
            mailMessage.Body = Body.Get(context);*/

            //Get email setting from extension
            IEmailSetting settings = context.GetExtension<IEmailSetting>();

            Console.WriteLine(settings.GetHost() + " " + settings.GetAddress() + " " + settings.GetUsername() + " " + settings.GetPassword()); 

            /*
            mailMessage.From = new System.Net.Mail.MailAddress(settings.GetAddress());
            var smtp = new System.Net.Mail.SmtpClient();
            smtp.Host = settings.GetHost();
            smtp.Credentials = new System.Net.NetworkCredential(settings.GetUsername(), settings.GetPassword());
            smtp.EnableSsl = useSSL;
            smtp.Send(mailMessage);*/
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            var runtimeArguments = new Collection<RuntimeArgument>();

            RuntimeArgument rA1 = new RuntimeArgument("To", typeof(string), ArgumentDirection.In, true);
            RuntimeArgument rA2 = new RuntimeArgument("Subject", typeof(string), ArgumentDirection.In, true);
            RuntimeArgument rA3 = new RuntimeArgument("Body", typeof(string), ArgumentDirection.In, true);
            RuntimeArgument rA4 = new RuntimeArgument("UseSSL", typeof(bool), ArgumentDirection.In, true);
            runtimeArguments.Add(rA1);
            runtimeArguments.Add(rA2);
            runtimeArguments.Add(rA3);
            runtimeArguments.Add(rA4);

            metadata.Bind(this.To, rA1);
            metadata.Bind(this.Subject, rA2);
            metadata.Bind(this.Body, rA3);
            metadata.Bind(this.UseSSL, rA4);

            metadata.SetArgumentsCollection(runtimeArguments);
        }
    }

}
