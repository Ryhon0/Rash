namespace SharpItch;
using System.Text.Json.Serialization;

partial class Itch
{
	public async Task<GamesResult> GetGame(long gameID)
	{
		return await new RequestBuilder(BaseAPIURLV2 + "games/" + gameID)
			.AddV2AcceptHeader()
			.AddV2Token(this)
			.Get<GamesResult>(HttpClient);
	}
}

public class GamesResult : ItchResponse
{
	[JsonPropertyName("game")]
	public Game Game { get; set; }
}