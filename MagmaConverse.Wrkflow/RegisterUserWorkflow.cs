using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using MagmaConverse.Data;
using MagmaConverse.Data.Workflow;
using MagmaConverse.Models;

namespace MagmaConverse.Workflow
{
    /*
      "submissionFunctions": [
        {
            "workflow": "workflow://registerUser",
            "async": false,
            "timeout": 60,
            "failOnFalse": true
            "properties":
            {
            }
        }]
     */
    [SBSWorkflow("RegisterUser")]
    public class RegisterUserWorkflow : FormWorkflowBase
    {
        public RegisterUserWorkflow(Data.Workflow.SBSFormSubmissionWorkflowProcessor processor, IFormSubmissionFunction submissionFunc) : base("registerUser", processor, submissionFunc)
        {
        }

        public override object Execute()
        {
            (string to, string subject, string body, string activationCode) = this.CreateEmail();
            if (!this.SendMail(to, subject, body))
            {
                return false;
            }

            // If there is a "mock" property that is true, then don't wait for the response from the user
            if (this.SubmissionFunction.Properties.Get("mock", false))
            {
                return true;
            }

            // Get the timeout for the user to respond to the activation email.
            // The default is five minutes to let the user respond to the registration email.
            // We can also get values from the submissionFunction Json or from the workflow's configuration entry.
            const int defaultTimeout = 300;  
            int timeoutSeconds = defaultTimeout;
            if (this.SubmissionFunction.Timeout > 0)
                timeoutSeconds = this.SubmissionFunction.Timeout;
            else if (this.GetWorkflowConfigurationProperty<int>("timeout") > 0)
                timeoutSeconds = this.GetWorkflowConfigurationProperty<int>("timeout");

            // Wait for the user to respond. If the user clicks on the activation link, then the FormModel will fire an event.
            ManualResetEvent eventWait = new ManualResetEvent(false);
            SBSFormModel.Instance.UserActivated += (formId, code) =>
            {
                if (code == activationCode && formId == this.Form.Id)
                {
                    eventWait.Set();
                }
            };
            var completed = eventWait.WaitOne(timeoutSeconds * 1000);

            return completed;
        }

        private bool SendMail(string to, string subject, string body)
        {
            const string from = "marc@ctoasaservice.org";
            MailMessage mail = new MailMessage(from, to)
            {
                IsBodyHtml = true,
                Body = body,
                Subject = subject,
            };

            // Create the SMTP client
            SmtpClient client = new SmtpClient
            {
                Host = this.GetWorkflowConfigurationProperty("smtpHost"),
                Port = this.GetWorkflowConfigurationProperty("smtpPort", 25),
                EnableSsl = this.GetWorkflowConfigurationProperty("smtpUseSSL", true),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                Credentials = null,
                Timeout = 5
            };

            string sCreds = this.GetWorkflowConfigurationProperty("smtpCredentials");
            if (!string.IsNullOrEmpty(sCreds))
            {
                client.UseDefaultCredentials = false;
                var parts = sCreds.Split(',');
                client.Credentials = new NetworkCredential(parts[0], parts[1]);
            }

            // Send the mail in a new thread, cause it may take some time.
            // We can implement some improvements:
            //  1) Instead of having a single endpoint, take a list of endpoints.
            //  2) Batch the mail messages and send the batch on a timer.

            try
            {
                client.Send(mail);
                return true;
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                return false;
            }
            finally
            {
                client.Dispose();
            }
        }

        private (string to, string subject, string body, string activationCode) CreateEmail()
        {
            string to = "magmasystems@gmail.com";
            if (this.WorkflowProcessor.Form["email"] != null)
            {
                to = this.WorkflowProcessor.Form["email"].Value as string;
            }
            else if (this.SubmissionFunction.Properties != null && this.SubmissionFunction.Properties.TryGetValue("to", out object objTo))
            {
                to = (string)objTo;
            }
            else
            {
                var toProp = this.GetWorkflowConfigurationProperty("to");
                if (toProp != null)
                    to = toProp;
            }
            to = new StringSubstitutor().PerformSubstitutions(to, null, this.Form);

            string activationCode = Guid.NewGuid().ToString().Replace("-", "");
            const string subject = "Payroll Activation Required";
            string link = $"http://localhost:8089/FormManagerService/user/activate/{this.Form.Id}/{activationCode}";
            string body = $"<p>Click on <a href='{link}'>this link</a> to activate your registration</p>";

            return (to, subject, body, activationCode);
        }
    }
}
