using System.Reflection;
using System;
using System.Collections.Generic;

using Ceras;

using FluentCeVIOWrapper.Common.Talk;
using System.Linq;

namespace FluentCeVIOWrapper.Common.Models;

/// <summary>
/// FCW formatter
/// </summary>
public class FCWFormatter : H.Formatters.CerasFormatter
{
	/// <summary>
	/// 内部フォーマッター
	/// </summary>
    public new CerasSerializer InternalFormatter { get; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="config"></param>
	public FCWFormatter(SerializerConfig? config = null) :base()
	{
		if(config is null){
			config = new SerializerConfig();
			//config.Advanced.PersistTypeCache = true;
			//やり取りする型をここで設定
			config
				.ConfigType<TalkerComponent>()
				/*.ConfigProperty(nameof(TalkerComponent.Id)).Include()
				.ConfigProperty(nameof(TalkerComponent.Name)).Include()
				.ConfigProperty(nameof(TalkerComponent.Value)).Include()*/
				.ConstructBy(
					()=>new TalkerComponent("","",0)
				);
			config.ConfigType<PhonemeData>()
				.ConstructBy(() => new PhonemeData(0.0,0.0,""));
		}
		InternalFormatter = new(config);
	}

	/// <summary>
	/// シリアライズ
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	protected override byte[] SerializeInternal(object obj)
    {
        return InternalFormatter.Serialize(obj);
    }


	/// <summary>
	/// デシリアライズ
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="bytes"></param>
	/// <returns></returns>
    protected override T DeserializeInternal<T>(byte[] bytes)
    {
        return InternalFormatter.Deserialize<T>(bytes);
    }
}
