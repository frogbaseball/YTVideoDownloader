using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YTVideoDownloader {
    internal static class FileData {
        static string path = $"{Environment.CurrentDirectory}\\..\\..\\CustomData\\data.txt";
        public static void SaveToFile(string message) {
            File.WriteAllText(path, message);
        }
        public static string ReadFromFile() {
            var file = File.ReadAllLines(path);
            if (file.Length == 0) {
                return null;
            }
            return file[0];
        }
    }
}
