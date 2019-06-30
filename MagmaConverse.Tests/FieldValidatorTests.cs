using System.Collections.Generic;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MagmaConverse.Tests
{
    [TestClass]
    public class FieldValidatorTests
    {
        [TestMethod]
        public void FieldRequiredValidatorTest()
        {
            IFieldValidator validator = new FieldRequiredValidator();

            bool rc = validator.Validate(new SBSPhoneNumberEditField { Value = null });
            Assert.IsFalse(rc, "A null value in a required field should be invalid");

            rc = validator.Validate(new SBSPhoneNumberEditField { Value = "973-912-0339" });
            Assert.IsTrue(rc, "A noin-null value in a required field should be valid");
        }

        [TestMethod]
        public void PhoneNumberValidatorTest()
        {
            // http://regexlib.com/REDetails.aspx?regexp_id=58
            // Optional opening paren
            // 3 digits
            // optional closing paren
            // Space or dash
            // 3 digits      ---
            // Space or dash   |-- or 7 digits
            // 4 digits      ---
            string regex = @"^(\(?\d{3}\)?|\d{3})( |-)?(\d{3}( |-)?\d{4}|\d{7})$";
            IFieldValidator validator = new FieldRegexValidator(regex);

            string[] phoneNumbers =
            {
                "973-912-0339",
                "(973) 912-0339",
            };
            foreach (var phone in phoneNumbers)
            {
                bool rc = validator.Validate(new SBSPhoneNumberEditField {Value = phone});
                Assert.IsTrue(rc, $"{phone} should be valid");
            }
        }

        [TestMethod]
        public void ValidateSingleFieldRule()
        {
            const string errorMsg = "The value must be no more than 50";
            IFieldValidator validator = new FieldRulesValidator(new List<FieldValidationRule>
            {
                new FieldValidationRule { Rule = "${value} <= 50", ErrorMessage = errorMsg }
            });

            bool rc = validator.Validate(new SBSIntegerEditField { Value = 30 });
            Assert.IsTrue(rc, "30 is less than 50");

            rc = validator.Validate(new SBSIntegerEditField { Value = 60 });
            Assert.IsFalse(rc, "60 is not less than 50");
            Assert.AreEqual(errorMsg, validator.ValidationFailedMessage);
        }

        [TestMethod]
        public void ValidateMultipleFieldsRule()
        {
            const string errorMsg = "The passwords must match";
            IFieldValidator validator = new FieldRulesValidator(new List<FieldValidationRule>
            {
                new FieldValidationRule { Rule = "${value} == ${field:password1}", ErrorMessage = errorMsg }
            });

            var password1 = new SBSPasswordEditField { Name = "password1", Value = "123" };
            var password2 = new SBSPasswordEditField { Name = "password2", Value = "123" };
            SBSForm form = new SBSForm("Test Password Form", new List<ISBSFormField> {password1, password2});

            bool rc = validator.Validate(password2, form);
            Assert.IsTrue(rc, "The passwords should match");

            password2.Value = "456";
            rc = validator.Validate(password2, form);
            Assert.IsFalse(rc, "The passwords should not match");
            Assert.AreEqual(errorMsg, validator.ValidationFailedMessage);
        }
    }
}