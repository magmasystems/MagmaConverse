namespace Magmasystems.Framework.Core
{
    public interface IHasId
    {
        /// <summary>
        /// The unique id of an instance of a class.
        /// This should really be set in the model
        /// </summary>
        string Id { get; }
    }
}
