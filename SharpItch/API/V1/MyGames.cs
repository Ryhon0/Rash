using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SharpItch;

partial class Itch
{
	
}
/*

	async Task<T> Get<T>(string uri, string jsonkey = null)
	{
		using(HttpClient http = new HttpClient())
		{
			var res = await http.GetAsync(uri);
			JsonNode json = JsonNode.Parse(await res.Content.ReadAsStringAsync());
			JsonNode? errs = json["errors"];
			
			if(errs != null)
				throw new ItchException(errs.AsArray().Select(x => x.AsValue().ToString()).ToArray());

			if(jsonkey != null)
				return JsonSerializer.Deserialize<T>(json[jsonkey]);
			else 
				return JsonSerializer.Deserialize<T>(json);
		}
	}
	
	public async Task<CredentialsInfo> GetCredentialsInfo()
	{
		string url = BuildServerURL("credentials/info");
		return await Get<CredentialsInfo>(url);
	}

	[Scope("profile:me")]
	public async Task<ItchUser> GetMe()
	{
		string url = BuildServerURL("me");
		return await Get<ItchUser>(url, "user");
	}

	[Scope("profile:games")]
	public async Task<List<ItchGame>> GetMyGames()
	{
		string url = BuildServerURL("my-games");
		return await Get<List<ItchGame>>(url, "games");
	}

	[Scope("game:view:purchases")]
	public async Task<ItchDownloadKey> GetDownloadKeys(ulong gameID, ulong userID)
	{
		string url = BuildServerURL($"game/{gameID}/{userID}");
		return await Get<ItchDownloadKey>(url, "download_key");
	}

	// [Scope("game:view:purchases")]
}

public class CredentialsInfo
{
	public List<string> Scopes;
	public DateTime? ExpiresAt;
}

public class ItchUser
{
	public string Username;
	public string DisplayName;
	public Uri CoverURL;
	public Uri URL;
	public bool Gamer;
	public bool Developer;
	public bool PressUser;
	public ulong ID;
}

public class ItchGame
{
	[JsonPropertyName("id")]
	public string CoverURL;
	public DateTime CreatedAt;
	public ulong DownloadsCount;
	public ulong ID;
	public ulong MinPrice;
	[JsonPropertyName("p_android")]
	public bool PlatformAndroid;
	[JsonPropertyName("p_linux")]
	public bool PlatformLinux;
	[JsonPropertyName("p_windows")]
	public bool PlatformWindows;
	[JsonPropertyName("p_osx")]
	public bool PlatformOSX;
	public bool Published;
	public DateTime? PublishedAt;
	public ulong PurchasesCount;
	[JsonPropertyName("short_text")]
	public string Description;
	public string Title;
	public string Type;
	public Uri URL;
	public ulong ViewsCount;
	public List<ItchGameEarning> Earnings;
}

public class ItchGameEarning
{
	public string Currency;
	public string AmountFormatted;
	public ulong Amount;
}

public class ItchDownloadKey
{
	public ulong ID;
	public DateTime CreatedAt;
	public ulong Downloads;
	public string Key;
	public ulong GameID;
	public ItchUser Owner;
}
*/