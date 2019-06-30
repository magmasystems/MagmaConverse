using System;
using MagmaConverse.Data;
using log4net;

namespace MagmaConverse.Workflow
{
    internal class SBSFormSubmissionWorkflowProcessor: IDisposable
    {
        internal ILog Logger = LogManager.GetLogger(typeof(SBSFormSubmissionWorkflowProcessor));

        public ISBSForm Form { get; set; }
        public Func<string, IFormSubmissionFunction, object> BodyExpressionEvaluator { get; set; }

        public SBSFormSubmissionWorkflowProcessor(ISBSForm form)
        {
            this.Form = form;
        }

        public void Dispose()
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

            if (submissionFunc.Workflow.Contains("rest://"))
            {
                var processor = new RestWorkflowProcessor(this, submissionFunc);
                return processor.Execute();
            }

            return null;
        }

    }
}
