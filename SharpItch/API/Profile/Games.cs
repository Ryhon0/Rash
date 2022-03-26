using System.Text.Json.Serialization;

namespace SharpItch;

partial class Itch
{
	/// <summary>
	/// Returns a list of games you have created, not your purchases.
	/// See Itch.GetOwnedKeys for your purchases.
	/// </summary>
	public async Task<ItchGamesResponse> GetGames()
	{
		var res = await new RequestBuilder(BaseAPIPath + "profile/games")
			.Get<ItchGamesResponse>(HttpClient);

		foreach(var g in res.Games)
			g.ProcessExtensionData();

		return res;
	}
}

public class ItchGamesResponse : ItchResponse
{
	[JsonPropertyName("games")]
	public List<Game> Games { get; set; }
}
