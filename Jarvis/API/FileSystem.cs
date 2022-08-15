using Jarvis.Behaviors;
using System;
using System.IO;

namespace Jarvis.API
{
    /// <summary>
    /// Class for getting a behavior's personal files.
    /// </summary>
    public static class FileSystem
    {
        private static string GetRelDir<T>(T behavior) where T : IBehaviorBase
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory,
                behaviorDir = baseDir + @"\Behaviors\" + behavior.GetType().Name;
            if (!Directory.Exists(behaviorDir)) Directory.CreateDirectory(behaviorDir);
            return behaviorDir;
        }

        private static string GetRelFile<T>(T behavior, string fileName) where T : IBehaviorBase =>
            GetRelDir(behavior) + "\\" + fileName;

        public static string GetFileText<T>(T behavior, string fileName) where T : IBehaviorBase
        {
            string file = GetRelFile(behavior, fileName);
            if (!File.Exists(file)) return null;
            return File.ReadAllText(file);
        }

        public static byte[] GetFileRaw<T>(T behavior, string fileName) where T : IBehaviorBase
        {
            string file = GetRelFile(behavior, fileName);
            if (!File.Exists(file)) return null;
            return File.ReadAllBytes(file);
        }

        public static bool ClearFiles<T>(T behavior) where T : IBehaviorBase
        {
            string behaviorDir = GetRelDir(behavior);
            string[] files = Directory.GetFiles(behaviorDir);
            for (int i = 0; i < files.Length; i++) File.Delete(files[i]);
            return true;
        }

        public static void WriteFileText<T>(T behavior, string fileName, string text) where T : IBehaviorBase =>
            File.WriteAllText(GetRelFile(behavior, fileName), text);

        public static void WriteFileBytes<T>(T behavior, string fileName, byte[] data) where T : IBehaviorBase =>
            File.WriteAllBytes(GetRelFile(behavior, fileName), data);
    }
}
