using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MagmaConverse.Framework;
using MagmaConverse.Interfaces;
using Jint;
using System.Text;

namespace MagmaConverse.Data
{
    public class StringSubstitutor
    {
        public static IHasLookup ReferenceDataRepository { get; set; }

        public StringSubstitutor(IHasLookup repo = null)
        {
            if (repo != null)
            {
                ReferenceDataRepository = repo;
            }
        }

        public string PerformSubstitutions(string src, ISBSFormField field, ISBSForm form, bool quotedStringValues = false)
        {
            // Don't disturb the original string.
            string sNew = (new StringBuilder(src)).ToString();

            // Process the {value} string
            if (field != null)
                sNew = this.ReplaceStringWithValue(sNew, "${value}", field.Value, quotedStringValues);

            // Process the {field:name} string
            if (form != null)
                sNew = this.SubstituteTerm(sNew, "field", form, fieldName => form.FindField(fieldName)?.Value, quotedStringValues);

            sNew = this.SubstituteTerm(sNew, "config", form, this.FindConfigurationValue, quotedStringValues);

            return sNew;
        }

        /// <summary>
        /// This will replace a term like ${field:fieldname} with a corresponding value.
        /// We pass in a lambda which is the evaluation function.
        /// The evaluation function takes what's on the right side of the colon, and returns a string to be substituted in.
        /// </summary>
        private string SubstituteTerm(string src, string templateKeyword, ISBSForm form, Func<string, object> fnValueGetter, bool quotedStringValues)
        {
            string template = $"${{{templateKeyword}:";
            int templateLen = template.Length;

            int idx;
            while ((idx = src.IndexOf(template, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                int idxEnd = src.IndexOf("}", idx, StringComparison.OrdinalIgnoreCase);
                if (idxEnd < 0)
                    return src;

                object value;

                // Isolate the name of the field.
                // Note that the fieldName could be the name of this form, plus a property.
                string fieldName = src.Substring(idx + templateLen, idxEnd - (idx + templateLen));

                if (fieldName.StartsWith(form.Name, StringComparison.OrdinalIgnoreCase) && fieldName.Contains("."))
                {
                    string propertyName = fieldName.Split('.')[1]; // Isolate the first-level property
                    PropertyInfo propInfo = typeof(SBSForm).GetProperty(propertyName,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                    value = propInfo?.GetValue(form);
                }
                else
                {
                    value = fnValueGetter(fieldName);
                }

                if (value != null)
                    src = ReplaceStringWithValue(src, src.Substring(idx, idxEnd - idx + 1), value, quotedStringValues);
            }

            return src;
        }

        private string ReplaceStringWithValue(string s, string wordToMatch, object value, bool quotedStringValues)
        {
            if (value == null)
                return s.Replace(wordToMatch, "null");
            if (value is string)
            {
                string sValue = quotedStringValues ? $"'{value.ToString()}'" : $"{value.ToString()}";
                return s.Replace(wordToMatch, sValue);
            }
            return s.Replace(wordToMatch, value.ToString());
        }


        /// <summary>
        /// GetFieldNamesInExpression
        /// </summary>
        /// <param name="src">The expression</param>
        /// <param name="templateKeyword">A keyword to look for within the expression</param>
        /// <returns>An series of field names</returns>
        public IEnumerable<string> GetFieldNamesInExpression(string src, string templateKeyword)
        {
            string template = $"${{{templateKeyword}:";
            int templateLen = template.Length;
            int idxStart = 0;

            int idx;
            while ((idx = src.IndexOf(template, idxStart, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                int idxEnd = src.IndexOf("}", idx, StringComparison.OrdinalIgnoreCase);
                if (idxEnd < 0)
                    yield break;

                // Isolate the name of the field.
                // Note that the fieldName could be the name of this form, plus a property.
                string fieldName = src.Substring(idx + templateLen, idxEnd - (idx + templateLen));
                idxStart = idxEnd + 1;
                yield return fieldName;
            }
        }

        public IEnumerable<ISBSFormField> GetFieldsInExpression(string src, string templateKeyword, ISBSForm form)
        {
            foreach (string fieldName in this.GetFieldNamesInExpression(src, templateKeyword))
            {
                yield return form.FindField(fieldName);  // yields null if the fieldname is not in the form
            }
        }

        public IEnumerable<object> GetValuesInExpression(string src, string templateKeyword, ISBSForm form)
        {
            foreach (var field in this.GetFieldsInExpression(src, templateKeyword, form))
            {
                yield return field?.Value;
            }
        }

        private string FindConfigurationValue(string configPath)
        {
            var config = ApplicationContext.Configuration;
            if (config == null)
                return configPath;
            return config.Evaluate(configPath);
        }

        private static long s_sequenceNumber;

        public static string SubstituteVariablesInExpression(string src)
        {
            return SubstituteVariablesInExpression(src, variable =>
            {
                string variableLower = variable.ToLower();

                if (variableLower.StartsWith("expr:", StringComparison.CurrentCultureIgnoreCase))
                {
                    string expr = variable.Substring(5);
                    return EvaluateJintExpression(expr);
                }

                if (variableLower.StartsWith("random:", StringComparison.CurrentCultureIgnoreCase))
                {
                    string expr = variable.Substring(7);
                    var resolver = new ReferenceDataResolver();
                    object data = resolver.Resolve(expr, ReferenceDataRepository);
                    if (data != null && data is IEnumerable<object> collection)
                    {
                        var enumerable = collection as IList<object> ?? collection.ToList();
                        int idx = new Random().Next(enumerable.Count);
                        return enumerable.ElementAt(idx).ToString();
                    }
                }

                switch (variableLower)
                {
                    case "today":
                        return $"{DateTime.Today.ToShortDateString()}";

                    case "guid":
                        return $"{Guid.NewGuid().ToString().Substring(0, 8)}";

                    case "sequence":
                    case "seq":
                        return (++s_sequenceNumber).ToString();

                    default:
                       return "[UnknownVariable]";
                }
            });
        }

        public static string SubstituteVariablesInExpression(string src, Func<string, string> substitutor)
        {
            string template = "${";
            int templateLen = template.Length;

            int idx;
            while ((idx = src.IndexOf(template, StringComparison.Ordinal)) >= 0)
            {
                int idxEnd = src.LastIndexOf('}', src.Length-1);
                if (idxEnd < 0)
                    break;

                // Isolate the name of the field.
                // Note that the fieldName could be the name of this form, plus a property.
                string variable = src.Substring(idx + templateLen, idxEnd - (idx + templateLen));
                string value = substitutor(variable);

                src = src.Replace(src.Substring(idx, idxEnd - idx + 1), value);
            }

            return src;
        }

        public static string EvaluateJintExpression(string expr)
        {
            Engine expressionEvaluator = new Engine();
            var rc = expressionEvaluator.Execute(expr).GetCompletionValue();
            return rc.ToString();
        }
    }
}