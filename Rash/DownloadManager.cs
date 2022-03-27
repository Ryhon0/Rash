namespace Rash;
using SharpItch;
using System.Diagnostics;
using System.Text.Json;

public static class DownloadManager
{
	public static event EventHandler OnDownloadStarted;
	public static List<GameDownload> Downloads { get; set; } = new();

	public static async Task StartDownloadUpload(long gameID, long uploadID)
	{
		var game = (await RashClient.Itch.GetGame(gameID)).Game;
		var key = RashClient.OwnedKeys.FirstOrDefault(k => k.GameID == gameID)?.ID ?? 0;
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
			Directory.CreateDirectory(lib.Path + "/" + game.ID);
			await File.WriteAllTextAsync(lib.Path + "/" + game.ID + "/game.json", JsonSerializer.Serialize(game));
		}

		LibraryUploadInfo ui = new LibraryUploadInfo()
		{
			Upload = upload
		};
		Directory.CreateDirectory(lib.Path + "/" + game.ID + "/" + upload.ID);
		await File.WriteAllTextAsync(lib.Path + "/" + game.ID + "/" + upload.ID + "/upload.json", JsonSerializer.Serialize(ui));
		gi.Uploads.Add(ui);

		var dl = new GameDownload(game, upload);
		Downloads.Add(dl);
		await dl.StartDownload();
		dl.Downloader.OnFinish += (c, a) =>
		{
			ui.DownloadFinished = true;
			File.WriteAllText(lib.Path + "/" + game.ID + "/" + upload.ID + "/upload.json", JsonSerializer.Serialize(ui));
		};
		OnDownloadStarted?.Invoke(dl, EventArgs.Empty);
	}
}

public class GameDownload
{
	public Game Game;
	public Upload Upload;
	public Downloader Downloader { get; set; }

	public GameDownload(Game game, Upload upload)
	{
		Game = game;
		Upload = upload;
	}

	public async Task StartDownload()
	{
		var key = RashClient.OwnedKeys.FirstOrDefault(k => k.GameID == Game.ID)?.ID ?? 0;
		var uuid = (await RashClient.Itch.NewDownloadSession(Game.ID, key)).UUID;

		var url = RashClient.Itch.CreateDownloadURL(Upload.ID, uuid, key);

		var gamedir = System.Environment.GetEnvironmentVariable("HOME") + "/Games/Rash/" + Game.ID;
		var dldir = gamedir + "/" + Upload.ID;
		Utils.CreateDirectoryWithParents(dldir);
		var filepath = dldir + "/" + Upload.Filename;

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
	FileStream output;
	public string OutputPath => output.Name;
	public long progressBytes = 0;
	public long totalBytes = 0;
	public float Progress => (float)progressBytes / totalBytes;
	public DownloaderState State = DownloaderState.Starting;
	public long DownloadSpeed => (long)ProgressHistory.Average();

	const int ProgressHistoryLength = 10;
	public long[] ProgressHistory = new long[ProgressHistoryLength];
	int currentHistoryId = 0;

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

		var hr = new HttpRequestMessage(HttpMethod.Get, url);

		var to = Math.Min(totalBytes, progressBytes + chunkSize - 1);
		if (to == 0) to = progressBytes + chunkSize - 1;
		hr.RequestUri = new Uri(url);
		hr.Headers.Add("Range", $"bytes={progressBytes}-{to}");

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
		await res.Content.CopyToAsync(output);

		sw.Stop();
		var time = sw.Elapsed.TotalSeconds;
		ProgressHistory[currentHistoryId] = (long)((long)res.Content.Headers.ContentLength / time);
		currentHistoryId++;
		currentHistoryId %= ProgressHistoryLength;

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