# Fluent CeVIO Wrapper

[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE) [![C Sharp 10](https://img.shields.io/badge/C%20Sharp-10-4FC08D.svg?logo=csharp&style=flat)](https://learn.microsoft.com/ja-jp/dotnet/csharp/) ![.NET Standard 2.0](https://img.shields.io/badge/%20.NET%20Standard-2.0-blue.svg?logo=dotnet&style=flat) ![.NET Framework 4.8](https://img.shields.io/badge/%20.NET%20Framework-4.8-blue.svg?logo=dotnet&style=flat)
![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/inuinu2022/fluentceviowrapper?include_prereleases&label=%F0%9F%9A%80release)
[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.3-lightgray.svg?logo=&style=flat)](https://cevio.jp/)

A wrapper library and integration IPC server of the [CeVIO](https://cevio.jp/) API for .NET 7 / .NET Standard 2.0

## What's this?

音声合成ソフト「**[CeVIO](https://cevio.jp/)**」の [.NET外部連携インターフェイス](https://cevio.jp/guide/cevio_ai/interface/dotnet/)を 最新の .NET 7等からも扱えるようにしたラッパーライブラリ＆連携サーバーです。.NET Framework 4.8以外むけの.NETアプリから利用できるようになります。また、`async`/`await`, `ValueTask`, `nullable`などモダンな書き方に対応しています。

A wrapper library and integration IPC server for the [.NET external integration interface](https://cevio.jp/guide/cevio_ai/interface/dotnet/) of the speech synthesis software "**[CeVIO](https://cevio.jp/)**", which can be used from the latest .NET 7 and other .NET Framework 4.8 environments. It also supports modern C# writing style such as `async`/`await`, `ValueTask`, `nullable`, and so on.

## 特徴 / Features

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
  - .NET Framework系環境・.NET Core系環境どちらからも利用可能
  - .NET 6 / 7での動作を確認済
- 連携IPCサーバーは .NET Framework 4.8上で起動
  - 名前付きパイプでのIPCを行います
- Unity3D対応
  - `.unitypackage` を用意しています
- **バグだらけ。テスト甘いです。**
  - 利用していないAPIはテストされていません

## 構成

- [FluentCeVIOWrapper.Common](FluentCeVIOWrapper.Common/)
  - 共通ライブラリ
  - .NET Standard 2.0 ![.NET Standard 2.0](https://img.shields.io/badge/%20.NET%20Standard-2.0-blue.svg?logo=dotnet&style=flat)
  - `.nupkg`
- [FluentCeVIOWrapper.Server](FluentCeVIOWrapper.Server/)
  - 連携IPCサーバー
  - .NET Framework 4.8 ![.NET Framework 4.8](https://img.shields.io/badge/%20.NET%20Framework-4.8-blue.svg?logo=dotnet&style=flat)
  - Windows console app .exe
- [FluentCeVIOWrapper.Unity](FluentCeVIOWrapper.Unity/)
  - Unity3D上で動かします
  - Unity3D 2021.3で動作確認
    - .NET Standard 2.0 をサポートするバージョンなら動くと思います
  - Windowsのみ
  - `.unitypackage`

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

- [API documents](https://inuinu2022.github.io/FluentCeVIOWrapper/)

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

### FluentCeVIOWrapper.Unity

unitypackageを[Releases](https://github.com/InuInu2022/FluentCeVIOWrapper/releases)からDLして取り込むだけです。

see [README](./FluentCeVIOWrapper.Unity/README.md)

`Assets/Init.cs`は実際の呼び出しスクリプトです。

### 使用例 / Examples

- **[Samples](./Samples/)**
  - C#10で書かれたサンプルのC#コンソールアプリケーションです
  - キャスト名(`さとうささら`)は所持済みのボイスライブラリ名に書き換えてください
    - 未所持の場合は動きません
- **[KuchiPaku](https://github.com/InuInu2022/KuchiPaku)**
  - YMM4（ゆっくりムービーメーカー4）むけの「あいうえお口パク（リップシンク）」を生成するツールです。
  - CeVIO API連携ボイスの口パク生成に Fluent CeVIO Wrapper を利用しています
  - このアプリ自体は .NET 6 向けに作られており、.NET Framework 向けであるCeVIOの外部連携インターフェイスをそのままでは利用できません。Fluent CeVIO Wrapper を使用することによりCeVIOを呼び出すことを実現しています。
- **[SasaraUtil](https://github.com/InuInu2022/SasaraUtil)**
  - CeVIOのあれこれを便利するユーティリティアプリ
  - 下記のボイパロイド機能を移植してUIを付けています
- **[LibSasara/Sample/SongToTalk](https://github.com/InuInu2022/LibSasara/tree/master/sample/csharp/SongToTalk)**
  - CeVIOソングのCCS/CCSTファイルからCeVIOトークの台詞を並べてボイパロイドするための簡易的なツールです
  - [LibSasara](https://github.com/InuInu2022/LibSasara/)と連携するサンプルにもなっています
- **[VRM_AI (fork edition)](https://github.com/InuInu2022/VRM_AI)**
  - ChatGPT/Whisper/VRMをつかってAITuberが簡単に作れる **[VRM_AI](https://note.com/tori29umai/n/n81f3dd2343f3)** をCeVIOに独自対応したfork版です
  - 公式で対応していないボイスの感情表現に対応しています

## 使用ライブラリ

- [H.Pipes](https://github.com/HavenDV/H.Pipes)
- [System.Reactive](https://github.com/dotnet/reactive)
- [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework)
- [Microsoft.Extensions.Hosting.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Abstractions/)
- [System.ComponentModel.Annotations](https://www.nuget.org/packages/System.ComponentModel.Annotations/)
- [xunit](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [MinVer](https://github.com/adamralph/minver)

## ライセンス

> MIT License
>
> Copyright (c) 2022 - 2023 いぬいぬ

See detail [LICENSE](./LICENSE)

## 🐶Developed by InuInu

- InuInu（いぬいぬ）
  - YouTube [YouTube](https://bit.ly/InuInuMusic)
  - Twitter [@InuInuGames](https://twitter.com/InuInuGames)
  - Blog [note.com](https://note.com/inuinu_)
  - niconico [niconico](https://nico.ms/user/98013232)
