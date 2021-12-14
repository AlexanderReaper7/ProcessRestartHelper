using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProcessRestartHelper
{
    static class Program
    {
        static int pid;
        static Process process;
        static string path, passingArgs = null;

        /// <summary>
        /// Restarts a program
        /// </summary>
        /// <param name="args">[0] = process to wait for by PID, [1]application path, [2]optional custom max wait time in milliseconds, [3]optional args to pass along</param>
        static void Main(string[] args)
        {

            try
            {
                #if DEBUG
                Console.WriteLine($"args: {args}");
                #endif
                // Parse process
                pid = int.Parse(args[0]);
                // Get process by pid
                try
                {
                    process = Process.GetProcessById(pid);
                }
                catch (Exception)
                {
                    try
                    {
                        path = args[1];
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw new ArgumentException("path is invalid");
                    }
                    StartApp();
                    return;
                }
                
                // Parse application path
                if (args[1] != null)
                {
                    path = args[1];
                    if(!File.Exists(path)) Console.WriteLine("Application path is invalid, the specified file does not exist. Reverting to process path.");
                    else path = process.MainModule.FileName;
                }
                else path = process.MainModule.FileName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return;
            }

            // Parse custom wait time
            int waitTime = 180000;
            if (args[2] != null)
            {
                if (!int.TryParse(args[2], out waitTime)) Console.WriteLine("Failed to parse wait time, reverting to standard wait time.");
            }

            // Assign passing args
            if (args[3] != null)
            {
                passingArgs = args[3];
            }


            // Wait for process to exit for 3 minutes
            Console.WriteLine($"Waiting for process {process.ProcessName} : {pid} to exit for {new TimeSpan(0,0,0,0,waitTime)}...");
            bool processExited = process.WaitForExit(180000);

            // Time out if process didn't exit in time
            if (!processExited)
            {
                Console.WriteLine("Timed out");
                Console.ReadKey();
                return;
            }
            // Start application
            StartApp();
            // Exit
        }

        private static void StartApp()
        {
            if (passingArgs != null) Process.Start(path, passingArgs);
            else Process.Start(path);

            // TODO: check for successful start
        }
    }
}
