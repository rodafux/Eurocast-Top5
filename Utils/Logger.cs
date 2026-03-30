using System;
using System.IO;

namespace Top5.Utils
{
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Log(string message)
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            // Format strict aaaa_mm_jj exigé
            string fileName = $"log_{DateTime.Now:yyyy_MM_dd}.txt";
            string filePath = Path.Combine(LogDirectory, fileName);

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(filePath, logEntry);
        }
    }
}