using System.Text.RegularExpressions;
using MagmaConverse.Interfaces;

namespace MagmaConverse.Data
{
    public interface IReferenceDataResolver
    {
        object Resolve(string reference, IHasLookup repo);
    }

    public class ReferenceDataResolver : IReferenceDataResolver
    {
        /// <summary>
        /// Given a expression like ${var:xxxx}, sees if the data structure named xxxx is in the reference data repository
        /// </summary>
        /// <param name="reference">The name of the reference data</param>
        /// <param name="repo">The repository to search through</param>
        /// <returns>The object that is in the repo</returns>
        public object Resolve(string reference, IHasLookup repo)
        {
            // The reference can be ${var:USStates}
            Regex regex = new Regex(@"^\${var:(?<varname>\w+)}$");
            var match = regex.Match(reference);

            if (match.Success)
            {
                string varname = match.Groups["varname"].Value;
                // Get the collection from the reference data repo
                return repo?.Get(varname);
            }

            return null;
        }
    }
}
