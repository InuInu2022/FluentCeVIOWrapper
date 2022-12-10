# Introduction to Fluent CeVIO Wrapper

## What's this

音声合成ソフト「**[CeVIO](https://cevio.jp/)**」の [.NET外部連携インターフェイス](https://cevio.jp/guide/cevio_ai/interface/dotnet/)を 最新の .NET 7等からも扱えるようにしたラッパーライブラリ＆連携サーバーです。.NET Framework 4.8以外むけの.NETアプリから利用できるようになります。また、`async`/`await`, `ValueTask`, `nullable`などモダンな書き方に対応しています。

A wrapper library and integration IPC server for the [.NET external integration interface](https://cevio.jp/guide/cevio_ai/interface/dotnet/) of the speech synthesis software "**[CeVIO](https://cevio.jp/)**", which can be used from the latest .NET 7 and other .NET Framework 4.8 environments. It also supports modern C# writing style such as `async`/`await`, `ValueTask`, `nullable`, and so on.

## Problems of the CeVIO .NET interface

### #1 .NET Framework 4.8

「[CeVIO](https://cevio.jp/)」の [.NET外部連携インターフェイス](https://cevio.jp/guide/cevio_ai/interface/dotnet/) は .NET Framework 4.8向けのもので、**最新の.NET環境（.NET 6～）から利用することができません。**

そのため外部連携インターフェイスを利用する.NETアプリは、古い .NET Framework 4.8むけに作らなくてはなりませんでした…。

最新の.NET環境はマルチプラットホーム対応で、高速、C#の新しい文法が使えるなど、どうしても使いたい…！

🎉そこで「**Fluent CeVIO Wrapper**」の登場です！

外部連携インターフェイスとの通信部分をIPCサーバーである **FluentCeVIOWrapper.Server** が行います。
アプリは **FluentCeVIOWrapper.Common** ライブラリを組み込むだけで、IPCサーバー経由で外部連携インターフェイスを使用できます。

FluentCeVIOWrapper.Commonは [.NET Standard 2.0](https://learn.microsoft.com/ja-jp/dotnet/standard/net-standard?tabs=net-standard-2-0) で書かれている為、
古い .NET Framework 環境、最新の .NET 6～ 環境どちらからも利用できます。

**アプリ開発者は最新の .NET環境でアプリを組むことができます**！🎉やったね！！！

### #2. Not supported `async`/`await`

「[CeVIO](https://cevio.jp/)」の [.NET外部連携インターフェイス](https://cevio.jp/guide/cevio_ai/interface/dotnet/) は C#の標準の非同期処理である`async`/`await`に対応していません。

内部で通信を行っていたり、AIになってからは合成自体に時間がかかるようになったため、同期呼び出しだとプチフリーズすることがよくありました…。

標準の非同期処理を使うために`await Task.Run()`で処理をいちいち囲む必要があってかなりツラい…。

🎉そこで「**Fluent CeVIO Wrapper**」の登場です！

基本的にすべて、`async`/`await`に対応しています。
可能であれば戻り値は`ValueTask<T>`になるようにしています。
元の外部連携インターフェイスでC#のフィールドになっていて非同期処理ができないAPIは非同期のメソッドに置き換えられています。
詳しくはAPI documentの`FluentCeVIO`クラスをご覧ください！！

### #3. Different name API (CeVIO CS / CeVIO AI)

CeVIO Creative StudioむけとCeVIO AIむけの外部連携インターフェイスはほぼ同じであるものの、クラス名が`Talker`と`Talker2`など微妙に異なり、両方のソフトに対応する際に違いを吸収するのが大変でした。

🎉そこで「**Fluent CeVIO Wrapper**」の登場です！

サーバー起動時のオプションを変えるだけで
共通のAPIでCeVIO CSもAIも呼び出すことができます。
（同時につかうにはサーバーを複数起動する必要があります）
**アプリ側ではCSとAIの違いをほとんど意識しないで組むことができます！**

### #4. Old style C# 7.3

CeVIOの外部連携インターフェイスは 古い .NET Framework 環境のため、そのままC#で書こうとすると C# 7.3 になっていました。

最新のC#に比べると…
- `namespace` のインデントが多い！！！！
- `using`が省略できない！！！！
- 数行のプログラムでもクラスや`Main`関数が省略できない！！！
- `nullable`が有効じゃなくてバグが入りやすい！！！！
- `switch`式が使えない！！！！
- `is`は使えても`is not`が使えない！！
- `new()`って書けない！
- `record`型が作れない！！

🎉そこで「**Fluent CeVIO Wrapper**」の登場です！

アプリ側は気にせず、最新のSDKと最新のC#で書けます！！
※Fluent CeVIO Wrapper自体もC#10で書かれています。

## Sample code

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

[Samples](https://github.com/InuInu2022/FluentCeVIOWrapper/tree/main/Samples)
