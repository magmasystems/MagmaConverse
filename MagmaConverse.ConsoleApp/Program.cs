using System;
using System.Reflection;
using System.Threading;
using log4net;

using MagmaConverse.Data;
using Magmasystems.Framework;
using MagmaConverse.Services;
using log4net.Config;
using System.IO;
using Magmasystems.Framework.Serialization;
using MagmaConverse.Models;
using System.Collections.Generic;

namespace MagmaConverse.ConsoleApp
{
    class Program : IDisposable
	{
		private static ILog Logger;

		private FormManagerService SBSFormService { get; }
		private bool NoConsole { get; set; }
		private bool NoAutocreateRestService { get; set; }

		static void Main(string[] args)
		{
            Program.InitLogging();
            Logger = LogManager.GetLogger(typeof(Program));

            Program app = new Program(args);
			Logger.Info("Created the app");

			app.TestCreateFormRequestDeserialization();

			if (!app.NoConsole)
			{
                // If we did not specify -noconsole on the command line, then just run a REPL loop to test out conversations 
				ConsoleHelpers.CommandLoop(app, app.ProcessCommand);
			}
            else if (app.NoAutocreateRestService)
            {
                // If the -norest and -noconsole arguments were on the command line, then we don't create the REST service, 
                // and we just run a sample form.
                app.TestCreateAndRunForm();
            }
            else
			{
                // If -noconsole was on the command line, and if -norest was NOT on the command line, then assume that we are driving
                // through a Postman test suite. So we wait for Postman to end the form processing.
				ManualResetEvent eventWaitForRestServiceToEnd = new ManualResetEvent(false);
				app.SBSFormService.RunFormEnded += form => eventWaitForRestServiceToEnd.Set();
				Console.CancelKeyPress += (sender, eventArgs) => { eventWaitForRestServiceToEnd.Set(); };
				eventWaitForRestServiceToEnd.WaitOne();
                eventWaitForRestServiceToEnd.Dispose();
			}

            app.Dispose();
		}

		public Program(string[] args)
		{
            // -noconsole -norest -purgedb
            this.ProcessCommandLine(args);

			// We don't want to auto-create the REST Service if we are running unit tests that create the REST service on its own
			if (!NoAutocreateRestService)
			{
				this.SBSFormService = new FormManagerService();

				// See if we purge the database on startup
				try
				{
					if (ApplicationContext.Configuration.PurgeDatabaseOnStartup)
						this.SBSFormService.DeleteDatabase("FakeToken");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}

				this.SBSFormService.LoadData();


#if USE_APPCONFIG_FOR_WCF
                this.SBSFormManagerRestServiceHost = new WebServiceHost(this.SBSFormRestService);
                this.SBSFormManagerRestServiceHost.Open();

                this.SwaggerHost = new WebServiceHost(typeof(SwaggerWcfEndpoint));
                this.SwaggerHost.Open();
#endif
			}
		}

		public void Dispose()
		{
			Logger.Info("Disposing");
			this.SBSFormService?.Dispose();
		}

        internal static void InitLogging()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            var currAppDir = AppDomain.CurrentDomain.BaseDirectory;
            var logfile = new FileInfo(currAppDir + "log4net.config");
            XmlConfigurator.Configure(logRepository, logfile);
        }

		private void ProcessCommandLine(string[] args)
		{
			if (args == null || args.Length == 0)
				return;

			foreach (string arg in args)
			{
				string arg2 = arg.ToLower();
				if (arg2.StartsWith("-", StringComparison.CurrentCulture))
					arg2 = arg2.Substring(1);

				switch (arg2)
				{
					case "noconsole":
						this.NoConsole = true;
						break;

					// We don't want to auto-create the REST Service if we are running unit tests that create the REST service on its own
					case "norest":
						this.NoAutocreateRestService = true;
						break;
				}
			}
		}

		private ResponseStatus ProcessCommand(string command)
		{
			if (string.IsNullOrEmpty(command))
				return ResponseStatus.OK;

			string[] args = command.Split(' ');
			object rc;
			string errorMsg;

			switch (args[0].ToLower())
			{
				case "createform":
					if (args.Length < 2)
					{
						errorMsg = $"Not enough arguments for command: {args[0]}";
						Logger.Error(errorMsg);
						return new ResponseStatus(ResponseStatusCodes.Error, errorMsg);
					}
					rc = this.SBSFormService.CreateForm(Json.Deserialize<FormCreateRequest>(args[1]));
					break;


				case ConsoleHelpers.CTRL_C_COMMAND:
					return new ResponseStatus(-1);

				default:
					errorMsg = $"Unknown command: {args[0]}";
					Logger.Error(errorMsg);
					return new ResponseStatus(ResponseStatusCodes.Error, errorMsg);
			}

			return new ResponseStatus(ResponseStatusCodes.OK, rc);
		}

		public void TestCreateFormRequestDeserialization()
		{
			try
			{
                string fileName = "DIYOnboardingForm.json";

                string json = File.ReadAllText(fileName);
				var request = Json.Deserialize<FormCreateRequest>(json);
				if (request != null)
				{
                    Logger.Info($"We read the form definition from ${fileName} and deserialized it");
				}
			}
			catch (Exception exc)
			{
				Logger.Error(ExceptionHelpers.Format(exc));
			}
		}

        public void TestCreateAndRunForm()
        {
            FormCreateRequest request = new FormCreateRequest
            {
                // We build a simple form that consists on one checkbox
                Forms = new List<FormTemplateFormDefinition>
                {
                    new FormTemplateFormDefinition
                    {
                        Title = "Test Marc Form",
                        Name = "MarcForm",
                        Description = "This is a test form",
                        Fields = new List<FormTemplateFieldDefinition>
                        {
                            new FormTemplateFieldDefinition
                            {
                                FieldType = "checkbox",
                                Prompt = "Are you a small business?"
                            }
                        }
                    }
                }
            };

            Logger.Info("Running TestCreateAndRunForm()");
            this.CreateAndRunForm(request);
        }

        private void CreateAndRunForm(FormCreateRequest request)
		{
			using (FormManagerService service = new FormManagerService())
			{
				var response = service.CreateForm(request);
				Console.WriteLine(response);

				var newformResponse = service.NewForm(response.Value[0].Id);
				Console.WriteLine(newformResponse);

				var idFormInstance = newformResponse.Value;
				var formInstance = SBSFormModel.Instance.GetFormInstance(idFormInstance);

				ManualResetEvent eventStop = new ManualResetEvent(false);
				var runformResponse = service.RunForm(idFormInstance);
				Console.WriteLine(runformResponse);

				formInstance.Submitted += form => { eventStop.Set(); };
				formInstance.Cancelled += form => { eventStop.Set(); };
				service.RunFormEnded += form =>
				{
                    service.SaveForm(idFormInstance);
					if (form.Id == idFormInstance)
						eventStop.Set();
				};

				eventStop.WaitOne();
                eventStop.Dispose();
			}
		}
	}
}

