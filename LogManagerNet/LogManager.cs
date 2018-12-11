using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.IO;

namespace LogManagerNet
{
    /// <summary>
    /// ログ出力クラス
    /// ログ出力を依頼されるメソッド…
    /// 実際にログ出力を実行するメソッド（定期的に300msで稼働している）
    /// 全てのログ出力が終わるまで待機するメソッド
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// ログ出力インスタンスを格納する辞書オブジェクト
        /// </summary>
        private static Dictionary<string, LogManager> s_instance = new Dictionary<string, LogManager>();
        /// <summary>
        /// 複数間スレッドの同期用ロックオブジェクト用
        /// </summary>
        private static object s_lockObj = new object();

        /// <summary>
        /// 指定キーに応じたログ出力インスタンスを生成（取得）する。
        /// </summary>
        public static LogManager GetInstance(string key = "")
        {
            if (s_instance.ContainsKey(key) == false)
            {
                s_instance.Add(key, new LogManager(key));
            }
            return s_instance[key];
        }

        /// <summary>
        /// 全てのログ出力インスタンスを終了させる。
        /// </summary>
        public static void AllClose()
        {
            foreach (KeyValuePair<string, LogManager> item in s_instance)
            {
                // 全てのインスタンスをクローズさせる
                item.Value.Close();
                //クリア
                s_instance.Remove(item.Key);

            }

        }
        

        /// <summary>
        /// 状態がクローズされているかのフラグ（true:クローズされている/ false:クローズされていない※ログ出力OK）
        /// </summary>
        private bool _isClose = false;

        private StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// ログ設定情報オブジェクト
        /// </summary>
        private LogConfig _config;

        /// <summary>
        /// 外部から呼び出されない用のプライベートコンストラクタ
        /// </summary>
        private LogManager(){}
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key">キー情報</param>
        private LogManager(string key = "")
        {
            _isClose = false;
            string configFile = @"log." + key + ".config";
            if ("".Equals(key))
            {
                configFile = @"log.config";
            }
            _config = new LogConfig(configFile);
            Info("[Config]ConfigFile=" + configFile);
            Info("[Config]LogLevel=" + _config.LogLevel.ToString());
            Info("[Config]MaxSizeLimitMB=" + _config.MaxSizeLimit + "( " + _config.MaxSizeLimitMB + " * 1024 * 1024 )");
            Info("[Config]LogRotateLimitIndex=" + _config.LogRotateLimitIndex);
            Info("[Config]Encoding=" + _config.LogEncoding.ToString());
            Info("[Config]LogDirPath=" + _config.LogDirPath);
            Info("[Config]LogFileName=" + _config.LogFileName);
            Info("[Config]LogFileNameExtension=" + _config.LogFileNameExtension);
            Info("[Config]LogFileNamePrefixDateTimeString=" + _config.LogFileNamePrefixDateTimeString);
            Info("[Config]LogFileNameSuffixDateTimeString=" + _config.LogFileNameSuffixDateTimeString);
            StartMonitor();
        }
        
        /// <summary>
        /// ログの出力レベルを設定する
        /// </summary>
        /// <param name="level">ログ出力レベル</param>
        public void SetLevel(LogLevel level)
        {
            _config.LogLevel = level;
        }


        /// <summary>
        /// ログファイルに書き込む処理を実行する。
        /// </summary>
        private void StartMonitor()
        {
            // 別スレッドで時間のかかるメイン処理を実行
            Task.Run(() =>
            {
                // 状態がクローズされていない限り、以下タスクを実行
                while (IsClose() == false)
                {
                    WriteFile();    // ログ情報をファイル書き込む

                    Task.Delay(1000);   // 1000ミリ秒待機
                }
            });
        }



        /// <summary>
        /// 状態がクローズされているかどうかを確認する。
        /// 出力途中の情報がデータとしてまだ残っている場合は、クローズ状態ではない。
        /// </summary>
        /// <returns>true:状態はクローズされている/false:状態はクローズされていない</returns>
        private bool IsClose()
        {
            if (_sb.Length > 0)
            {
                return false;   // 状態はクローズされていない
            }
            return _isClose;    // 状態はクローズされている、ログ出力を断念すべき状態
        }


