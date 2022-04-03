namespace Rash;
using SharpItch;
using System.Diagnostics;
using System.Text.Json;
using SharpCompress.Compressors;
using SharpCompress.Readers;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.Xz;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
public static class DownloadManager
{
	public static event EventHandler OnDownloadStarted;
	public static List<GameDownload> Downloads { get; set; } = new();

	public static async Task StartDownloadUpload(long gameID, long uploadID)
	{
		var game = await RashClient.GetGame(gameID);
		var key = RashClient.GetKey(uploadID);
		var upload = (await RashClient.Itch.GetUpload(uploadID, key)).Upload;

		var lib = Library.Libraries.First();
		LibraryGameInfo gi = lib.Games.FirstOrDefault(g => g.Game.ID == game.ID);
		if (gi == null)
		{
			gi = new LibraryGameInfo()
			{
				Game = game
			};
			lib.Games.Add(gi);
			var gamepath = lib.Path + "/" + game.ID;
			Directory.CreateDirectory(gamepath);
			await File.WriteAllTextAsync(gamepath + "/game.json", JsonSerializer.Serialize(game));
		}

		var path = lib.Path + "/" + game.ID + "/" + upload.ID;
		LibraryUploadInfo ui = new LibraryUploadInfo()
		{
			Upload = upload,
			DirectoryPath = path
		};
		Directory.CreateDirectory(path);
		ui.Save();
		gi.Uploads.Add(ui);

		await ContinueUploadDownload(gi.Game, ui);
	}

	public static async Task ContinueUploadDownload(Game g, LibraryUploadInfo ui)
	{
		var dl = new GameDownload(g, ui);
		dl.Downloader.OnFinish += (c, a) =>
		{
			ui.DownloadFinished = true;
			ui.Save();
			dl.Downloader.output.Close();
		};
		Downloads.Add(dl);
		OnDownloadStarted?.Invoke(dl, EventArgs.Empty);
		await dl.StartDownload();
	}

	public static async Task InstallUpload(Game g, LibraryUploadInfo ui)
	{
		var file = ui.DirectoryPath + "/" + ui.Upload.Filename;

		// https://itch.io/docs/itch/integrating/compatibility-policy.html
		// Gold tier
		// Should extract without any warnings
		if (file.EndsWith(".zip"))
		{

		}
		else if (file.EndsWith(".rar"))
		{

		}
		else if (file.EndsWith(".tar") ||
				file.EndsWith(".tar.gz") ||
				file.EndsWith(".tar.bz2") ||
				// Technically Silver tier
				file.EndsWith(".tar.xz"))
		{
			Stream input = File.Open(ui.DirectoryPath + "/" + ui.Upload.Filename, FileMode.Open);

			var compression = new FileInfo(file).Extension;
			switch (compression)
			{
				case "gz":
					input = new DeflateStream(input, CompressionMode.Decompress);
					break;
				case "bz2":
					input = new BZip2Stream(input, CompressionMode.Decompress, true);
					break;
				case "xz":
					input = new XZStream(input);
					break;
			}

			var tar = TarArchive.Open(input);
			tar.ExtractAllEntries();
			using (var reader = tar.ExtractAllEntries())
			{
				while(reader.MoveToNextEntry())
				{
					reader.WriteEntryToDirectory(ui.DirectoryPath, new SharpCompress.Common.ExtractionOptions()
					{
						ExtractFullPath = true,
						PreserveAttributes = true,
						PreserveFileTime = true,
						Overwrite = true
					});
				}
			}
		}
		else if (file.EndsWith(".dmg"))
		{

		}
		// Silver tier
		else if (file.EndsWith(".7z"))
		{

		}

		else if (file.EndsWith(".apk"))
		{

		}
		else if (file.EndsWith(".exe"))
		{
			// Check if is a InstallShield archive
		}
		else
		{
			// Unknown, bronze/oh no tier download
		}

		ui.ScannedArchive = (await RashClient.Itch.GetUploadScannedArchive(ui.Upload.ID,
			RashClient.GetKey(ui.Upload.ID))).ScannedArchive;
		ui.ExtractFinished = true;
	}
}

