namespace SharpItch;
using System.Text.Json.Serialization;

partial class Itch
{
	public async Task<OwnedKeysResult> GetOwnedKeys(long page = 1)
	{
		var okr = await new RequestBuilder(BaseAPIURLV2 + "profile/owned-keys")
			.AddV2AcceptHeader()
			.AddV2Token(this)
			.AddLong("page", page)
			.Get<OwnedKeysResult>(HttpClient);

		return okr;
	}
}

public class OwnedKeysResult : ItchResponse
{
	[JsonPropertyName("page")]
	public int Page { get; set; }
	[JsonPropertyName("per_page")]
	public int PerPage { get; set; }
	[JsonPropertyName("owned_keys")]
	public List<OwnedKey> OwnedKeys { get; set; }
}

public class OwnedKey
{
	[JsonPropertyName("game")]
	public Game Game { get; set; }
	[JsonPropertyName("id")]
	public long ID { get; set; }
	[JsonPropertyName("game_id")]
	public long GameID { get; set; }
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAt { get; set; }
	[JsonPropertyName("created_at")]
	public DateTime Created { get; set; }
	[JsonPropertyName("downloads")]
	public long Downloads { get; set; }
}