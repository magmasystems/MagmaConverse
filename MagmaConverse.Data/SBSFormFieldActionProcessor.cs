using System;
using System.Collections.Generic;
using log4net;
using Magmasystems.Framework;
using Newtonsoft.Json.Linq;

namespace MagmaConverse.Data
{
    public class FieldActionResult
    {
        public bool Success { get; internal set; }
        public int NewIndex { get; internal set; }

        public FieldActionResult(int idxCurrent)
        {
            this.NewIndex = idxCurrent;
            this.Success = true;
        }
    }
    
    public class SBSFormFieldActionProcessor
    {
        private SBSFormField Field { get; }
        private ISBSForm Form { get; }
        private int CurrentIndex { get; }
        public FieldActionResult Result { get; }

        private ILog Logger { get; }

        private static readonly IDictionary<string, Predicate<SBSFormField>> ActionPredicateMap = new Dictionary<string, Predicate<SBSFormField>>
        {
            { "always",     f => true  },
            { "never",      f => false  },
            { "ontrue",     f => f.Value is bool && (bool) f.Value  },
            { "onfalse",    f => f.Value is bool && (bool) f.Value == false },
            { "onnotnull",  f => f.Value != null  },
            { "onnull",     f => f.Value == null  },
            
        };

        public SBSFormFieldActionProcessor()
        {
            this.Logger = LogManager.GetLogger(this.GetType());
        }


        public SBSFormFieldActionProcessor(ISBSFormField field, int idxCurrent)
        {
            this.Field = (SBSFormField) field;
            this.Form = this.Field.Form;
            this.CurrentIndex = idxCurrent;

            // Init the return information
            this.Result = new FieldActionResult(idxCurrent);
        }

        public virtual FieldActionResult PerformActions()
        {
            try
            {
                if (this.Field.Actions == null)
                    return this.Result;

                foreach (var action in this.Field.Actions)
                {
                    var thingToDo = action.Value as JObject;
                    if (thingToDo == null)
                        continue;

                    if (ActionPredicateMap.TryGetValue(action.Key.ToLower(), out var predicate))
                    {
                        if (predicate(this.Field))
                        {
                            this.ExecuteJump(thingToDo)
                                .ExecuteWorkflow(thingToDo);
                        }
                    }
                    else
                    {
                        ConsoleHelpers.ColoredWriteLine($"Unknown field action {action.Key}", ConsoleColor.Red);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return this.Result;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private SBSFormFieldActionProcessor ExecuteJump(JObject jumpObject)
        {
            /* "onTrue": { "jump": "sectionAfterSignatory" } */
            this.Result.NewIndex = this.CalculateJumpIndex(jumpObject["jump"]);
            return this;
        }

        private int CalculateJumpIndex(JToken jumpTarget)
        {
            if (jumpTarget == null)
                return this.CurrentIndex;

            int idxNew = this.Form.FindFieldIndex(jumpTarget.Value<string>());
            return idxNew < 0 ? this.CurrentIndex : idxNew;
        }

        private SBSFormFieldActionProcessor ExecuteWorkflow(JObject thingToDo)
        {
            /* "onTrue": { "submissionFunctions": [ {}, {} ] } */
            var jFunctions = thingToDo["submissionFunctions"] as JArray;
            if (jFunctions == null)
                return this;

            List<FormSubmissionFunction> submissionFunctions = jFunctions.ToObject<List<FormSubmissionFunction>>();
            if (submissionFunctions == null || submissionFunctions.Count == 0)
                return this;

            this.Form.RunWorkflow(submissionFunctions);

            return this;
        }
    }
}
