using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CompoundException
    {
        public CompoundException(IReadOnlyList<Exception> exceptions, AssertionLibraryFilter filter)
        {
            var all = exceptions.Select(x => new ExceptionInfo(x, filter)).ToArray();

            PrimaryException = all.First();
            IsAssertionException = filter.IsAssertionException(exceptions.First());
            SecondaryExceptions = all.Skip(1).ToArray();
            CompoundStackTrace = GetCompoundStackTrace(all);
        }

        public ExceptionInfo PrimaryException { get; }
        public string Type => PrimaryException.Type;
        public string Message => PrimaryException.Message;
        public bool IsAssertionException { get; }

        public IReadOnlyList<ExceptionInfo> SecondaryExceptions { get; }

        public string CompoundStackTrace { get; }

        static string GetCompoundStackTrace(IEnumerable<ExceptionInfo> exceptions)
        {
            using (var console = new StringWriter())
            {
                bool isPrimaryException = true;

                foreach (var ex in exceptions)
                {
                    if (isPrimaryException)
                    {
                        console.WriteLine(ex.Message);
                        console.Write(ex.StackTrace);
                    }
                    else
                    {
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine($"===== Secondary Exception: {ex.Type} =====");
                        console.WriteLine(ex.Message);
                        console.Write(ex.StackTrace);
                    }

                    var walk = ex;
                    while (walk.InnerException != null)
                    {
                        walk = walk.InnerException;
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine($"------- Inner Exception: {walk.Type} -------");
                        console.WriteLine(walk.Message);
                        console.Write(walk.StackTrace);
                    }

                    isPrimaryException = false;
                }
                return console.ToString();
            }
        }
    }
}