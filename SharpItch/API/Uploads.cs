namespace SharpItch;

using System.Text.Json;
using System.Text.Json.Serialization;

partial class Itch
{
	/// <summary>
	/// Returns a single upload by it's ID or the Game ID if it has only one upload.
	/// </summary>
	/// <param name="uploadID"></param>
	/// <returns></returns>
	public async Task<UploadResult> GetUpload(long uploadID, long downloadKeyId = 0, string password = null, string secret = null)
	{
		var ur = await new RequestBuilder(BaseAPIPath + "uploads/" + uploadID)
			.AddLongIfNotZero("download_key_id", downloadKeyId)
			.AddStringIfNotEmpty("password", password)
			.AddStringIfNotEmpty("secret", secret)
			.Get<UploadResult>(HttpClient);

		var u = ur.Upload;
		u.Platforms = PlatformHelper.FromTraits(u.ExtensionData);
		var traits = u.ExtensionData["traits"];
		if (traits.ValueKind == JsonValueKind.Array)
		{
			foreach (var tv in traits.EnumerateArray())
			{
				var t = tv.GetString();
				switch (t)
				{
					case "demo":
						u.IsDemo = true;
						break;
				}
			}
		}
		u.ExtensionData = null;

		return ur;
	}
	public async Task<ListUploadsResult> ListUploads(long gameID, long downloadKeyId = 0, string password = null, string secret = null)
	{
		var ur = await new RequestBuilder(BaseAPIPath + "games/" + gameID + "/uploads")
			.AddLongIfNotZero("download_key_id", downloadKeyId)
			.AddStringIfNotEmpty("password", password)
			.AddStringIfNotEmpty("secret", secret)
			.Get<ListUploadsResult>(HttpClient);

		foreach (var u in ur.Uploads)
		{
			u.Platforms = PlatformHelper.FromTraits(u.ExtensionData);
			var traits = u.ExtensionData["traits"];
			if (traits.ValueKind == JsonValueKind.Array)
			{
				foreach (var tv in traits.EnumerateArray())
				{
					var t = tv.GetString();
					switch (t)
					{
						case "demo":
							u.IsDemo = true;
							break;
					}
				}
			}
			u.ExtensionData = null;

			if(u.Type == "html")
			{
				u.Platforms |= Platforms.Web;
			}
		}

		return ur;
	}
}

public class UploadResult : ItchResponse
{
	[JsonPropertyName("upload")]
	public Upload Upload { get; set; }
}

public class ListUploadsResult : ItchResponse
{
	[JsonPropertyName("uploads")]
	public List<Upload> Uploads { get; set; }
}

public class Upload
{
	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }
	[JsonPropertyName("filename")]
	public string Filename { get; set; }
	[JsonPropertyName("size")]
	public long Size { get; set; }
	[JsonPropertyName("id")]
	public long ID { get; set; }
	[JsonPropertyName("storage")]
	public string Storage { get; set; }
	[JsonPropertyName("game_id")]
	public long GameID { get; set; }
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAt { get; set; }
	[JsonPropertyName("created_at")]
	public DateTime CreatedAt { get; set; }
	[JsonPropertyName("md5_hash")]
	public string MD5Hash { get; set; }
	[JsonPropertyName("position")]
	public long Position { get; set; }
	[JsonPropertyName("type")]
	public string Type { get; set; }

	public bool IsDemo { get; set; }
	public Platforms Platforms { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement> ExtensionData { get; set; }
}