using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Jint;

namespace MagmaConverse.Data
{
    public interface IFieldValidator
    {
        string Name { get; }
        string ValidationFailedMessage { get; set; }

        bool Validate(ISBSFormField field, ISBSForm form = null);
    }

    [DataContract]
    [KnownType(typeof(FieldRegexValidator))]
    [KnownType(typeof(FieldLengthValidator))]
    [KnownType(typeof(FieldNumericRangeValidator))]
    [KnownType(typeof(FieldRequiredValidator))]
    [KnownType(typeof(FieldRulesValidator))]
    public abstract class FieldValidator : IFieldValidator
    {
        [DataMember(Name = "name")]
        public string Name { get; }

        [DataMember(Name = "errormessage")]
        public string ValidationFailedMessage { get; set; } = "The field is not valid";
 
        protected FieldValidator(string name)
        {
            this.Name = name;
        }

        protected FieldValidator(string name, string errorMessage) : this(name)
        {
            this.ValidationFailedMessage = errorMessage;
        }

        public virtual bool Validate(ISBSFormField field, ISBSForm form = null)
        {
            return true;
        }
    }

    [DataContract]
    public class FieldRequiredValidator : FieldValidator
    {
        public FieldRequiredValidator() : base("Required", "This field is required")
        {
        }

        public override bool Validate(ISBSFormField field, ISBSForm form = null)
        {
            if (field.Value == null && field.DefaultValue == null)
                return false;

            return base.Validate(field, form);
        }
    }

    [DataContract]
    public class FieldLengthValidator : FieldValidator
    {
        [DataContract]
        public enum LengthComparator
        {
            EQ, NE, LT, LE, GT, GE
        }

        [DataMember(Name="comparator")]
        public LengthComparator Comparator { get; private set; }

        [DataMember(Name = "length")]
        public int Length { get; private set; }

        public FieldLengthValidator(int n, LengthComparator comparator = LengthComparator.EQ) : base("Length", "The field length is not correct")
        {
            this.Length = n;
            this.Comparator = comparator;
        }

        public override bool Validate(ISBSFormField field, ISBSForm form = null)
        {
            if (field?.Value == null)
                return false;

            string val = field.Value.ToString();
            int len = val.Length;

            switch (this.Comparator)
            {
                case LengthComparator.EQ:
                    if (len != this.Length)
                        return false;
                    break;
                case LengthComparator.NE:
                    if (len == this.Length)
                        return false;
                    break;
                case LengthComparator.LT:
                    if (len >= this.Length)
                        return false;
                    break;
                case LengthComparator.LE:
                    if (len > this.Length)
                        return false;
                    break;
                case LengthComparator.GT:
                    if (len <= this.Length)
                        return false;
                    break;
                case LengthComparator.GE:
                    if (len < this.Length)
                        return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return base.Validate(field, form);
        }
    }

    [DataContract]
    public class FieldRegexValidator : FieldValidator
    {
        [DataMember(Name="regex")]
        public string ReadableRegex { get; set; }

        private Regex CompiledRegex { get; }

        public FieldRegexValidator(string regex) : this(regex, RegexOptions.Compiled)
        {
        }

        public FieldRegexValidator(string regex, RegexOptions options) : base("Regex", "The value is not the correct format")
        {
            if (string.IsNullOrEmpty(regex))
                throw new ApplicationException("The regular expression cannot be empty");
            this.ReadableRegex = regex;

            this.CompiledRegex = new Regex(this.ReadableRegex, RegexOptions.Compiled | options);
        }

        public override bool Validate(ISBSFormField field, ISBSForm form = null)
        {
            if (field?.Value == null)
                return false;

            if (!this.CompiledRegex.IsMatch(field.Value.ToString()))
                return false;

            return base.Validate(field, form);
        }
    }

    [DataContract]
    public class FieldNumericRangeValidator : FieldValidator
    {
        [DataMember(Name="min")]
        public double Min { get; }

        [DataMember(Name = "max")]
        public double Max { get; }

        public FieldNumericRangeValidator(int min = Int32.MinValue, int max = Int32.MaxValue, string errorMsg = "The value is not between the minimum and maximum values") : base("Range", errorMsg)
        {
            this.Min = min;
            this.Max = max;
        }

        public override bool Validate(ISBSFormField field, ISBSForm form = null)
        {
            try
            {
                double dValue = Convert.ToDouble(field.Value);
                return dValue >= this.Min && dValue <= this.Max;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    [DataContract]
    public class FieldRulesValidator : FieldValidator
    {
        [DataMember(Name = "rules")]
        public List<FieldValidationRule> Rules { get; set; }

        public FieldRulesValidator(List<FieldValidationRule> rules, string errorMsg = "The value broke the rule") : base("Rules", errorMsg)
        {
            this.Rules = rules;
        }

        public override bool Validate(ISBSFormField field, ISBSForm form = null)
        {
            if (this.Rules == null)
                return true;

            Engine expressionEvaluator = new Engine();

            var substitutor = new StringSubstitutor();
            foreach (var rule in this.Rules)
            {
                rule.CompiledRule = substitutor.PerformSubstitutions(rule.Rule, field, form, quotedStringValues: true);
                bool rc = expressionEvaluator.Execute((string) rule.CompiledRule).GetCompletionValue().AsBoolean();
                if (!rc)
                {
                    if (!string.IsNullOrEmpty(rule.ErrorMessage))
                        this.ValidationFailedMessage = rule.ErrorMessage;
                    return false;
                }
            }

            return true;
        }
    }

    public static class ValidatorFactory
    {
        public static IFieldValidator Create(string validator, params object[] args)
        {
            switch (validator.ToLower())
            {
                case "required":
                    return new FieldRequiredValidator();
                case "length":
                    return new FieldLengthValidator(Convert.ToInt32(args[0]));
                case "minlength":
                    return new FieldLengthValidator(Convert.ToInt32(args[0]), FieldLengthValidator.LengthComparator.GE);
                case "maxlength":
                    return new FieldLengthValidator(Convert.ToInt32(args[0]), FieldLengthValidator.LengthComparator.LE);
                case "regex":
                    return new FieldRegexValidator(args[0].ToString());
                case "range":
                    return new FieldNumericRangeValidator(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
                case "min":
                    return new FieldNumericRangeValidator(min: Convert.ToInt32(args[0]));
                case "max":
                    return new FieldNumericRangeValidator(max: Convert.ToInt32(args[0]));
                case "rules":
                    return new FieldRulesValidator(args[0] as List<FieldValidationRule>);
                default:
                    return null;
            }
        }
    }
}
