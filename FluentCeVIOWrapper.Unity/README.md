# Sample for Unity

## Build

### 手動でやるばあい

#### Serverの準備

`Assets/StreamingAssets/`以下に`FluentCeVIOWrapper.Server`のファイルを展開してください。

※`net48`フォルダ以下を丸ごとコピー

#### ライブラリの準備

最新の`FluentCeVIOWrapper.Common`を [github releases](https://github.com/InuInu2022/FluentCeVIOWrapper/releases) からDL、nupkgを拡張子をzipに変えて展開して中身のファイルを`Assets`以下の適当なフォルダに展開してください。

※このサンプルプロジェクトではその後`Packages`以下に移動しています。

`FluentCeVIOWrapper.Common`が依存しているライブラリはそのままでは取り込まれないので、その場合は
[NuGet importer for Unity](https://github.com/kumaS-nu/NuGet-importer-for-Unity) で [H.Pipes](https://github.com/HavenDV/H.Pipes) をインストールしてください。

### unitypackage

TODO

### UPM

TODO

#### Serverの用意

手動でやる場合と同じ方法で用意してください。

#### ライブラリの用意

UPMをつかって導入します。
