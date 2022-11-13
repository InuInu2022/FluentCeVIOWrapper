# Fluent CeVIO Wrapper

A named pipe server of CeVIO API for .NET 6 / .NET Standard 2.0

# What's this?

音声合成ソフト「[CeVIO](https://cevio.jp/)」の .NET外部連携インターフェイスを 最新の .NET 6からも扱えるようにしたラッパーライブラリ＆連携サーバーです。.NET Framework 4.8以外むけの.NETアプリから利用できるようになります。また、`async`/`await`, `ValueTask`, `nullable`などモダンな書き方に対応しています。

## 特徴

- CeVIO AI, CeVIO Creative Studio 7 対応
- 共通ライブラリAPIはモダンな記法が可能
  - `async` / `await`
  - `nullable`
  - `ValueTask<T>`
  - C# 10
- nuget経由での導入
  - No more GAC、nupkg形式での提供
  - 現在はローカルnugetの想定です
- 共通ライブラリは .NET Standard 2.0対応
  - .NET Framework系環境・.NET Core系環境どちらかも利用可能
- 連携サーバーは .NET Framework 4.8上で起動
- **バグだらけ。テスト甘いです。**
  - 利用していないAPIはテストされていません

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
   1. download from [Releases](https://github.com/InuInu2022/FluentCeVIOWrapper/releases)
2. nupkgをローカルnugetリポジトリに登録
3. ライブラリとして追加。
   1. 例：`dotnet add package FluentCeVIOWrapper.Common`

```cs
//ファクトリメソッドで非同期生成
//IDisposableを継承しているためusingが使えます
using var fcw = await FluentCeVIO.FactoryAsync();

//非同期でCeVIO外部連携インターフェイス起動
await fcw.StartAsync();
//利用可能なキャスト（ボイス）を非同期で取得
var casts = await fcw.GetAvailableCastsAsync();
//感情一覧を非同期で取得
var emotes = await fcw.GetComponentsAsync();
var newEmo = emotes
	.Select(v => {
		v.Value = (v.Name == "哀しみ") ?
			(uint)100 :
			(uint)0;
		return v;
	})
	.ToList();
//メソッドチェーンでまとめてパラメータ指定
await fcw.CreateParam()
	.Cast(casts[0])
	.Alpha(30)
	.Speed(50)
	.ToneScale(75)
	.Components(newEmo)
	.SendAsync();
//非同期で音声合成
await fcw.SpeakAsync("こんにちは。");

//感情設定は Emotions() で簡単にできる
await fcw.CreateParam()
  //キャスト名の直接指定でも実はOK
	.Cast("さとうささら")
	//感情一覧を取得しなくても使える便利関数
	//感情名が一致すれば設定します。存在しない場合は無視
	.Emotions(new()
		{
			["元気"] = 0,
			["哀しみ"] = 0,
			["怒り"] = 75,
			["普通"] = 50
		})
	.SendAsync();
await fcw.SpeakAsync("こんにちは!!");
```

### FluentCeVIOWrapper.Server

1. exeファイルをDL
   1. download from [Releases](https://github.com/InuInu2022/FluentCeVIOWrapper/releases)
2. `Process.Start()`などで外部プロセス呼び出し
3. サーバー起動後は`FluentCeVIOWrapper.Common.FluentCeVIO`クラスで通信が可能です

- 起動オプション
  - `-help` : ヘルプ表示
  - `-cevio` : `CeVIO_AI` or `CeVIO_CS`
  - `-pipeName` : IPCで使われる名前付きパイプ名。複数起動時に設定します。
  - `-dllPath` : CeVIOのインストールフォルダパス指定

CeVIO AIとCeVIO Creative Studioに同時に通信する場合、サーバーを2つ立ち上げてください。

## 使用ライブラリ

- [H.Pipes](https://github.com/HavenDV/H.Pipes)
- [System.Reactive](https://github.com/dotnet/reactive)
- [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework)
- [Microsoft.Extensions.Hosting.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Abstractions/)
- [System.ComponentModel.Annotations](https://www.nuget.org/packages/System.ComponentModel.Annotations/)
- [xunit](https://xunit.net/)

## ライセンス

> MIT License
>
> Copyright (c) 2022 いぬいぬ

See detail [LICENSE](.LICENSE)

## 🐶Developed by InuInu

- InuInu（いぬいぬ）
  - YouTube [YouTube](https://bit.ly/InuInuMusic)
  - Twitter [@InuInuGames](https://twitter.com/InuInuGames)
  - Blog [note.com](https://note.com/inuinu_)
  - niconico [niconico](https://nico.ms/user/98013232)
