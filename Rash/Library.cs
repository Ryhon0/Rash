namespace Rash;

using System.Text.Json;
using SharpItch;

public static class Library
{
	public static List<LibraryInfo> Libraries = new List<LibraryInfo>()
	{
		new LibraryInfo()
		{
			Path = Config.DefaultLibraryPath
		}
	};
}

public class LibraryInfo
{
	public string Path { get; set; }

	public List<LibraryGameInfo> Games = new();

	public async Task Discover()
	{
		if (!Directory.Exists(Path))
		{
			Log.Write($"Library at {Path} does not exist, creating directory");
			Utils.CreateDirectoryWithParents(Path);
			return;
		}

		Log.Write($"Discovering games in {Path}");
		foreach (var gf in new DirectoryInfo(Path).EnumerateDirectories())
		{
			if (!long.TryParse(gf.Name, out _))
			{
				Log.Write($"Game folder '{gf.Name}' is not a number, skipping");
				continue;
			}

			var gamefile = gf.FullName + "/game.json";
			if (!File.Exists(gamefile))
			{
				Log.Write($"Game folder found but no game file found, skipping ({gf.Name})");
				continue;
			}

			var game = JsonSerializer.Deserialize<Game>(File.ReadAllText(gamefile));
			var gi = new LibraryGameInfo()
			{
				Game = game
			};
			Log.Write($"Discovered game {game.Title}({game.ID})");

			foreach (var uf in gf.EnumerateDirectories())
			{
				if (!long.TryParse(uf.Name, out _))
				{
					Log.Write($"Upload folder '{uf.Name}' is not a number, skipping");
					continue;
				}

				var uploadfile = uf.FullName + "/upload.json";
				if (!File.Exists(uploadfile))
				{
					Log.Write($"Upload folder found but no upload file found, skipping ({uf.Name})");
					continue;
				}

				var upload = JsonSerializer.Deserialize<LibraryUploadInfo>(File.ReadAllText(uploadfile));
				gi.Uploads.Add(upload);

				Log.Write($"Discovered upload {upload.Upload.DisplayName??upload.Upload.Filename}({upload.Upload.ID}) for game {game.Title}({game.ID})");

				if(!upload.DownloadFinished)
				{
					Log.Write($"Download for {upload.Upload.DisplayName??upload.Upload.Filename}({upload.Upload.ID}) was not finished, continuing");
					await DownloadManager.StartDownloadUpload(game.ID, upload.Upload.ID);
				}
			}
		
			if(gi.Uploads.Any())
				Games.Add(gi);
			else
				Log.Write($"Game folder found but no uploads found, skipping ({gf.Name})");
		}
	}
}

public class LibraryGameInfo
{
	public Game Game;

	public List<LibraryUploadInfo> Uploads = new();
}

public class LibraryUploadInfo
{
	public Upload Upload { get; set; }

	public ScannedArchive ScannedArchive { get; set; }
	public bool DownloadFinished { get; set; }
	public bool ExtractFinished { get; set; }
}