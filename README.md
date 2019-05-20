# LogManagerNet
[忘備録] C# .NET Net 4.6 系アプリ開発時のファイルログを出力する用ライブラリです。<br>

# Dependency
- .NET Framework 4.6.1

# Setup
当プロジェクトをビルドして生成されるLogManagerNet.dllを開発で利用したい.NET環境に格納してください。<br>
ログ出力が必要なクラス上で`using LogManagerNet;`と記載してください。<br>
log.configファイルをプロジェクトに追加してください。<br>
log.configの内容をお使いの環境に合わせて変更してください。<br>


■ログ出力設定値<br>
- LogLevel : ログレベルです。
  - DEBUG=全てのログを出力する
  - INFO=INFO(情報）以上のログを出力する（INFO、WARN、ERROR、FATAL）
  - WARN=WARN（警告）レベル以上のログを出力する。（WARN、ERROR、FATAL）
  - ERROR=ERROR(エラー)レベル以上のログを出力する（ERROR、FATAL）
  - FATAL=FATAL(致命的なエラー)しかログに出力しない
- MaxSizeLimitMB : 1ファイル当たりのサイズリミット MB( 0:リミット無し / 0越え:リミットあり )
- LogRotateLimitIndex : 1ファイル当たりのローテーションリミット ( 0:ローテートのリミットなし / 0越え:指定個数分ファイルをローテートする ）
- Encoding : ログファイルの出力文字コード※Encoding.GetEncoding(文字列)で認識できる文字コードのみ対応
- LogDirPath : ログファイル出力ディレクトリパス
- LogFileName : ファイル名キーワード
- LogFileNameExtension : ログファイル拡張子
- LogFileNamePrefixDateTimeString : ファイル名接頭辞
- LogFileNameSuffixDateTimeString : ファイル名接尾語


<br>
<br>
■ログ出力ライブラリの使い方<br>

log.configの内容<br>

```<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="LogLevel" value="DEBUG" />
    <add key="MaxSizeLimitMB" value="1" />
    <add key="LogRotateLimitIndex" value="0" />
    <add key="Encoding" value="shift_jis" />
    <add key="LogDirPath" value="D:\temp\C#" />
    <add key="LogFileName" value="_debug" />
    <add key="LogFileNameExtension" value=".log" />
    <add key="LogFileNamePrefixDateTimeString" value="yyyyMMdd" />
    <add key="LogFileNameSuffixDateTimeString" value="" />
  </appSettings>
</configuration>
```

ログ出力をしたいクラスのコード上にて<br>

```// ログファイルのインスタンスを取得する。
LogManager logger = LogManager.GetInstance();
logger.Debug("テスト");// Debugログとして書き込む
logger.Info("テスト");// Infoログとして書き込む
logger.Warn("テスト");// Warnログとして書き込む
logger.Error("テスト");// Errorログとして書き込む
logger.Fatal("テスト");// Fatalログとして書き込む
```
<br>
<br>

※サンプルのログ設定ファイル上の、ログレベルがDEBUGなのですべてのログが出力されますが、
コーディングを変えずログ出力レベルを変更したい場合に対応できるようログレベルを用意しています。<br>
また、1MBを超えると自動的にローテートして、以下のように古いファイルほど、大きい数字が付くようにファイルが切り替わっていきます。<br>

```20181205_debug.log
20181205_debug.log.1
20181205_debug.log.2
20181205_debug.log.3
20181205_debug.log.4
20181205_debug.log.5
20181205_debug.log.6
```

<br>
<br>
なお複数のログ出力設定に合わせて動かすことができるようログ設定ファイル名のつけ方にはルールがあります。<br>
基本的なログ設定ファイル名称は、`log.config`としてますが、`log.キーワード.config`という名前の付け方も可能です。<br>
例）<br>
log.type2.config<br>
```<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="LogLevel" value="ERROR" />
    <add key="MaxSizeLimitMB" value="0" />
    <add key="LogRotateLimitIndex" value="1" />
    <add key="Encoding" value="shift_jis" />
    <add key="LogDirPath" value="..\..\..\bin\logs" />
    <add key="LogFileName" value="app" />
    <add key="LogFileNameExtension" value=".log" />
    <add key="LogFileNamePrefixDateTimeString" value="yyyyMM/dd_" />
    <add key="LogFileNameSuffixDateTimeString" value="_error" />
  </appSettings>
</configuration>
```

```// ログファイルのインスタンスを取得する。
LogManager logger3 = LogManager.GetInstance("type2"); // log.type2.configを読み込む。第１引数はログファイルのキーワード
logger3.Debug("テスト");// Debugログとして書き込む
logger3.Info("テスト");// Infoログとして書き込む
logger3.Warn("テスト");// Warnログとして書き込む
logger3.Error("テスト");// Errorログとして書き込む
logger3.Fatal("テスト");// Fatalログとして書き込む
```

<br>
※設定ファイルの例として以下フォルダ に、設定ファイルのサンプルとその実行結果を入れております。<br>

[log.config.samples](/log.config.samples/)

 - log.config → logs\yyyyMMdd_debug.log<br>
 - log.type1.config → logs\warn_yyyyMMdd_.log<br>
 - log.type2.config → logs\yyyyMMdd\dd_app_error.log<br>


# License
[MIT License](/LICENSE)

# Authors
hanamizuki10

# References
- 特になし
