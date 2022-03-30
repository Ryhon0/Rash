namespace Rash;
using SharpItch;
using System.Text.Json;
public static class RashClient
{
	public static bool LoggedIn = false;
	public static Itch Itch = new();

	public static void StartKeyDownload()
	{
		new Task(async () =>
		{
			Log.Write("Starting download of owned keys");
			KeyDownloadFinished = false;
			var usev1 = false;
			if(Config.PreferV1GetGames)
				usev1 = Itch.APIKey != null;
			
			if(Itch.ItchCookie == null)
			{
				if(Itch.APIKey != null)
					usev1 = true;
				else
				{
					Log.Write("No API credentials set, downloading owned keys finished");
					KeyDownloadFinished = true;
					return;
				}
			}	

			int page = 1;
			while (true)
			{
				OwnedKeysResult okr = await (usev1 ? Itch.GetOwnedKeysV1(page) : Itch.GetOwnedKeys(page));
				CachedKeys.AddRange(okr.OwnedKeys);
				foreach(var k in okr.OwnedKeys)
					CachedGames[k.GameID] = k.Game;

				if (okr.PerPage != okr.OwnedKeys.Count)
				{
					Log.Write("Downloading owned keys finished");
					KeyDownloadFinished = true;
					return;
				}
				page++;
			}
		}).Start();
	}

	public static async Task<Game> GetGame(long id)
	{
		if (CachedGames.ContainsKey(id))
			return CachedGames[id];
		else
		{
			var g = await Itch.GetGame(id);
			CachedGames.Add(id, g.Game);
			return g.Game;
		}
	}

	public static long GetKey(long gameID)
		=> CachedKeys.FirstOrDefault(k => k.GameID == gameID)?.ID ?? 0;

	public static bool KeyDownloadFinished = false;
	public static List<OwnedKey> CachedKeys = new();
	public static Dictionary<long, Game> CachedGames = new();

	public static Config Config = new();
}

public class Config
{
	public List<string> LibraryPaths { get; set; } = new() { DefaultLibraryPath };
	public string ItchCookie { get; set; }
	public string ItchAPIKey { get; set; }
	public bool PreferV1GetGames { get; set; } = false;

	public void Save()
	{
		File.WriteAllText(DefaultConfigPath, JsonSerializer.Serialize(this));
	}

	public async Task Setup()
	{
		if (ItchCookie != null) RashClient.LoggedIn = true;
		if (ItchAPIKey == null)
		{
			// TODO: generate subkey
		}

		RashClient.Itch.ItchCookie = ItchCookie;
		RashClient.Itch.APIKey = ItchAPIKey;

		foreach (var lp in LibraryPaths)
		{
			var li = new LibraryInfo()
			{
				Path = lp
			};

			await li.Discover();
		}
	}

	public static Config Load()
	{
		if (!File.Exists(DefaultConfigPath))
		{
			var c = new Config();
			Utils.CreateDirectoryWithParents(DefaultConfigPath.Substring(0, DefaultConfigPath.LastIndexOf('/')));
			c.Save();
			return c;
		}

		var json = File.ReadAllText(DefaultConfigPath);
		return JsonSerializer.Deserialize<Config>(json);
	}

	public static string DefaultConfigPath => System.Environment.GetEnvironmentVariable("HOME") + "/.config/rash/config.json";
	public static string DefaultLibraryPath => System.Environment.GetEnvironmentVariable("HOME") + "/Games/Rash";
}