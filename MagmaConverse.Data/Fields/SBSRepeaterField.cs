using System.Collections.Generic;
using System.Runtime.Serialization;
using Magmasystems.Framework;
using MagmaConverse.Interfaces;
using Newtonsoft.Json;

namespace MagmaConverse.Data.Fields
{
    [DataContract]
    public class SBSRepeaterField : SBSFormField
    {
        /// <summary>
        /// The name of the group, such as "Employee"
        /// </summary>
        [DataMember]
        public string GroupName { get; private set; }

        /// <summary>
        /// The name of the field that is the last field to process before we jump back to the start of the loop (this repeater field)
        /// </summary>
        [DataMember]
        public string EndingFieldName { get; private set; }

        /// <summary>
        /// We may want to append a custom suffix to the name of a field that is repeated (like Employee.Name.1, Employee.Name.2, etc)
        /// </summary>
        [DataMember]
        public string FieldInstanceSuffix { get; private set; }

        /// <summary>
        /// At the end of the repeater should be a field (button, checkbox, choice) that asks the user if they want to add another entry.
        /// The value that the usewr enters into this field dictates whether the repeater will loop again.
        /// </summary>
        [DataMember]
        public object ContinueLoopValue { get; private set; }

        /// <summary>
        /// Contains a list of changed fields within the repeater
        /// </summary>
        [DataMember]
        public List<NameValueList> SavedObjects { get; } = new List<NameValueList>();

        protected override void InitializeData(FormTemplateFieldDefinition fieldDef, object data = null, IHasLookup referenceDataRepo = null)
        {
            this.GroupName = this.GetProp<string>("groupname") ?? IdGenerators.FieldId();
            this.EndingFieldName = this.GetProp<string>("end");
            this.FieldInstanceSuffix = this.GetProp<string>("suffix") ?? "${index}";
            this.ContinueLoopValue = this.GetProp("continuevalue") ?? true;

            base.InitializeData(fieldDef, data, referenceDataRepo);
        }

        [IgnoreDataMember]
        [JsonIgnore]
        public int RepeaterIndex { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        public int EndingIndex { get; set; }

        public NameValueList SaveFields()
        {
            var nvList = new NameValueList();

            // Find all fields that are persistable, and record those of them that have non-null values
            for (int idx = this.RepeaterIndex + 1; idx < this.EndingIndex; idx++)
            {
                var field = this.Form.Fields[idx];
                if (field is SBSPersistableFormField && field.Value != null)
                    nvList.Add(new NameValuePair(field.Name, field.Value));
            }

            return nvList;
        }
    }
}
