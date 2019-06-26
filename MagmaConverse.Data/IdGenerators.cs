using System;

namespace MagmaConverse.Data
{
    public class IdGenerators
    {
        public static string FieldId(string prefix = "FormField.")
        {
            return $"{prefix ?? ""}{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public static string FormId(string prefix = "Form.")
        {
            return $"{prefix ?? ""}{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public static string FormInstanceId(string prefix = "FormInstance.")
        {
            return $"{prefix ?? ""}{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public static string RefDataId(string prefix = "ReferenceData.")
        {
            return $"{prefix ?? ""}{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