public class GameDownload
{
	public Game Game;
	public LibraryUploadInfo Upload;
	public Downloader Downloader { get; set; }

	public GameDownload(Game game, LibraryUploadInfo upload)
	{
		Game = game;
		Upload = upload;
	}

	public async Task StartDownload()
	{
		var key = RashClient.GetKey(Game.ID);
		var uuid = (await RashClient.Itch.NewDownloadSession(Game.ID, key)).UUID;

		var url = RashClient.Itch.CreateDownloadURL(Upload.Upload.ID, uuid, key);

		var dldir = Upload.DirectoryPath;
		Utils.CreateDirectoryWithParents(dldir);
		var filepath = dldir + "/" + Upload.Upload.Filename;

		var dl = new Downloader(url, filepath, RashClient.Itch.HttpClient);
		Downloader = dl;

		if (File.Exists(filepath))
			dl.progressBytes = new FileInfo(filepath).Length;

		dl.Download();
	}
}

public class Downloader
{
	public event EventHandler OnProgress;
	public event EventHandler OnPaused;
	public event EventHandler OnFinish;
	public event EventHandler OnError;

	HttpClient http;
	string url;
	public FileStream output;
	public long progressBytes = 0;
	public long totalBytes = 0;
	public float Progress => (float)progressBytes / totalBytes;
	public DownloaderState State = DownloaderState.Starting;
	public long DownloadSpeed;

	const long chunkSize = 512 * 1024;
	public Downloader(string url, string dest, HttpClient http = null)
	{
		this.url = url;

		if (http == null)
			this.http = new HttpClient();
		else
			this.http = http;

		output = File.Open(dest, FileMode.Append, FileAccess.Write);
		output.Seek(0, System.IO.SeekOrigin.End);
	}

	// Starts a new thread and downloads until it's stops
	public void Download()
	{
		State = DownloaderState.Starting;
		new Task(async () =>
		{
			while (await DownloadChunk())
			{ }
		}).Start();
	}

	public async Task<bool> DownloadChunk()
	{
		if (State == DownloaderState.Paused)
		{
			OnPaused?.Invoke(this, null);
			return false;
		}

		var to = Math.Min(totalBytes, progressBytes + chunkSize - 1);
		if (to == 0) to = progressBytes + chunkSize - 1;

		var hr = new RequestBuilder(url)
			.AddHeader("Range", $"bytes={progressBytes}-{to}")
			.AddV2Token(RashClient.Itch)
			.Build();
		hr.Method = HttpMethod.Get;

		Stopwatch sw = new Stopwatch();
		sw.Start();

		var res = await http.SendAsync(hr);

		if (!res.IsSuccessStatusCode)
		{
			State = DownloaderState.Error;
			OnError?.Invoke(this, null);
			return false;
		}

		progressBytes += (long)res.Content.Headers.ContentLength;
		totalBytes = (long)res.Content.Headers.ContentRange.Length;

		sw.Stop();
		var time = sw.Elapsed.TotalSeconds;
		DownloadSpeed = (long)(res.Content.Headers.ContentLength / time);

		await res.Content.CopyToAsync(output);

		OnProgress?.Invoke(this, null);

		if (State == DownloaderState.Paused)
		{
			OnPaused?.Invoke(this, null);
			return false;
		}
		else
			State = DownloaderState.Downloading;

		var next = progressBytes != totalBytes;
		if (!next)
		{
			State = DownloaderState.Finished;
			OnFinish?.Invoke(this, null);
		}
		return next;
	}
}

public enum DownloaderState
{
	Starting,
	Downloading,
	Finished,
	Paused,
	Error,
}