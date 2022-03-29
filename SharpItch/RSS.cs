namespace SharpItch;

using System.Xml;

partial class Itch
{
	public string BuildGameFeedURL(ItchGameFeedParameters p, int page = 1)
	{
		string url = "https://itch.io/games";

		// The search options need to be in a specific order
		// If they're not, the website will redirect to a url with proper order
		// The redirect does not copy the query parameters, like page

		if (p.Price != Price.Default)
		{
			url += "/" + 
			new Dictionary<Price, string>()
			{
				[Price.Free] = "free",
				[Price.OnSale] = "on-sale",
				[Price.Paid] = "store",
				[Price._5DollarsOrLess] = "5-dollars-or-less",
				[Price._15DollarsOrLess] = "15-dollars-or-less"
			}
			[p.Price];
		}

		if (!p.Platforms.HasFlag(Platforms.All))
		{
			if (p.Platforms.HasFlag(Platforms.Android)) url += "/platform-android";
			if (p.Platforms.HasFlag(Platforms.iOS)) url += "/platform-ios";
			if (p.Platforms.HasFlag(Platforms.Linux)) url += "/platform-linux";
			if (p.Platforms.HasFlag(Platforms.MacOS)) url += "/platform-osx";
			if (p.Platforms.HasFlag(Platforms.Web)) url += "/platform-web";
			if (p.Platforms.HasFlag(Platforms.Windows)) url += "/platform-windows";
		}

		if (p.Tags != null && p.Tags.Any())
			url += "/" + string.Join("/", p.Tags.Select(t => "tag-" + t));

		// TODO do everything else

		var rb = new RequestBuilder(url + ".xml");
		if(p.ExcludedTag != null)
			rb.AddString("exclude", "tg." + p.ExcludedTag);
		rb.AddLong("page", page);

		return rb.BuildURL() ;
	}
	public async Task<List<Game>> ReadFeed(string url)
	{
		List<Game> games = new List<Game>();
		string xmls;
		using(HttpClient http = new HttpClient())
		{
			xmls = await (await http.GetAsync(url)).Content.ReadAsStringAsync();
		}


		XmlDocument xml = new XmlDocument();
		xml.LoadXml(xmls);

		foreach(XmlNode e in xml.DocumentElement.FirstChild.ChildNodes)
		{
			if(e.Name != "item") continue;

			Game game = new();
			foreach(XmlNode c in e.ChildNodes)
			{
				switch(c.Name)
				{
					case "plainTitle":
						game.Title = c.InnerText;
						break;
					case "imageurl":
						game.CoverURL = c.InnerText;
						break;
					case "price":
						var price = c.InnerText[1..].Replace(".", "");
						game.PriceCents = long.Parse(price);
						break;
					case "discountpercent":
						game.Sale = new();
						game.Sale.Rate = int.Parse(c.InnerText);
						break;
					case "fullPrice":
						//var price = long.Parse(c.InnerText[1..].Replace(".", ""));
						break;
					case "saleends":
						game.Sale.EndDate = DateTime.Parse(c.InnerText);
						break;
					case "description":
						game.ShortText = c.InnerText;
						break;
					case "pubDate":
						game.PublishedAt = DateTime.Parse(c.InnerText);
						break;
					case "createDate":
						game.CreatedAt = DateTime.Parse(c.InnerText);
						break;
					case "link":
						game.URL = c.InnerText;
						break;
					/*
					case "updateDate":
						game.PublishedAt = DateTime.Parse(c.InnerText);
						break;
					*/
					case "platforms":
						foreach(XmlNode pl in c.ChildNodes)
						{
							switch(pl.Name)
							{
								case "windows":
									game.Platforms |= Platforms.Windows;
									break;
								case "linux":
									game.Platforms |= Platforms.Linux;
									break;
								case "osx":
									game.Platforms |= Platforms.MacOS;
									break;
								case "android":
									game.Platforms |= Platforms.Android;
									break;
								case "html":
								case "flash":
								case "unity":
									game.Type = pl.Name;
									game.Platforms |= Platforms.Web;
									break;
							}
						}
						break;
				}
			}
			games.Add(game);
		}

		return games;
	}
}

public class ItchGameFeedParameters
{
	public Platforms Platforms { get; set; }
	public Price Price;
	public List<string> Tags;
	public string ExcludedTag;
}

public enum Price
{
	Default,
	Free,
	OnSale,
	Paid,
	_5DollarsOrLess,
	_15DollarsOrLess
}

public enum When
{
	LastDay,
	Last7Days,
	Last30Days,
}


public enum Genre
{
	Action,
	Adventure,
	CardGame,
	Educational,
	Fighting,
	InteractiveFiction,
	Platformer,
	Puzzle,
	Racing,
	Rhythm,
	RolePlaying,
	Shooter,
	Simulation,
	Sports,
	Strategy,
	Survival,
	VisualNovel,
	Other,
}

[Flags]
public enum InputMethods
{
	Keyboard,
	Mouse,
	XboxController,
	Gamepad,
	Joystick,
	Touchscreen,
	VoiceControl,
	OculusRift,
	LeapMotion,
	Wiimote,
	Kinect,
	NeuroSkyMindwave,
	Accelerometer,
	OSVR,
	Smartphone,
	DancePad,
	HTCVive,
	GoogleDaydreamVR,
	GoogleCardboardVR,
	PlaystationController,
	MIDIController,
	JoyCon,
	MagicLeap,
	OculusQuest,
	OculusGo,
	WindowsMixedReality,
	ValveIndex
}

public enum SessionLength
{
	FewSeconds,
	FewMinutes,
	HalfHour,
	Hour,
	FewHours,
	Days
}

public enum MultiplayerFeatures
{
	LocalMultiplayer,
	ServerBased,
	AdHoc
}

public enum AccesebilityFeatures
{
	ColorBlindFriendly,
	Subtitles,
	ConfigurableControls,
	HighContrast,
	InteractiveTutorial,
	OneButton,
	BlindFriendly,
	Textless
}

public enum GameType
{
	HTML5,
	Downloadable,
	Flash
}

public enum Misc
{
	WithSteamKeys,
	InGameJams,
	NotInGameJams,
	WithDemos,
	Featured
}