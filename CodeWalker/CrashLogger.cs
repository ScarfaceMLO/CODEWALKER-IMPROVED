using System;
using System.IO;

namespace CodeWalker
{
    public static class CrashLogger
    {
        private static string logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "CodeWalker_CrashLog.txt"
        );
        
        private static object lockObj = new object();

        public static void Log(string message)
        {
            try
            {
                lock (lockObj)
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    string logMessage = $"[{timestamp}] {message}";
                    
                    // Écrit dans le fichier
                    File.AppendAllText(logPath, logMessage + Environment.NewLine);
                    
                    // Écrit aussi dans Debug pour Visual Studio
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
            }
            catch
            {
                // Ignore les erreurs de logging
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(logPath))
                {
                    File.Delete(logPath);
                }
            }
            catch
            {
                // Ignore
            }
        }
    }
}
