using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Magmasystems.Framework;
using MagmaConverse.Interfaces;

namespace MagmaConverse.Data.Fields
{
    [DataContract]
    public class SBSListboxField : SBSPersistableFormField
    {
        [DataMember(Name = "items")]
        public NameValueList Items { get; set; } = new NameValueList();

        protected override void InitializeData(FormTemplateFieldDefinition fieldDef, object data = null, IHasLookup referenceDataRepo = null)
        {
            base.InitializeData(fieldDef, data, referenceDataRepo);

            if (data != null)
            {
                return;
            }

            if (fieldDef.Items != null && fieldDef.Items.Count > 0)
            {
                this.CopyCollection(this.Items, fieldDef.Items);
                return;
            }

            if (string.IsNullOrEmpty(fieldDef.Reference))
                return;

            // Get the collection from the reference data repo   
            IReferenceDataResolver referenceDataResolver = new ReferenceDataResolver();
            object collection = referenceDataResolver.Resolve(fieldDef.Reference, referenceDataRepo);
            this.CopyCollection(this.Items, collection);
        }

        private void CopyCollection(NameValueList dest, object collection)
        {
            if (dest == null || collection == null)
                return;

            if (collection is SortedDictionary<string, object> sorteddict)
            {
                foreach (var kvp in sorteddict)
                    this.Items.Add(new NameValuePair(kvp.Key, kvp.Value));
            }
            else if (collection is Dictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                    this.Items.Add(new NameValuePair(kvp.Key, kvp.Value));
            }
            else if (collection is IEnumerable<(string, object)> kvparray)
            {
                foreach (var a in kvparray)
                    this.Items.Add(new NameValuePair(a.Item1, a.Item2));
            }
            else if (collection is IEnumerable<object> array)
            {
                foreach (var a in array)
                    this.Items.Add(new NameValuePair(a.ToString(), a));
            }
            else
            {
                throw new ApplicationException($"Unknown reference data collection type {collection.GetType().Name}");
            }
        }

        public object LookupKey(string line)
        {
            return this.Items.FirstOrDefault(kvp => kvp.Name.Equals(line, StringComparison.OrdinalIgnoreCase));
        }
    }
}