using System;
using System.Text;
using System.IO;
using System.Configuration;

namespace LogManagerNet
{
    public class LogConfig
    {


        /// <summary>
        /// [設定][Encoding]ログ出力時の文字コード
        /// </summary>
        public Encoding LogEncoding { get; set; }

        /// <summary>
        /// [設定][MaxSizeLimitMB*1024*1024]１ファイル当たりのサイズリミット ( 0:リミット無し / 0越え:リミットあり )
        /// </summary>
        public double MaxSizeLimit { get; set; } = 40000000;
        /// <summary>
        /// [設定][MaxSizeLimitMB]１ファイル当たりのサイズリミット MB( 0:リミット無し / 0越え:リミットあり )
        /// </summary>
        public double MaxSizeLimitMB { get; set; } = 0;

        /// <summary>
        /// [設定][LogRotateLimitIndex]１ファイル当たりのローテーションリミット ( 0:ローテートなし / 0越え:指定個数分ファイルをローテートする ）
        /// </summary>
        public int LogRotateLimitIndex { get; set; } = 99;

        /// <summary>
        /// [設定][LogLevel]ログ出力レベル
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.DEBUG;

        /// <summary>
        /// [設定][LogDirPath]ファイル出力先パス
        /// </summary>
        public string LogDirPath { get; set; } = "";
        /// <summary>
        /// [設定][LogFileName]ファイル名キーワード
        /// </summary>
        public string LogFileName { get; set; } = "";
        /// <summary>
        /// [設定][LogFileNameExtension]ファイル名拡張子
        /// </summary>
        public string LogFileNameExtension { get; set; } = "";
        /// <summary>
        /// [設定][LogFileNamePrefixDateTimeString]ファイル名接頭辞
        /// </summary>
        public string LogFileNamePrefixDateTimeString { get; set; } = "";
        /// <summary>
        /// [設定][LogFileNameSuffixDateTimeString]ファイル名接尾語
        /// </summary>
        public string LogFileNameSuffixDateTimeString { get; set; } = "";

        /// <summary>
        /// [設定][FileDeleteDayPoint]ファイルを削除する日付ポイント（-1:削除しない、N:N日前の古いログを削除する）
        /// </summary>
        public int FileDeleteDayPoint { get; set; } = -1;



        /// <summary>
        /// 設定ファイルの読み込み
        /// </summary>
        /// <param name="configFile">設定ファイル名</param>
        /// <exception cref="System.NullReferenceException">構成ファイルにデータが存在していない。</exception>
        public LogConfig(string configFile)
        {

            string configFilePath = Path.Combine(Environment.CurrentDirectory, configFile);
            if (File.Exists(configFilePath) == false)
            {
                // 構成ファイルが存在しないためエラー
                throw new FileNotFoundException("Not Config File.configFile=" + configFile, configFilePath);
            }

            ExeConfigurationFileMap exeFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(exeFileMap, ConfigurationUserLevel.None);


            // ログレベルの設定
            var strLogLevel = GetConfig(config, "LogLevel", "DEBUG");
            try
            {
                LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), strLogLevel);
            }
            catch (Exception e)
            {
                LogLevel = LogLevel.DEBUG; // ここでエラーが発生した場合、デバックレベルとする
                System.Console.WriteLine("[LogConfigLoadError]LogLevel=" + "DEBUG," + e.ToString());
            }
            // １ファイル当たりのファイルマックス数
            var strMaxSizeLimit = GetConfig(config, "MaxSizeLimitMB", "0");
            if (double.TryParse(strMaxSizeLimit, out double mb))
            {
                MaxSizeLimit = mb * 1024 * 1024;
                MaxSizeLimitMB = mb;
            }
            else
            {
                MaxSizeLimit = 0; // 指定なし
                MaxSizeLimitMB = 0;
                System.Console.WriteLine("[LogConfigLoadError]MaxSizeLimitMB=" + MaxSizeLimit);
            }

            // 1ファイル当たりのローテート数
            var strLogRotateLimitIndex = GetConfig(config, "LogRotateLimitIndex", "-1");
            if (int.TryParse(strLogRotateLimitIndex, out int idx))
            {
                LogRotateLimitIndex = idx;
            }
            else
            {
                LogRotateLimitIndex = -1;// 指定なし
                System.Console.WriteLine("[LogConfigLoadError]LogRotateLimitIndex=" + LogRotateLimitIndex);
            }




            // 出力文字コードの指定
            var strEncoding = GetConfig(config, "Encoding", "shift_jis");
            try
            {
                LogEncoding = Encoding.GetEncoding(strEncoding);
            }
            catch (Exception e)
            {
                LogEncoding = Encoding.GetEncoding("shift_jis");   // 指定なし
                System.Console.WriteLine("[LogConfigLoadError]Encoding=" + "shift_jis," + e.ToString());
            }


            // [設定][LogDirPath]ファイル出力先パス
            var strLogDirPath = GetConfig(config, "LogDirPath", Path.Combine(Environment.CurrentDirectory, "logs"));
            try
            {
                if (Directory.Exists(strLogDirPath) == false)
                {
                    // フォルダが存在していないのでフォルダを作成
                    Directory.CreateDirectory(strLogDirPath);
                }
                LogDirPath = strLogDirPath;
            }
            catch (Exception e)
            {
                // ファイル出力先フォルダが存在しない例外情報をスロー
                System.Console.WriteLine("[LogConfigLoadError]LogDirPath=" + strLogDirPath);
                throw e;
            }
            // [設定][LogFileName]ファイル名キーワード
            LogFileName = GetConfig(config, "LogFileName", "_debug_");
            // [設定][LogFileNameExtension]ファイル名拡張子
            LogFileNameExtension = GetConfig(config, "LogFileNameExtension", ".log");
            // [設定][LogFileNamePrefixDateTimeString]ファイル名接頭辞
            LogFileNamePrefixDateTimeString = GetConfig(config, "LogFileNamePrefixDateTimeString", "");
            // [設定][LogFileNameSuffixDateTimeString]ファイル名接尾語
            LogFileNameSuffixDateTimeString = GetConfig(config, "LogFileNameSuffixDateTimeString", "");




            // [設定][FileDeleteDayPoint]ファイルを削除する日付ポイント（-1:削除しない、N:N日前の古いログを削除する）
            var strFileDeleteDayPoint = GetConfig(config, "FileDeleteDayPoint", "-1");
            if (int.TryParse(strFileDeleteDayPoint, out int day))
            {
                FileDeleteDayPoint = day;
            }
            else
            {
                FileDeleteDayPoint = -1;// 指定なし
                System.Console.WriteLine("[LogConfigLoadError]FileDeleteDayPoint=" + LogRotateLimitIndex);
            }
        }

        /// <summary>
        /// 設定ファイルから
        /// </summary>
        /// <param name="config">設定データオブジェクト</param>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">初期値</param>
        /// <returns></returns>
        private string GetConfig(Configuration config, string key, string defaultValue)
        {
            string keyvalue = "";
            try
            {
                keyvalue = config.AppSettings.Settings[key].Value;

            }
            catch (Exception e)
            {
                // データが存在しないのでデフォルト値を指定
                keyvalue = defaultValue;
                System.Console.WriteLine("[LogConfigLoadError]not key config. Key=" + key);
                System.Console.WriteLine(e.ToString());
            }

            return keyvalue;
        }
    }
}
