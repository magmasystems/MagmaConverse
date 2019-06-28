using System;
using System.Collections;
using System.Runtime.InteropServices;
using MagmaConverse.Framework;

namespace MagmaConverse.Utilities
{
    public static class ConsoleHelpers
    {
        public const string CTRL_C_COMMAND = "!!!!CTRL+C!!!!";

        #region Console Helpers
        public static void CommandLoop(IDisposable app, Func<string, ResponseStatus> commandProcessor, string commandPrompt = "Input a command or press ENTER to quit")
        {
            bool exitRequested = false;

            Console.CancelKeyPress += (sender, args) =>
            {
                if (commandProcessor != null)
                {
                    ResponseStatus response = commandProcessor(CTRL_C_COMMAND);
                    if ((int) response.Value < 0)
                        exitRequested = true;
                }
                else
                {
                    ColoredWriteLine("CTRL+C Pressed - exiting the command loop", ConsoleColor.Red);
                    exitRequested = true;
                }
            };

            try
            {
                while (!exitRequested)
                {
                    ColoredWriteLine(commandPrompt, ConsoleColor.Cyan);
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;

                    // The "cls" command will clear the console
                    if (line.Equals("cls", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Clear();
                        continue;
                    }

                    if (commandProcessor != null)
                    {
                        ResponseStatus response = commandProcessor(line);
                        if (response == null)
                            continue;
                        if (response.StatusCode == ResponseStatusCodes.OK)
                        {
                            ColoredWriteLine(response.Value ?? "OK", ConsoleColor.Green);
                        }
                        else
                        {
                            ColoredWriteLine(response.ErrorMessage ?? "Error", ConsoleColor.Red);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                ColoredWriteLine(ExceptionHelpers.Format(exc), ConsoleColor.Red);
                Console.WriteLine();
                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
            }
            finally
            {
                app.Dispose();
            }
        }

        public static bool Choice(string prompt = "Do you want to try again? (Y/y)")
        {
            Console.WriteLine();
            Console.WriteLine(prompt);
            var keyInfo = Console.ReadKey();
            return keyInfo.KeyChar == 'y' || keyInfo.KeyChar == 'Y';
        }

        public static string ReadPassword()
        {
            string input = "";

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true); // true to intercept
                if (keyInfo.Key == ConsoleKey.Enter)
                    break;
                Console.Write("*");
                input = input + keyInfo.KeyChar;
            }
            Console.WriteLine();

            return input;
        }

        public static void ColoredWriteLine(object msg, ConsoleColor foreground)
        {
            Console.ForegroundColor = foreground;
            Console.WriteLine(msg.ToString());
            Console.ResetColor();
        }

        public static void DumpResults(object results, int limit = 0)
        {
            if (results == null)
                return;

            if (results is IList resultList)
            {
                int count = 0;

                foreach (var element in resultList)
                {
                    if (element is IList list)
                    {
                        foreach (var item in list)
                        {
                            ColoredWriteLine($"{item} ", ConsoleColor.White);
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        ColoredWriteLine(element.ToString(), ConsoleColor.White);
                    }
                    count++;
                    if (limit > 0 && count >= limit)
                        break;
                }
                return;
            }

            if (results is string sResults)
            {
                ColoredWriteLine(sResults, ConsoleColor.White);
            }
        }
        #endregion

        #region Methods to spaewn a new console and control it
        public const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        private static bool CreatedNewConsole { get; set; }

        /// <summary>
        /// Allocates a new console for current process.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        /// <summary>
        /// Frees the console.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        public static IntPtr SpawnConsole()
        {

            #if NETCOREAPP
            CreatedNewConsole = true;
            return (IntPtr) 0x666;
            #else
            // Get console window
            CreatedNewConsole = false;

            var handle = GetConsoleWindow();
            if (handle.ToInt32() == 0)
            {
                // Create the console window
                AllocConsole();
                CreatedNewConsole = true;
            }
            handle = GetConsoleWindow();

            // Show console window
            ShowWindow(handle, SW_SHOW);

            return handle;
            #endif
        }

        public static void KillConsole(IntPtr handle)
        {
            if (CreatedNewConsole)
            {
                #if NETCOREAPP
                #else
                ShowWindow(handle, SW_HIDE);
                FreeConsole();
                #endif
            }
        }
        #endregion
    }
}