        /// <summary>
        /// 全てのログ書き込みが終わるまで待機する。
        /// </summary>
        public void Close()
        {
            // タスク処理が終わるまで待機
            Info("Close");
            while (_sb.Length != 0)
            {
                // データがすべて出力し終わるまで待機する。
                Task.Delay(1000); // 1000ミリ秒待機するという仕事の完了を待ち
            }

            // タスクを新たにスタートしないようにする。
            _isClose = true;

        }
        /// <summary>
        /// デバックログを出力
        /// ※ログ出力レベルがDebugモードでなければ、呼び出しても情報出力されません。
        /// ※コンソールにも出力されます。
        /// </summary>
        /// <param name="msg">ログ出力メッセージ</param>
        /// <param name="lineNo">呼び出し元の行番号</param>
        /// <param name="methodName">呼び出し元のメソッド名</param>
        /// <param name="filePath ">呼び出し元のファイルパス</param>
        public void Debug(string msg
            , [CallerLineNumber]int lineNo = 0
            , [CallerMemberName]string methodName = ""
            , [CallerFilePath]string filePath = "")
        {
            if (_config.LogLevel == LogLevel.DEBUG)
            {
                // 出力する
                AppendLog(LogLevel.DEBUG, msg, lineNo, methodName, filePath);
            }
            else
            {
                // 出力しない
            }
        }
        /// <summary>
        /// 情報ログを出力
        /// ※ログ出力レベルがDebug,Infoモードでなければ情報出力されません。
        /// ※コンソールにも出力されます。
        /// </summary>
        /// <param name="msg">ログ出力メッセージ</param>
        /// <param name="lineNo">呼び出し元の行番号</param>
        /// <param name="methodName">呼び出し元のメソッド名</param>
        /// <param name="filePath ">呼び出し元のファイルパス</param>
        public void Info(string msg
            , [CallerLineNumber]int lineNo = 0
            , [CallerMemberName]string methodName = ""
            , [CallerFilePath]string filePath = "")
        {
            if (_config.LogLevel == LogLevel.DEBUG || _config.LogLevel == LogLevel.INFO)
            {
                // 出力する
                string outputmsg =AppendLog(LogLevel.INFO, msg, lineNo, methodName, filePath);
                System.Console.WriteLine(outputmsg);
            }
            else
            {
                // 出力しない
            }

        }
        /// <summary>
        /// 警告ログを出力
        /// ※ログ出力レベルがDebug,Info,Warnモードでなければ情報出力されません。
        /// ※コンソールにも出力されます。
        /// </summary>
        /// <param name="msg">ログ出力メッセージ</param>
        /// <param name="lineNo">呼び出し元の行番号</param>
        /// <param name="methodName">呼び出し元のメソッド名</param>
        /// <param name="filePath ">呼び出し元のファイルパス</param>
        public void Warn(string msg
            , [CallerLineNumber]int lineNo = 0
            , [CallerMemberName]string methodName = ""
            , [CallerFilePath]string filePath = "")
        {
            if (_config.LogLevel == LogLevel.DEBUG || _config.LogLevel == LogLevel.INFO || _config.LogLevel == LogLevel.WARN)
            {
                // 出力する
                string outputmsg = AppendLog(LogLevel.WARN, msg, lineNo, methodName, filePath);
                System.Console.WriteLine(outputmsg);
            }
            else
            {
                // 出力しない
            }
        }
        /// <summary>
        /// エラーログを出力
        /// ※ログ出力レベルがDebug,Info,Warn,Errorモードでなければ情報出力されません。
        /// ※コンソールにも出力されます。
        /// </summary>
        /// <param name="msg">ログ出力メッセージ</param>
        /// <param name="lineNo">呼び出し元の行番号</param>
        /// <param name="methodName">呼び出し元のメソッド名</param>
        /// <param name="filePath ">呼び出し元のファイルパス</param>
        public void Error(string msg
            , [CallerLineNumber]int lineNo = 0
            , [CallerMemberName]string methodName = ""
            , [CallerFilePath]string filePath = "")
        {
            if (_config.LogLevel != LogLevel.FATAL)
            {
                // 出力する
                string outputmsg = AppendLog(LogLevel.ERROR, msg, lineNo, methodName, filePath);
                System.Console.WriteLine(outputmsg);
            }
            else
            {
                // 出力しない
            }

        }
        /// <summary>
        /// システムにおいて致命的な死の要因となった情報を出力する
        /// ※このメソッドのログは必ず出力されます。
        /// ※コンソールにも出力されます。
        /// </summary>
        /// <param name="msg">ログ出力メッセージ</param>
        /// <param name="lineNo">呼び出し元の行番号</param>
        /// <param name="methodName">呼び出し元のメソッド名</param>
        /// <param name="filePath ">呼び出し元のファイルパス</param>
        public void Fatal(string msg
            , [CallerLineNumber]int lineNo = 0
            , [CallerMemberName]string methodName = ""
            , [CallerFilePath]string filePath = "")
        {
            string outputmsg = AppendLog(LogLevel.FATAL, msg, lineNo, methodName, filePath);
            System.Console.WriteLine(outputmsg);

        }


