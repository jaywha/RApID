using System;
using System.Collections.ObjectModel;
using System.IO;

namespace csExceptionLogger
{
    /// <summary>
    /// Handles daily exceptions, appending new ones to that date's file.
    /// </summary>
    public class csExceptionLogger
    {
        /// <summary>
        /// Default location to place logs (current directory).
        /// Please be sure to end with a '\'
        /// <para/>
        /// Can be full or relative path --> C:\CamApp\Logs\ || ..\Logs\
        /// </summary>
        public static string DefaultLogLocation { get; set; } = "";

        /// <summary>
        /// Writes the specified debugee tag along with a full script for the exception to a file. (Singular)
        /// </summary>
        /// <param name="DEBUGEE">The debugee.</param>
        /// <param name="e">The exception.</param>        
        public static void Write(string DEBUGEE, Exception e)
        {
            if (!Directory.Exists(DefaultLogLocation)) Directory.CreateDirectory(DefaultLogLocation);

            var targetFileName = DefaultLogLocation + DEBUGEE + "-" + DateTime.Now.ToString("MM-dd-yyyy") + ".txt";
            var baseMessage = new string[] {
                        "===== START NEW RECORD =====",
                        "",
                        "The exception was of the type " + e.GetBaseException().ToString(),
                        "The inner exception was of the type " + (e.InnerException != null ? e.InnerException.GetType().ToString() ?? "not of a type we know!" : "nothing at all; it was null."),
                        "The main message was " + e.Message,
                        "The stack trace is as follows " + e.StackTrace,
                        "",
                        "==== END OF THIS RECORD =====" };

            Console.WriteLine($"Exception -> [{e.Source}]: {e.Message} {{{e.HelpLink}}}");

            try
            {
                if (!File.Exists(targetFileName))
                {
                    File.WriteAllLines(targetFileName, new string[] { $">>> Error log for {DEBUGEE} : {DateTime.Now:MM-dd-yyyy}", ">>> Exception", "" });
                }
                File.AppendAllLines(targetFileName, baseMessage);
            }
            catch (IOException ioe)
            {
                Console.WriteLine($"csExceptionLogger -> {ioe.Message}");
            }
        }

        /// <summary>
        /// Writes evey exception in the given list with the same debug tag. (Plural)
        /// </summary>
        /// <param name="DEBUGEE"></param>
        /// <param name="exceptionList"></param>
        public static void Write(string DEBUGEE, ReadOnlyCollection<Exception> exceptionList)
        {
            foreach (Exception e in exceptionList) Write(DEBUGEE, e);
        }
    }
}
