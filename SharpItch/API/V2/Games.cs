namespace SharpItch;
using System.Text.Json.Serialization;

partial class Itch
{
	public async Task<GamesResult> GetGame(long gameID)
	{
		return await new RequestBuilder(BaseAPIURLV2 + "games/" + gameID)
			.AddV2AcceptHeader()
			.AddItchCookie(ItchCookie)
			.Get<GamesResult>(HttpClient);
	}
}

public class GamesResult : ItchResponse
{
	[JsonPropertyName("game")]
	public Game Game { get; set; }
}