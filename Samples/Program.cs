using System.Diagnostics;
using FluentCeVIOWrapper.Common;

//サーバーを外部プロセス起動
var psi = new ProcessStartInfo()
{
	FileName = Path.Combine(
		AppDomain.CurrentDomain.BaseDirectory,
		//@"Path\To\FluentCeVIOWrapper.Server.exe"
		@"..\..\..\..\FluentCeVIOWrapper.Server\bin\Release\net48\FluentCeVIOWrapper.Server.exe"
	),
	Arguments = "-cevio CeVIO_AI",
};
var process = Process.Start(psi);
if(process is null)
{
	return;
}

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
	.ToList()
	.AsReadOnly();
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

//サーバー終了
process.Kill();