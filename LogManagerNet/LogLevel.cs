using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManagerNet
{
    /// <summary>
    /// ログレベル列挙型
    /// ログイベントを発生させるレベル
    /// DEBUG が最も低いレベルで、FATAL が最高のレベルということになります。
    /// DEBUG　＜　INFO　＜　WARN　＜　ERROR　＜　FATAL
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 全てのログを出力する
        /// </summary>
        DEBUG = 1,
        /// <summary>
        /// INFO(情報）以上のログを出力する
        /// （INFO、WARN、ERROR、FATAL）
        /// </summary>
        INFO,
        /// <summary>
        /// WARN（警告）レベル以上のログを出力する。
        /// （WARN、ERROR、FATAL）
        /// </summary>
        WARN,
        /// <summary>
        /// ERROR(エラー)レベル以上のログを出力する
        /// （ERROR、FATAL）
        /// </summary>
        ERROR,
        /// <summary>
        /// FATAL(致命的なエラー)しかログに出力しない
        /// </summary>
        FATAL
    }
}
