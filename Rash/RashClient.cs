namespace Rash;
using SharpItch;
using System.Text.Json;
public static class RashClient
{
	public static Itch Itch = new(null);
	public static List<OwnedKey> OwnedKeys = new();
	public static bool OwnedKeysFinished = false;

	public static Config Config = new();
}

public class Config 
{
	public List<string> LibraryPaths { get; set; } = new(){ DefaultLibraryPath };
	public string ItchCookie { get; set; }
	public string ItchAPIKey { get; set; }

	public void Save()
	{
		File.WriteAllText(DefaultConfigPath, JsonSerializer.Serialize(this));
	}

	public void Setup()
	{
		RashClient.Itch.SetCookieToken(ItchCookie);
		RashClient.Itch.APIKey = ItchAPIKey;
	}

	public static Config Load()
	{
		if(!File.Exists(DefaultConfigPath))
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