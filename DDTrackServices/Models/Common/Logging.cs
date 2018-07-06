/*
 *  Author: D. Edwards (Bluesmith Information Systems)
 *
 *  File Name: Logging.cs
 *
 *  Version History
 *
 *  Version Date        Who     Description
 *  ------- ----------  ---     --------------
 *  1.0     04/12/2017  D.E     Original Version.
 */

using System;
using Serilog;


namespace FGH.DDTrackPlus.Common
{
    /// <summary>
    /// Static class providing logging related functions.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Configures logging for the application.
        /// </summary>
        public static void ConfigureLogging()
        {
            const String APPLICATION_DIRECTORY_VARIABLE = "BASEDIR";

            // Set an environment variable (current process only) for serilog to use for the application directory.
            Environment.SetEnvironmentVariable(APPLICATION_DIRECTORY_VARIABLE, AppDomain.CurrentDomain.BaseDirectory);

            // Configure the logger for the application.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .ReadFrom.AppSettings()
                .CreateLogger();
            
            // Set the process exit event so that the logger can be flushed before closing.
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        /// <summary>
        /// Flushes the logger when the application process exists.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnProcessExit(Object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}
