namespace Rash;
using SharpItch;
using System.Diagnostics;

public static class DownloadManager
{
	public static event EventHandler OnDownloadStarted;
	public static List<GameDownload> Downloads { get; set; } = new();

	public static async Task StartDownloadUpload(long gameID, long uploadID)
	{
		var game = (await RashClient.Itch.GetGame(gameID)).Game;
		var key = RashClient.OwnedKeys.FirstOrDefault(k => k.GameID == gameID)?.ID ?? 0;
		var upload = (await RashClient.Itch.GetUpload(uploadID, key)).Upload;

		var dl = new GameDownload(game, upload);
		Downloads.Add(dl);
		await dl.StartDownload();
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

		var gamedir = System.Environment.GetEnvironmentVariable("HOME") + "/Games/" + Game.ID;
		if(!Directory.Exists(gamedir))
			Directory.CreateDirectory(gamedir);
		var dldir = gamedir + "/" + Upload.ID;
		if(!Directory.Exists(dldir))
			Directory.CreateDirectory(dldir);

		var dl = new Downloader(url, dldir + "/" + Upload.Filename, RashClient.Itch.HttpClient);
		Downloader = dl;

		new Task(async () =>
		{
			while (await dl.DownloadChunk())
			{ }
		}).Start();
	}
}

public class Downloader
{
	public event EventHandler OnProgress;
	public event EventHandler OnFinish;
	public event EventHandler OnError;

	HttpClient http;
	string url;
	FileStream output;
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

		File.Delete(dest);
		output = File.Open(dest, FileMode.CreateNew, FileAccess.Write);
		output.Seek(0, System.IO.SeekOrigin.End);
	}

	public async Task<bool> DownloadChunk()
	{
		var hr = new HttpRequestMessage(HttpMethod.Get, url);

		var to = Math.Min(totalBytes, progressBytes + chunkSize - 1);
		if (to == 0) to = progressBytes + chunkSize - 1;
		hr.RequestUri = new Uri(url);
		hr.Headers.Add("Range", $"bytes={progressBytes}-{to}");

		Stopwatch sw = new Stopwatch();
		sw.Start();

		var res = await http.SendAsync(hr);

		if(!res.IsSuccessStatusCode)
		{
			State = DownloaderState.Error;
			return false;
		}

		State = DownloaderState.Downloading;
		progressBytes += (long)res.Content.Headers.ContentLength;
		totalBytes = (long)res.Content.Headers.ContentRange.Length;
		await res.Content.CopyToAsync(output);

		sw.Stop();
		var time = sw.Elapsed.TotalSeconds;
		ProgressHistory[currentHistoryId] = (long)((long)res.Content.Headers.ContentLength / time);
		currentHistoryId++;
		currentHistoryId %= ProgressHistoryLength;
		OnProgress?.Invoke(this, null);
		

		var next = progressBytes != totalBytes;
		if(!next)
		{
			State = DownloaderState.Finished;
			OnFinish?.Invoke(this, null);
		}
		return next;
	}

	public static string BytesToString(long byteCount)
	{
		string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		if (byteCount == 0)
			return "0" + suf[0];
		long bytes = Math.Abs(byteCount);
		int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
		double num = Math.Round(bytes / Math.Pow(1024, place), 1);
		return (Math.Sign(byteCount) * num).ToString() + suf[place];
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