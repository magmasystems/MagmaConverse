using System;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using MagmaConverse.Framework;

namespace MagmaConverse.Views.Console
{
    public class ConsoleRepeaterField : ConsoleNoInputOutputFieldBase
    {
        #region Variables
        /// <summary>
        /// This is the index within the list of fields that this repeater control is
        /// </summary>
        private int ThisRepeaterIndex { get; }

        /// <summary>
        /// Where we are in grid
        /// </summary>
        internal int IterationCount { get; set; }

        /// <summary>
        /// A reference to the ending field
        /// </summary>
        internal ISBSFormField EndingField { get; }

        /// <summary>
        /// The 0-based index of the ending field
        /// </summary>
        internal int EndingIndex { get; }

        /// <summary>
        /// True if the user chose to stop the loop
        /// </summary>
        internal bool IsEnded { get; set; }
        #endregion

        #region Constructor
        public ConsoleRepeaterField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
            if (!(formField is SBSRepeaterField sbsRepeaterField))
                return;

            this.ThisRepeaterIndex = this.FormView.Form.FindFieldIndex(this.SBSFormField.Name);

            var endingField = this.FindEndingField(sbsRepeaterField.EndingFieldName);
            this.EndingField = endingField ?? throw new ApplicationException($"In the repeating group {this.SBSFormField.Name}, the ending field named {sbsRepeaterField.EndingFieldName} could not be found");
            this.EndingIndex = this.FindEndingFieldIndex(sbsRepeaterField.EndingFieldName);

            // Copy the starting index and ending index to the underlying repeater field model
            sbsRepeaterField.RepeaterIndex = this.ThisRepeaterIndex;
            sbsRepeaterField.EndingIndex = this.EndingIndex;

            formView.FormFieldChanged += (field, value) =>
            {
                if (field != this.EndingField)
                    return;
                
                // Save the filled-out fields as name/value pairs.
                sbsRepeaterField.SavedObjects.Add(this.SaveFields());

                // If the ending field's value is not equal to the loop-continue value, then end the repeater loop
                if (!field.Value.Equals( ((SBSRepeaterField) this.SBSFormField).ContinueLoopValue) )
                    this.IsEnded = true;
            };
        }
        #endregion

        #region Methods
        private NameValueList SaveFields()
        {
            var nvList = new NameValueList();

            // Find all fields that are persistable, and record those of them that have non-null values
            for (int idx = this.ThisRepeaterIndex+1;  idx < this.EndingIndex;  idx++)
            {
                var field = this.FormView.Form.Fields[idx];
                if (field is SBSPersistableFormField && field.Value != null)
                    nvList.Add(new NameValuePair(field.Name, field.Value));
            }

            return nvList;
        }

        private ISBSFormField FindEndingField(string fieldname)
        {
            var field = this.FormView.Form.FindField(fieldname);
            return field;
        }

        private int FindEndingFieldIndex(string fieldname)
        {
            var idx = this.FormView.Form.FindFieldIndex(fieldname);
            return idx;
        }
        #endregion
    }
}