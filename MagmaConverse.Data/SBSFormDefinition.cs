using MagmaConverse.Framework.Core;
using MagmaConverse.Persistence.Interfaces;

namespace MagmaConverse.Data
{
    public class SBSFormDefinition : IPersistableDocumentObject, IHasId
    {
        #region Variables
        public string Id { get; private set; }
        public string id { get => this.Id; set => this.Id = value; }

        /// <summary>
        /// The actual definition of the form
        /// </summary>
        public FormTemplateFormDefinition Definition { get; set; }
        #endregion

        #region Constructors
        public SBSFormDefinition(FormTemplateFormDefinition def)
        {
            this.Id = IdGenerators.FormId("FormDefinition.");
            this.Definition = def;
        }
        #endregion
    }
}
