using System;
using Magmasystems.Framework.Core;

namespace MagmaConverse.Data.Workflow
{
    public interface IWorkflow : IHasName, IDisposable
    {
        event Action WorkflowAborted;

        object Execute();
        object ExecuteAsync();
        void Abort();
    }
}
