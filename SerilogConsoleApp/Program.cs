using System;
using System.Threading;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.IOFile;

namespace SerilogConsoleApp
{
    internal class Program
    {
        protected static ILogger Log =
            new LoggerConfiguration()
                // Set logging level
                .MinimumLevel.Debug()
               
                // Define where to write the log output
                .WriteTo.ColoredConsole()
                .WriteTo.ElasticSearch(indexFormat: "serilog-{0:yyyy.MM.dd}")
                .WriteTo.Sink(new FileSink(@"C:\temp\logs\json-log.txt", new JsonFormatter(), null))
                
                // Enrichers
                .Enrich.FromLogContext()
                .CreateLogger();

        private static void Main(string[] args)
        {
            // Creates a logger for this specific class
            var log = Log.ForContext<Program>();

            // Multiple contextual loggers can be created, and these can be nested, e.g.
            // var txnLog = log.ForContext("TransactionId", currentTransaction.Id);

            #region Basic logging

            log.Debug("Wowsie! Stuff happened at {loc}.", new {@long = 22.4, @lat = 43.1});
            Console.ReadKey(true);

            log.Information("Person entered the stage: {@data}", new {User = "Torstein", Presenter = "true"});
            Console.ReadKey(true);

            log.Warning(new NotImplementedException("Too soon!"), "Joke failed.");
            Console.ReadKey(true);

            log.Error(new OutOfMemoryException("You should start closing tabs."), "Could not open another browser.");
            Console.ReadKey(true);

            #endregion

            Console.ReadKey(true);
            Console.WriteLine();

            #region Log Context

            var users = new[] {"Torstein", "Vidar", "Alice"};
            foreach (string user in users)
            {
                // For this to work, you must add Enrich.FromLogContext() to the configuration
                using (LogContext.PushProperty("User", user))
                {
                    // All log messages within this scope will have the field "User" 

                    log.Debug("Getting accounts");
                    log.Debug("Moving money");
                    log.Debug("Checking balance");

                    if (string.Equals(user, "Alice"))
                    {
                        log.Error(new ArgumentException("Too much money"), "Could not move money.");
                    }
                }
            }

            #endregion

            Console.ReadKey(true);
            Console.WriteLine();

            #region Timing (Extension)

            using (log.BeginTimedOperation("Performing timing.", "Overlord"))
            {
                Thread.Sleep(142);
            }

            Console.ReadKey(true);

            using (log.BeginTimedOperation("Timing critcal task", "The Fox.", LogEventLevel.Debug, TimeSpan.FromSeconds(1)))
            {
                Thread.Sleep(1100);
            }

            #endregion

            Console.ReadKey(true);
        }
    }
}