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
    /// （<b>CeVIO AIトークボイス専用</b>）
    /// </summary>
    /// <param name="fcw"></param>
    /// <param name="castName">キャスト名。未指定の場合は現在のキャストで取得。</param>
    /// <returns>内部的なキャストID文字列（CTNV-xxx-x）</returns>
    /// <exception cref="NotSupportedException"><see cref="FluentCeVIO.CurrentProduct"/>が<see cref="Product.CeVIO_AI"/>でない場合は未対応</exception>
    public static async Task<string> GetCastIdAsync(
        this FluentCeVIO fcw,
        string? castName = null
    )
    {
        if(fcw.CurrentProduct is Product.CeVIO_CS){
			throw new NotSupportedException("CeVIO AI以外はサポートしていません。");
		}

        if(castName is not null){
			await fcw.SetCastAsync(castName);
		}

		var comps = await fcw.GetComponentsAsync();
		return string
			.Join(
				"-",
				comps[0]
					.Id
					.Split('-')
					.Take(3)
			);
	}
}