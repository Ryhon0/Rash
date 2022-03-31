namespace SharpItch;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

partial class Itch
{
	public async Task<ScannedArchiveResponse> GetUploadScannedArchive(long uploadID, long downloadKeyId = 0, string password = null, string secret = null)
	{
		return await new RequestBuilder(BaseAPIURLV2 + "uploads/" + uploadID + "/scanned-archive")
			.AddV2AcceptHeader()
			.AddV2Token(this)
			.AddLongIfNotZero("download_key_id", downloadKeyId)
			.AddStringIfNotEmpty("password", password)
			.AddStringIfNotEmpty("secret", secret)
			.Get<ScannedArchiveResponse>(HttpClient);
	}
}

public class ScannedArchiveResponse : ItchResponse
{
	[JsonPropertyName("scanned_archive")]
	public ScannedArchive ScannedArchive { get; set; }
}

public class ScannedArchive
{
	[JsonPropertyName("object_id")]
	public long ObjectID { get; set; }
	/// <summary>
	/// upload/build
	/// </summary>
	[JsonPropertyName("object_type")]
	public string ObjectType { get; set; }
	[JsonPropertyName("extracted_size")]
	public long? ExctractedSize { get; set; }
	[JsonPropertyName("launch_targets")]
	public List<JsonObject> LaunchTargets { get; set; }
	[JsonPropertyName("manifest")]
	public JsonObject Manifest { get; set; }
}