        /// <summary>
        /// ログ出力メッセージをフォーマットして追加
        /// </summary>
        /// <param name="loglevel">ログレベル</param>
        /// <param name="msg">ログ出力メッセージ</param>
        /// <param name="lineNo">呼び出し元の行番号</param>
        /// <param name="methodName">呼び出し元のメソッド名</param>
        /// <param name="filePath ">呼び出し元のファイルパス</param>
        private string AppendLog(LogLevel loglevel
            , string msg
            , int lineNo
            , string methodName
            , string filePath)
        {

            string outputline = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            // 現在EXEのプロセス番号を取得
            outputline += "," + System.Diagnostics.Process.GetCurrentProcess().Id;

            // 現在のスレッド番号を取得
            outputline += "," + System.Threading.Thread.CurrentThread.ManagedThreadId;

            // 現在のプロセスのメモリ使用量
            outputline += "," + Environment.WorkingSet;     // プロセスが使用しているメモリ使用量を取得する
                                                            //outputline += "," + GC.GetTotalMemory(true);    // GCを実行後に値
                                                            //outputline += "," + GC.GetTotalMemory(false);   // GCせずに現在値を取得

            //ログレベル
            outputline += "," + loglevel;
            //呼び出し元のファイル名
            outputline += "," + Path.GetFileName(filePath);
            //呼び出し元のメソッド名
            outputline += "," + methodName;
            //呼び出し元の行番号
            outputline += "," + lineNo;

            // 出力メッセージ
            outputline += "," + msg;

            if (IsClose()==false)
            {
                lock (s_lockObj)
                {
                    // 出力する
                    _sb.AppendLine(outputline);
                }
            }
            return outputline;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        private void WriteFile()
        {

            lock (s_lockObj)
            {
                if (_sb.Length == 0)
                {
                    // ログ出力対象データがない場合なにもしない。
                    return;
                }

                // ファイルパス生成

                string filename = "";
                // 接頭辞が指定されていた場合は、文字列として付与する。
                if (!"".Equals(_config.LogFileNamePrefixDateTimeString))
                {
                    filename += DateTime.Now.ToString(_config.LogFileNamePrefixDateTimeString);
                }
                // ファイル名の連結
                filename += _config.LogFileName;
                // 接尾語が指定されていた場合は、文字列として付与する。
                if (!"".Equals(_config.LogFileNameSuffixDateTimeString))
                {
                    filename += DateTime.Now.ToString(_config.LogFileNameSuffixDateTimeString);
                }
                // 拡張子文字列の連結
                filename += _config.LogFileNameExtension;
                string filePath = Path.Combine(_config.LogDirPath, filename);
                string dirPath = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                // １ファイルあたりのサイズリミットが指定されている場合
                if (_config.MaxSizeLimit > 0 && File.Exists(filePath))
                {
                    // ファイルのサイズを取得し、
                    // ファイルサイズがリミットを超えていた場合に、ファイル名をローテートリネームする
                    // (ファイルの末尾に、.1、.2・・・とつけていく）
                    FileInfo f = new FileInfo(filePath);
                    long filesize = f.Length;
                    if (_config.MaxSizeLimit < filesize)
                    {
                        // ファイル名リネーム
                        LogRotate(filePath);
                    }
                }
                try
                {
                    // ファイルにメッセージを追記
                    File.AppendAllText(filePath, _sb.ToString(), _config.LogEncoding);
                }
                catch (Exception e)
                {
                    // 例外が発生したとしても無視するが、念のためコンソールに情報を出力する。
                    System.Console.WriteLine("[LogWriteError]" + e.ToString());
                }

                // 現在の内容をリセットする
                _sb.Length = 0;
                _sb.Clear();
            }
        }

        /// <summary>
        /// ファイル名のローテート
        /// </summary>
        /// <param name="orgFilePath">オリジナルファイルパス</param>
        /// <param name="filePath">リネーム対象のファイルパス</param>
        /// <param name="index">ローテート番号</param>
        private void LogRotate(string orgFilePath, string filePath = "", int index = 1)
        {
            if ("".Equals(filePath))
            {
                filePath = orgFilePath;
            }


            if (_config.LogRotateLimitIndex < 0)
            {
                // ローテートのリミット無し
            }
            else if (_config.LogRotateLimitIndex == 0)
            {
                // ローテートのリミット未指定
                // 何もしない
                return;
            }
            else if (_config.LogRotateLimitIndex < index)
            {
                // 現在のファイルを削除する
                File.Delete(filePath);
                return;
            }

            string renameFilePath = orgFilePath + "." + index;
            if (File.Exists(renameFilePath))
            {
                // 今回リネームしたいファイルが存在する場合、更にファイル名ローテート
                LogRotate(orgFilePath, renameFilePath, (index + 1));
            }
            else
            {
                // ファイルが存在していない場合
                // リネーム
                File.Move(filePath, renameFilePath);

            }
        }


        /// <summary>
        /// 古いログ情報を削除する
        /// </summary>
        public void DeleteOldFile()
        {
            if (_config.FileDeleteDayPoint == -1)
            {
                return; //　ファイル削除しない
            }

            DateTime DelDateTime = DateTime.Now.AddDays(-_config.FileDeleteDayPoint);    // 例えば３日前以上のファイルを削除する。など

            //"C:\test"以下のファイルをすべて取得する
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(_config.LogDirPath);
            IEnumerable<System.IO.FileInfo> files =
                di.EnumerateFiles("*" + _config.LogFileName + "*", System.IO.SearchOption.AllDirectories);

            //ファイルを列挙する
            foreach (System.IO.FileInfo f in files)
            {
                if ( f.LastWriteTime < DelDateTime)
                {
                    // ファイルを削除する
                    f.Delete();
                }
            }
        }
    }
}
