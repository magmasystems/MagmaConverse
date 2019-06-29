using Magmasystems.Framework;

namespace MagmaConverse.Data
{
    public interface IFormSubmissionFunction
    {
        string Workflow { get; set; }
        bool Async { get; set; }
        int Timeout { get; set; }
        bool FailOnFalse { get; set; }
        bool Cancel { get; set; }
        Properties Properties { get; set; }
    }
}