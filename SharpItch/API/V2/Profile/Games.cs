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
		var res = await new RequestBuilder(BaseAPIURLV2 + "profile/games")
			.AddV2AcceptHeader()
			.AddItchCookie(ItchCookie)
			.Get<ItchGamesResponse>(HttpClient);

		return res;
	}
}

public class ItchGamesResponse : ItchResponse
{
	[JsonPropertyName("games")]
	public List<MyGame> Games { get; set; }
}

public class MyGame : Game
{
	[JsonPropertyName("views_count")]
	public long ViewsCount { get; set; }
	[JsonPropertyName("downloads_count")]
	public long DownloadsCount { get; set; }
	[JsonPropertyName("purchases_count")]
	public long PurchasesCount { get; set; }
	[JsonPropertyName("published")]
	public bool Published { get; set; } = true;
	[JsonPropertyName("earnings")]
	public List<Earning> Earnings { get; set; }
}

public class Earning
{
	[JsonPropertyName("amount")]
	public long Amount { get; set; }
	[JsonPropertyName("currency")]
	public string Currency { get; set; }
	[JsonPropertyName("amount_formatted")]
	public string AmountFormatted { get; set; }
}