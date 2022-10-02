# Fluent CeVIO Wrapper

A named pipe server of CeVIO API for .NET 6 / .NET Standard 2.0

# What's this?

音声合成ソフト「[CeVIO](https://cevio.jp/)」の .NET外部連携インターフェイスを 最新の .NET 6からも扱えるようにしたラッパーライブラリ＆連携サーバーです。async/await, ValueTask, nullableなどモダンな書き方に対応しています。

## 特徴

- CeVIO AI, CeVIO Creative Studio 7 対応
- 共通ライブラリAPIはモダンな記法が可能
  - async / await
  - nullable
  - `ValueTask<T>`
  - C# 10
  - No more GAC、nupkg形式での提供
- 共通ライブラリは .NET Standard 2.0対応
  - .NET Framework系環境・.NET Core系環境どちらかも利用可能
- 連携サーバーは .NET Framework 4.8上で起動

## 構成

- [FluentCeVIOWrapper.Common](FluentCeVIOWrapper.Common/)
  - 共通ライブラリ
  - .NET Standard 2.0
  - .nupkg
- [FluentCeVIOWrapper.Server](FluentCeVIOWrapper.Server/)
  - 連携IPCサーバー
  - .NET Framework 4.8
  - Windows console app .exe

## 使い方

### FluentCeVIOWrapper.Common

1. nupkgファイルをDL
2. nupkgをローカルnugetリポジトリに登録
3. ライブラリとして追加。例：`dotnet add package FluentCeVIOWrapper.Common`

```cs
using FluentCeVIOWrapper.Common;

//サーバーを外部プロセス起動
var psi = new ProcessStartInfo()
{
	FileName = Path.Combine(
		AppDomain.CurrentDomain.BaseDirectory,
		@"Path\To\FluentCeVIOWrapper.Server.exe"
	),
	Arguments = $"-cevio CeVIO_AI",
};
var process = Process.Start(psi);

//ファクトリメソッドで非同期生成
//IDisposeを継承しているためusingが使えます
using var fcw = await FluentCeVIO.FactoryAsync();

//非同期でCeVIO外部連携インターフェイス起動
await fcw.StartAsync();
//利用可能なキャスト（ボイス）を非同期で取得
var casts = await fcw.GetAvailableCastsAsync();
//キャストを非同期で指定
await fcw.SetCastAsync(casts[0]);
//非同期で音声合成
await fcw.SpeakAsync("こんにちは。");
```

### FluentCeVIOWrapper.Server

1. exeファイルをDL
2. `Process.Start()`などで外部プロセス呼び出し
3. サーバー起動後は`FluentCeVIOWrapper.Common.FluentCeVIO`クラスで通信が可能です

- 起動オプション
  - `-help` : ヘルプ表示
  - `-cevio` : `CeVIO_AI` or `CeVIO_CS`
  - `-pipeName` : IPCで使われる名前付きパイプ名。複数起動時に設定します。
  - `-dllPath` : CeVIOのインストールフォルダパス指定


## ライブラリ

- H.Pipes
- System.Reactive
- ConsoleAppFramework
- Microsoft.Extensions.Hosting.Abstractions
- xunit