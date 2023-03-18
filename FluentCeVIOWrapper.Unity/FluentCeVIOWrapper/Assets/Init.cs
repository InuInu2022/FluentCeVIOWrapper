using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using FluentCeVIOWrapper.Common;

using UnityEngine;

public class Init : MonoBehaviour
{
	private Process process;

	// Start is called before the first frame update
	void Start()
  {
    UnityEngine.Debug.Log("Started.");
		var _ = AwakeServer();
		_ = AwakeAsync();
  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnApplicationQuit()
  {
		process.Kill();
		process.WaitForExit(500);
  }

  async Task AwakeServer()
  {
		var assets = Application.dataPath;
		var server = "StreamingAssets/FluentCeVIOWrapper.Server/FluentCeVIOWrapper.Server.exe";
		var path = System.IO.Path.Combine(assets, server);

		var psi = new System.Diagnostics.ProcessStartInfo()
    {
      FileName = path,
      Arguments = "-cevio CeVIO_AI",
      //サーバーのコンソールを出さない設定は以下2行が必要
      CreateNoWindow = true,
      UseShellExecute = false
    };
    process = System.Diagnostics.Process.Start(psi);
		await Task.Delay(1000);
	}

  async Task AwakeAsync()
  {
    using var fcw = await FluentCeVIO
      .FactoryAsync();
    UnityEngine.Debug.Log("Awaked.");
    UnityEngine.Debug.Log($"fcw pipe: {FluentCeVIO.PipeName}");
    UnityEngine.Debug.Log($"product:{fcw.CurrentProduct}");

		var result = await fcw.StartAsync();
		UnityEngine.Debug.Log($"result:{result}");

		await fcw.SpeakAsync("こんにちは！");
	}
}
