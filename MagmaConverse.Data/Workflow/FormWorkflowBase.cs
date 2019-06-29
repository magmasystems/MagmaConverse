using System;
using System.Threading;
using System.Threading.Tasks;
using MagmaConverse.Configuration;
using Magmasystems.Framework;
using log4net;

namespace MagmaConverse.Data.Workflow
{
    public abstract class FormWorkflowBase : IFormWorkflow
    {
        #region Events
        public event Action WorkflowAborted = () => { };
        #endregion

        #region Variables
        public string Name { get; set; }
        public SBSFormSubmissionWorkflowProcessor WorkflowProcessor { get; }
        public ISBSForm Form { get; }
        public IFormSubmissionFunction SubmissionFunction { get; }

        public Task WorkflowTask { get; protected set; }
        public CancellationTokenSource CancellationTokenSource { get; protected set; }
        public bool IsAborted { get; protected set; }

        protected ILog Logger { get; }
        #endregion

        #region Constructors
        protected FormWorkflowBase(string name, SBSFormSubmissionWorkflowProcessor processor, IFormSubmissionFunction submissionFunc)
        {
            this.Name = name;
            this.WorkflowProcessor = processor;
            this.Form = processor.Form;
            this.SubmissionFunction = submissionFunc;

            this.Logger = LogManager.GetLogger(this.GetType());
        }
        #endregion

        #region Cleanup
        public virtual void Dispose()
        {   
        }
        #endregion

        #region Methods
        public abstract object Execute();

        public virtual object ExecuteAsync()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            return Task<object>.Factory.StartNew(() =>
            {
                try
                {
                    return this.Execute();
                }
                catch (Exception exc)
                {
                    this.Logger.Error(exc.Message);
                    return null;
                }
            }, this.CancellationTokenSource.Token);
        }

        public virtual void Abort()
        {
            if (this.WorkflowTask != null)
            {
                this.CancellationTokenSource?.Cancel();
            }

            this.IsAborted = true;
            this.WorkflowAborted();
        }

        protected MagmaConverseConfiguration.WorkflowConfiguration GetWorkflowConfiguration()
        {
            return ApplicationContext.Configuration.Workflows?.Get(this.Name);
        }

        protected string GetWorkflowConfigurationProperty(string name)
        {
            var config = this.GetWorkflowConfiguration();
            return config?.Props.Get(name) != null ? config.Props.Get(name).Value : null;
        }

        protected T GetWorkflowConfigurationProperty<T>(string name, T defaultValue = default)
        {
            string s = this.GetWorkflowConfigurationProperty(name);
            if (string.IsNullOrEmpty(s))
                return defaultValue;
            return (T) Convert.ChangeType(s, typeof(T));
        }
        #endregion
    }
}
