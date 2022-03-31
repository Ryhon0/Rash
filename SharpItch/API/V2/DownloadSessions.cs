namespace SharpItch;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

partial class Itch
{
	public async Task<NewDownloadSessionResponse> NewDownloadSession(long gameID, long downloadKeyId = 0, string password = null, string secret = null)
	{
		return await new RequestBuilder(BaseAPIURLV2 + "games/" + gameID + "/download-sessions")
			.AddV2AcceptHeader()
			.AddV2Token(this)
			.AddLongIfNotZero("download_key_id", downloadKeyId)
			.AddStringIfNotEmpty("password", password)
			.AddStringIfNotEmpty("secret", secret)
			.Post<NewDownloadSessionResponse>(HttpClient);
	}

	public string CreateDownloadURL(long uploadID, string uuid = null, long downloadKeyId = 0, string password = null, string secret = null)
	{
		return new RequestBuilder(BaseAPIURLV2 + "uploads/" + uploadID + "/download")
			.AddV2AcceptHeader()
			.AddV2Token(this)
			.AddStringIfNotEmpty("uuid", uuid)
			.AddLongIfNotZero("download_key_id", downloadKeyId)
			.AddStringIfNotEmpty("password", password)
			.AddStringIfNotEmpty("secret", secret)
			.BuildURL();
	}
}

public class NewDownloadSessionResponse : ItchResponse
{
	[JsonPropertyName("uuid")]
	public string UUID { get; set; }
}