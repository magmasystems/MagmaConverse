namespace MagmaConverse.Data.Workflow
{
    public interface IFormWorkflow : IWorkflow
    {
        ISBSForm Form { get; }
        IFormSubmissionFunction SubmissionFunction { get; }
    }
}