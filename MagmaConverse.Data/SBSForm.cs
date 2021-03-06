﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MagmaConverse.Data.Fields;
using MagmaConverse.Data.Workflow;
using Magmasystems.Framework.Core;
using MagmaConverse.Interfaces;
using Magmasystems.Persistence.Interfaces;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MagmaConverse.Data
{
    public interface ISBSForm : IHasName, IHasId
    {
        event Action<ISBSForm> Submitted;
        event Func<ISBSForm, bool> PreCancelled;
        event Action<ISBSForm> Cancelled;

        /// <summary>
        /// Returns true if the iteration through the submission function list should continue, false if not.
        /// </summary>
        event Func<IFormSubmissionFunction, object, bool> WorkflowResultReturned;

        /// <summary>
        /// Unique Id of the definition of the form.
        /// </summary>
        string DefinitionId { get; set; }

        /// <summary>
        /// The title that will be displayewd in the form
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Optional subtitle of the form
        /// </summary>
        string SubTitle { get; set; }

        /// <summary>
        /// Optional description of the form
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// A list of the fields in the form. The actual navigation order might
        /// be represdented by a FSM.
        /// </summary>
        List<ISBSFormField> Fields { get; set; }

        /// <summary>
        /// Returns a field within a form that matches the name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ISBSFormField FindField(string name);

        /// <summary>
        /// Indexer function
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        ISBSFormField this[string fieldName] { get; }

        /// <summary>
        /// Returns the 0-based index of a field within a form that matches the name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The 0-based index if found, -1 if not found</returns>
        int FindFieldIndex(string name);

        /// <summary>
        /// Called to submit a form (usually by one of the buttons in the view)
        /// </summary>
        void Submit(bool isCancel = false);

        /// <summary>
        /// Runs a workflow when a form's "submit" UI control is invoked.
        /// </summary>
        /// <param name="submissionFunctions">Contains the parameters of the submission function. A form can have multiple submission functions.</param>
        bool RunWorkflow(List<FormSubmissionFunction> submissionFunctions);
    }

    [DataContract]
    public class SBSForm : ISBSForm, IPersistableDocumentObject
    {
        #region Events
        public event Action<ISBSForm> Submitted = f => { };
        public event Func<ISBSForm, bool> PreCancelled = f => true;
        public event Action<ISBSForm> Cancelled = f => { };

        // <inheritdoc />
        public event Func<IFormSubmissionFunction, object, bool> WorkflowResultReturned = (submissionFunc, result) => true;
        #endregion

        #region Variables
        private readonly ILog Logger = LogManager.GetLogger(typeof(SBSForm));
        private readonly object m_fieldlistLock = new object();

        /// <inheritdoc />
        [IgnoreDataMember]
        [BsonIgnore]
        [JsonIgnore]
        public string Id { get; private set; }

        [DataMember]
        [BsonId]
        public string _id
        {
            get => this.Id;
            set => this.Id = value;
        }

        /// <inheritdoc />
        [DataMember]
        public string DefinitionId { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Name { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Title { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string SubTitle { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<ISBSFormField> Fields { get; set; } = new List<ISBSFormField>();
        #endregion

        #region Constructors
        public SBSForm()
        {
            this.Id = IdGenerators.FormInstanceId();
        }

        public SBSForm(SBSFormDefinition def) : this()
        {
            if (def?.Definition == null)
                return;

            if (!string.IsNullOrEmpty(def.Definition.Name))
                this.Id = IdGenerators.FormInstanceId($"FormInstance.{def.Definition.Name}.");

            this.DefinitionId = def.Id;
            this.Name = def.Definition.Name;
            this.Title = def.Definition.Title;
            this.SubTitle = def.Definition.SubTitle;
            this.Description = def.Definition.Description;
        }

        public SBSForm(string name, List<ISBSFormField> fields)
        {
            this.Name = name;
            this.Fields = fields;
        }
        #endregion

        #region Methods
        public static ISBSForm Materialize(SBSFormDefinition def, IHasLookup referenceDataResolver = null)
        {
            if (def?.Definition == null)
                return null;

            var form = new SBSForm(def);
            return form.MaterializeFields(def, referenceDataResolver);
        }

        public ISBSForm MaterializeFields(SBSFormDefinition def, IHasLookup referenceDataResolver = null)
        {
            if (def?.Definition == null)
                return null;

            var newRequest = new FormAddFieldsRequest { Fields = def.Definition.Fields };
            this.AddFields(newRequest, referenceDataResolver);

            return this;
        }

        public IEnumerable<ISBSFormField> AddFields(FormAddFieldsRequest request, IHasLookup referenceDataResolver = null)
        {
            var newRequest = new FormAddFieldsRequest { Fields = request.Fields, Index = -1, InsertMode = InsertMode.After };
            return this.InsertFields(newRequest, referenceDataResolver);
        }

        public IEnumerable<ISBSFormField> InsertFields(FormAddFieldsRequest request, IHasLookup referenceDataResolver = null)
        {
            if (request == null)
                return null;

            var newFields = new List<ISBSFormField>();

            foreach (var fieldDef in request.Fields)
            {
                ISBSFormField field = SBSFormField.Create(this, fieldDef, referenceDataResolver);
                newFields.Add(field);
            }

            int targetIndex;
            if (!string.IsNullOrEmpty(request.Target))
            {
                targetIndex = this.FindFieldIndex(request.Target);
                if (targetIndex < 0)
                    return null;
            }
            else
            {
                targetIndex = request.Index == -1 ? this.Fields.Count : request.Index;
            }
            targetIndex += request.InsertMode == InsertMode.Before ? 0 : 1;

            if (targetIndex >= this.Fields.Count)
            {
                this.Fields.AddRange(newFields);
            }
            else
            {
                this.Fields.InsertRange(targetIndex, newFields);
            }
            
            return newFields;
        }

        public ISBSFormField this[string fieldName] => this.FindField(fieldName);

        public ISBSFormField FindField(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return this.Fields.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public int FindFieldIndex(string name)
        {
            if (string.IsNullOrEmpty(name))
                return -1;

            return this.Fields.FindIndex(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void ClearFields()
        {
            this.Fields?.Clear();
        }

        public bool DeleteField(string name)
        {
            var idx = this.FindFieldIndex(name);
            if (idx < 0)
            {
                return false;
            }

            lock (this.m_fieldlistLock)
            {
                this.Fields.RemoveAt(idx);
            }
            return true;
        }

        public void Submit(bool isCancel = false)
        {
            if (isCancel)
            {
                this.Cancelled(this);
                return;
            }

            this.Submitted(this);
        }

        public bool RunWorkflow(List<FormSubmissionFunction> submissionFunctions)
        {
            // Sanity check
            if (submissionFunctions == null)
                return false;

            bool isCancel = false;

            // Go through all of the submission functions. Even if one of the functions is a cancellation function, we execute the process.
            using (var processor = new SBSFormSubmissionWorkflowProcessor(this, this.EvaluateBodyExpression))
            {
                foreach (var submissionFunc in submissionFunctions)
                {
                    //System.Console.WriteLine(submissionFunc);
                    if (submissionFunc.Cancel)
                    {
                        isCancel = true;
                    }
                    if (submissionFunc.Workflow != null)
                    {
                        // TODO - take care of async processing here
                        try
                        {
                            var result = processor.RunWorkflow(submissionFunc);
                            if (!this.WorkflowResultReturned(submissionFunc, result))
                                break;
                        }
                        catch (Exception e)
                        {
                            this.Logger.Error(e);
                        }
                    }
                }
            }

            // Returns false if the workflow resulted in a cancellation
            return !isCancel;
        }

        private object EvaluateBodyExpression(string expr, IFormSubmissionFunction submissionFunction)
        {
            // ${field:employeeDetailsGroup}
            object rc = null;
            
            foreach (var field in new StringSubstitutor().GetFieldsInExpression(expr, "field", this))
            {
                if (field is SBSRepeaterField repeaterField)
                {
                    var nvList = repeaterField.SaveFields();
                    rc = nvList.ToDictionary(nvp => nvp.Name, nvp => nvp.Value);
                }
                else
                {
                    rc = field.Value;
                }
            }

            return rc;
        }

        #endregion

        #region Property Change Handlers
        public virtual void OnFormPropertyChanged(string propName, object oldValue, object value)
        {
            if (string.IsNullOrEmpty(propName))
                // ReSharper disable once RedundantJumpStatement
                return;
        }

        public virtual void OnFieldPropertyChanged(SBSFormField field, string propName, object oldValue, object value)
        {
            if (field == null || string.IsNullOrEmpty(propName))
                // ReSharper disable once RedundantJumpStatement
                return;
        }
        #endregion
    }
}
