using System.Threading.Tasks;
using FluentCeVIOWrapper.Common;

namespace nugettest;

internal static class Program
{
	private static async Task Main()
	{
		var fcw = await FluentCeVIO.FactoryAsync();


		await fcw.StartAsync();
	}
}