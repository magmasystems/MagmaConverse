using System;
using log4net;

namespace MagmaConverse.Data.Workflow
{
    public class SBSFormSubmissionWorkflowProcessor: IDisposable
    {
        internal ILog Logger = LogManager.GetLogger(typeof(SBSFormSubmissionWorkflowProcessor));

        public ISBSForm Form { get; set; }
        public Func<string, IFormSubmissionFunction, object> BodyExpressionEvaluator { get; set; }

        public SBSFormSubmissionWorkflowProcessor(ISBSForm form, Func<string, IFormSubmissionFunction, object> bodyExpressionEvaluator = null)
        {
            this.Form = form;
            this.BodyExpressionEvaluator = bodyExpressionEvaluator;
        }

        public virtual void Dispose()
        {
        }

        public object RunWorkflow(IFormSubmissionFunction submissionFunc)
        {
            if (submissionFunc == null)
                return null;

            if (submissionFunc.Cancel)
                return null;

            if (submissionFunc.Workflow == null)
                return null;

            var processor = this.LoadWorkflow(submissionFunc);
            return submissionFunc.Async ? processor?.ExecuteAsync() : processor?.Execute();
        }

        protected IWorkflow LoadWorkflow(IFormSubmissionFunction submissionFunc)
        {
            const string protocolTag = "://";
            string workflowName = submissionFunc.Workflow;
            if (workflowName.Contains(protocolTag))
            {
                workflowName = workflowName.Substring(0, workflowName.IndexOf(protocolTag, StringComparison.Ordinal));
                // "workflow://name" is just an alias for "name"
                if (workflowName.Equals("workflow", StringComparison.OrdinalIgnoreCase))
                {
                    workflowName = submissionFunc.Workflow.Substring("workflow://".Length);
                }
            }

            var workflowType = WorkflowRepository.Instance.GetType(workflowName);
            if (workflowType == null)
            {
                throw new ApplicationException($"Cannot find the workflow named {workflowName}");
            }

            if (typeof(IFormWorkflow).IsAssignableFrom(workflowType))
            {
                if (Activator.CreateInstance(workflowType, this, submissionFunc) is IFormWorkflow processor)
                    return processor;
            }
            else
            {
                if (Activator.CreateInstance(workflowType, this, submissionFunc) is IWorkflow processor)
                    return processor;
            }

            return null;
        }
    }
}
