using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThumbDriveDuplicator
{
    public static class ExceptionExtensions
    {
        public static string FormatFullMessage(this Exception err)
        {
            return err.FormatFullMessage(false, 4);
        }

        public static string FormatFullMessage(this Exception err, bool onlyInner)
        {
            return err.FormatFullMessage(onlyInner, 4);
        }

        public static string FormatFullMessage(this Exception err, int indent)
        {
            return err.FormatFullMessage(false, indent);
        }

        public static string FormatFullMessage(this Exception err, bool onlyInner, int indent)
        {
            if (err == null)
                return null;
            return err.ExceptionChain(onlyInner).Select((e, i) =>
                (new[] { string.Format("{0," + (i * indent) + "}{1}: {2}", string.Empty, e.GetType().FullName, e.Message) })
                .Concat(e.FormatStackTrace((i * indent) + indent)
                .Concat(e.FormatData((i * indent) + indent))
                ).MakeDelimited(Environment.NewLine)).MakeDelimited(Environment.NewLine);
        }

        public static IEnumerable<string> FormatStackTrace(this Exception err, int indent)
        {
            return err.StackTrace.GetLines(true).Select(l => string.Format("{0," + indent + "}{1}", string.Empty, l.Trim()));
        }

        public static IEnumerable<string> FormatData(this Exception err, int indent)
        {
            return err.Data.Cast<DictionaryEntry>().Select(d => string.Format("{0," + indent + "}{1}={2}", string.Empty, d.Key, d.Value));
        }

        public static IEnumerable<Exception> ExceptionChain(this Exception err)
        {
            return err.ExceptionChain(false);
        }

        public static IEnumerable<Exception> ExceptionChain(this Exception err, bool innerExceptionsOnly)
        {
            if (err == null)
                yield break;
            if (!innerExceptionsOnly)
                yield return err;
            for (var inner = err.InnerException; inner != null; inner = inner.InnerException)
                yield return inner;
        }
    }
}
