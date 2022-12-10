using System;
using System.Linq;
using System.Threading.Tasks;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// ユーティリティクラス
/// </summary>
public static class FluentCeVIOUtil
{
    /// <summary>
    /// 現在のキャストまたは指定したキャスト名からキャストIDを取得
    /// </summary>
    /// <param name="fcw"></param>
    /// <param name="castName">キャスト名。未指定の場合は現在のキャストで取得。</param>
    /// <returns>内部的なキャストID文字列（CeVIO AI:<c>CTNV-xxx-x</c>, CeVIO CS: <c>X</c>）</returns>
    /// <exception cref="NotSupportedException"><see cref="FluentCeVIO.CurrentProduct"/>が<see cref="Product.CeVIO_AI"/>, <see cref="Product.CeVIO_CS"/>でない場合は未対応</exception>
    public static async Task<string> GetCastIdAsync(
        this FluentCeVIO fcw,
        string? castName = null
    )
    {
        if(fcw.CurrentProduct is not Product.CeVIO_CS or Product.CeVIO_AI){
			throw new NotSupportedException("CeVIO AI, CeVIO CS以外はサポートしていません。");
		}

        if(castName is not null){
			await fcw.SetCastAsync(castName);
		}

		var comps = await fcw.GetComponentsAsync();
		return fcw.CurrentProduct switch
		{
			Product.CeVIO_AI =>
			string
				.Join(
					"-",
					comps[0]
						.Id
						.Split('-')
						.Take(3)
					),
			Product.CeVIO_CS =>
			string.Join(
				"-",
				comps[0]
					.Id
					.Split('-')[0]
			),
			_ => ""
        }
        ;
	}
}