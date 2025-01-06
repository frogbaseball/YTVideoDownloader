using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YTVideoDownloader {
    internal static class FileData {
        public readonly static string dataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\YTVideoDownloader\\CustomData\\data.txt";
        public static void CreateDirectoryAndFile() {
            Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
            File.Create(dataPath).Close();
        }
        public static void SaveToFile(string message) {
            File.WriteAllText(dataPath, message);
        }
        public static string ReadFromFile() {
            var file = File.ReadAllLines(dataPath);
            if (file.Length == 0) {
                return null;
            }
            return file[0];
        }
    }
}
