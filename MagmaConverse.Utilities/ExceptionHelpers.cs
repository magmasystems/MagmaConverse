using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MagmaConverse.Utilities
{
    public static class ExceptionHelpers
    {
        public static string GetInnermostExceptionMessage(Exception exc)
        {
            string message = string.Empty;

            for (; exc != null; exc = exc.InnerException)
                message = exc.Message;

            return message;
        }

        public static void DumpExceptionStack(Exception exc)
        {
            while (exc != null)
            {
                Debug.Print(exc.Message);
                exc = exc.InnerException;
            }
        }

        public static string Format(Exception exc, bool doIndent = true)
        {
            if (exc is AggregateException aggregateException)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var e in aggregateException.InnerExceptions)
                {
                    sb.AppendLine(InternalFormat(e, 0, doIndent));
                }
                return sb.ToString();
            }

            return InternalFormat(exc, 0, doIndent);
        }

        private static string InternalFormat(Exception exc, int level, bool doIndent)
        {
            if (exc == null)
                return string.Empty;

            string s = (level > 0) ? Environment.NewLine : string.Empty;
            if (doIndent)
                s = Enumerable.Range(0, level).Aggregate(s, (current, n) => current + "  ");  // put space at the beginning for indentation
            s += exc.Message;

            return s + InternalFormat(exc.InnerException, level + 1, doIndent);
        }
    }
}

