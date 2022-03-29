namespace SharpItch;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

public class Game
{
	[JsonPropertyName("id")]
	public long ID { get; set; }
	[JsonPropertyName("url")]
	public string URL { get; set; }
	[JsonPropertyName("title")]
	public string Title { get; set; }
	[JsonPropertyName("short_text")]
	public string ShortText { get; set; }
	/// <summary>
	/// One of these: default, flash, unity, html
	/// flash and unity are legacy
	/// </summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = "default";
	/// <summary>
	/// One of these: game, tool, assets, game_mod, physical_game, soundtrack, other, book
	/// </summary>
	[JsonPropertyName("classification")]
	public string Classification { get; set; }
	[JsonPropertyName("embed")]
	public GameEmbedData Embed { get; set; }
	[JsonPropertyName("cover_url")]
	public string CoverURL { get; set; }
	[JsonPropertyName("created_at")]
	public DateTime CreatedAt { get; set; }
	[JsonPropertyName("published_at")]
	public DateTime PublishedAt { get; set; }
	[JsonPropertyName("min_price")]
	public long PriceCents { get; set; }
	[JsonPropertyName("can_be_bought")]
	public bool CanBeBought { get; set; }
	[JsonPropertyName("has_demo")]
	public bool HasDemo { get; set; }
	[JsonPropertyName("in_press_system")]
	public bool IsInPressSystem { get; set; }
	[JsonPropertyName("user")]
	public User User { get; set; }
	[JsonPropertyName("user_id")]
	public long UserID { get; set; }
	[JsonPropertyName("sale")]
	public Sale Sale { get; set; }

	[JsonPropertyName("views_count")]
	public long ViewsCount { get; set; }
	[JsonPropertyName("downloads_count")]
	public long DownloadsCount { get; set; }
	[JsonPropertyName("purchases_count")]
	public long PurchasesCount { get; set; }
	[JsonPropertyName("published")]
	public bool Published { get; set; } = true;

	public Platforms Platforms { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement> ExtensionData { get; set; }

	public void ProcessExtensionData()
	{
		Platforms = PlatformHelper.FromTraits(ExtensionData);

		if (new[] { "html", "unity", "flash" }.Contains(Type))
			Platforms |= Platforms.Web;

		if (ExtensionData.ContainsKey("traits"))
		{
			var traits = ExtensionData["traits"];
			if (traits.ValueKind != JsonValueKind.Array) return;

			foreach (var tv in traits.EnumerateArray())
			{
				var t = tv.GetString();
				switch (t)
				{
					case "can_be_bought":
						CanBeBought = true;
						break;
					case "has_demo":
						HasDemo = true;
						break;
					case "in_press_system":
						IsInPressSystem = true;
						break;
				}
			}
		}

		// ExtensionData = null;
	}

	public static async Task<long> GetIDFromURL(string url)
	{
		string html;
		using (var http = new HttpClient())
		{
			var res = await http.GetAsync(url);
			html = await res.Content.ReadAsStringAsync();
		}

		// Very lazy, better implementation would be nice
		var idrx = new Regex("<meta content=\\\"games/(?<id>\\d*)\\\" name=\\\"itch:path\\\"/>");
		var m = idrx.Match(html);
		long id = long.Parse(m.Groups[1].Value);
		return id;
	}
}

public class GameEmbedData
{
	[JsonPropertyName("game_id")]
	public long GameID { get; set; }
	[JsonPropertyName("width")]
	public long Width { get; set; }
	[JsonPropertyName("height")]
	public long Height { get; set; }
	[JsonPropertyName("fullscreen")]
	public bool Fullscreen { get; set; }
}

/*
public class Platforms
{
	/// <summary>
	/// all, 386, amd64
	/// </summary>
	/// 
	[JsonPropertyName("windows")]
	public string Windows { get; set; }
	[JsonPropertyName("linux")]
	public string Linux { get; set; }
	[JsonPropertyName("osx")]
	public string OSX { get; set; }
}
*/

public class Sale
{
	[JsonPropertyName("id")]
	public long ID { get; set; }
	[JsonPropertyName("game_id")]
	public long GameID { get; set; }
	[JsonPropertyName("rate")]
	public double Rate { get; set; }
	[JsonPropertyName("start_date")]
	public DateTime StartDate { get; set; }
	[JsonPropertyName("end_date")]
	public DateTime EndDate { get; set; }
}

[Flags]
public enum Platforms
{
	Windows = 1,
	MacOS = 1 << 1,
	Linux = 1 << 2,
	Android = 1 << 3,
	iOS = 1 << 4,
	Web = 1 << 5,
	All = Windows | MacOS | Linux | Android | iOS | Web,
	Desktop = Windows | MacOS | Linux,
	Mobile = Android | iOS
}

public static class PlatformHelper
{
	public static Platforms FromTraits(Dictionary<string, JsonElement> data)
	{
		Platforms p = 0;
		if (data.ContainsKey("traits"))
		{
			var traits = data["traits"];
			if (traits.ValueKind != JsonValueKind.Array) return p;

			foreach (var tv in traits.EnumerateArray())
			{
				var t = tv.GetString();
				switch (t)
				{
					case "p_osx":
						p |= Platforms.MacOS;
						break;
					case "p_windows":
						p |= Platforms.Windows;
						break;
					case "p_linux":
						p |= Platforms.Linux;
						break;
					case "p_android":
						p |= Platforms.Android;
						break;
				}
			}
		}

		return p;
	}
}