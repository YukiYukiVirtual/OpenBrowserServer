using System;
using System.Collections.Generic;
using System.IO;

namespace OpenBrowserServer.Logger
{
    public class History
    {
        const string directoryName = "Log";
        const string filePrefix = "History_";
        public string LogFileName { get; private set; }
        public History()
        {
            // 起動時の日付でログファイル名を作成する
            LogFileName = $"{directoryName}\\{filePrefix}{DateTime.Today:yyyy-MM-dd}.log";
            // ログ用ディレクトリを作成する(無ければ)
            Directory.CreateDirectory(directoryName);

            // 10ファイル残して古いファイルを削除する
            int maxFileNum = 10;
            string[] filenameList = Directory.GetFiles(directoryName, $"{filePrefix}*", SearchOption.TopDirectoryOnly);
            Array.Sort<string>(filenameList);
            for(int i=0; i<(filenameList.Length - maxFileNum); i++)
            {
                string filename = filenameList[i];
                Console.WriteLine($"Delete {filename}");
                File.Delete(filename);
            }

        }
        public void WriteLine(string str)
        {
            string log = $"{DateTime.Now:[yyyy-MM-dd HH:mm:ss.ffff]} {str}";
            Console.WriteLine(log);
            using (var writer = new StreamWriter(LogFileName, true))
            {
                writer.WriteLine(log);
            }
        }
    }
}
