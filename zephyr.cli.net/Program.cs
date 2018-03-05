using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace zephyr.cli.net
{
    class Program
    {
        //todo:
        //  - add array bounds checking on args
        //  - make sure target zephyr lib exists
        //  - try/catch the assembly load write nice errors
        //  - same for invoking main
        //  - plus add 'help'
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                WriteHelpAndExit();
            }

            if (IsHelp(args[0]))
            {
                WriteHelpAndExit();
            }

            string zephyrLib = $"zephyr.{args[0]}";
            AssemblyName an = new AssemblyName( zephyrLib );
            try
            {
                Assembly z = Assembly.Load(an);
                Type t = z.GetType("ZephyrCommandLine", throwOnError: true, ignoreCase: true);
                object zephyrCli = Activator.CreateInstance(t);

                MethodInfo main = zephyrCli.GetType().GetRuntimeMethod("Main", new Type[] { typeof(string[]) });
                if (main == null)
                    throw new Exception("Not able to execute Main(). Method is not defined in library.");

                string[] a = new string[args.Length - 1];
                for (int i = 1; i < args.Length; i++)
                    a[i - 1] = args[i];
                main.Invoke(zephyrCli, new object[] { a });
            }
            //catch (FileNotFoundException)
            //{
            //    Console.WriteLine("Not able to locate the library.");
            //}           
            catch (Exception ex)
            {
                WriteHelpAndExit(UnwindException(ex));
            }
            
        }

        static bool IsHelp(string s)
        {
            s = s.ToLower();
            if (s.Equals("help") || s.Equals("?"))
                return true;
            else
                return false;
        }

        static void WriteHelpAndExit(string message = null)
        {
            bool haveError = !string.IsNullOrWhiteSpace(message);

            ConsoleColor defaultColor = Console.ForegroundColor;

            Console_WriteLine($"zephyr.cli.exe, Version: {typeof(Program).Assembly.GetName().Version}\r\n", ConsoleColor.Green);
            Console.WriteLine("Syntax:");
            Console_WriteLine("  zephyr.cli.exe {0}libraryShortName{1} {0}actionTopic{1} {0}actionParameters{1}\r\n", ConsoleColor.Cyan, "{", "}");

            Console.WriteLine("  Examples:");
            Console.WriteLine("     zephyr.cli datatransformation json convert file:c\\temp\\products.json");
            Console.WriteLine("         outputFormat:yaml\r\n");
            Console.WriteLine("     zephyr.cli crypto werkit foo:bar hello:kitty");
            
            if (haveError)
                Console_WriteLine($"\r\n\r\n*** Last error:\r\n{message}\r\n", ConsoleColor.Red);

            Console.ForegroundColor = defaultColor;

            Environment.Exit(haveError ? 1 : 0); 
        }

        static void Console_WriteLine(string s, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(s, args);
        }

        // copied from Synapse.Core.Utilities.ExceptionHelpers
        public static string UnwindException(Exception ex)
        {
            return UnwindException(null, ex);
        }

        public static string UnwindException(string context, Exception ex, bool asSingleLine = false)
        {
            //string lineEnd = asSingleLine ? "|" : @"\r\n";
            string lineEnd = asSingleLine ? "|" : "\r\n";

            StringBuilder msg = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(context))
                msg.Append($"An error occurred in: {context}{lineEnd}");

            msg.Append($"{ex.Message}{lineEnd}");

            if (ex.InnerException != null)
            {
                if (ex.InnerException is AggregateException)
                {
                    AggregateException ae = ex.InnerException as AggregateException;
                    foreach (Exception wcx in ae.InnerExceptions)
                    {
                        Stack<Exception> exceptions = new Stack<Exception>();
                        exceptions.Push(wcx);

                        while (exceptions.Count > 0)
                        {
                            Exception e = exceptions.Pop();

                            if (e.InnerException != null)
                                exceptions.Push(e.InnerException);

                            msg.Append($"{e.Message}{lineEnd}");
                        }
                    }
                }
                else
                {
                    Stack<Exception> exceptions = new Stack<Exception>();
                    exceptions.Push(ex.InnerException);

                    while (exceptions.Count > 0)
                    {
                        Exception e = exceptions.Pop();

                        if (e.InnerException != null)
                            exceptions.Push(e.InnerException);

                        msg.Append($"{e.Message}{lineEnd}");
                    }
                }
            }

            return asSingleLine ? msg.ToString().TrimEnd('|') : msg.ToString();
        }

    }
}