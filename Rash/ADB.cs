using System.Diagnostics;

public class ADB
{
	public string adbPath = "adb";

	public async Task StartServer()
	{
		Process p = new();
		ProcessStartInfo si = new ProcessStartInfo(adbPath, "start-server");
		//si.RedirectStandardOutput = true;
		p.StartInfo = si;
		p.Start();

		await p.WaitForExitAsync();
	}

	public async Task KillServer()
	{
		Process p = new();
		ProcessStartInfo si = new ProcessStartInfo(adbPath, "kill-server");
		//si.RedirectStandardOutput = true;
		p.StartInfo = si;
		p.Start();

		await p.WaitForExitAsync();
	}

	public async IAsyncEnumerable<ADBDevice> GetDevices()
	{
		Process p = new();
		ProcessStartInfo si = new ProcessStartInfo(adbPath, "devices");
		si.RedirectStandardOutput = true;
		p.StartInfo = si;
		p.Start();

		bool devsstart = false;
		while (!p.StandardOutput.EndOfStream)
		{
			var line = await p.StandardOutput.ReadLineAsync();
			if (!devsstart)
			{
				if (line == "List of devices attached")
					devsstart = true;
			}
			else
			{
				if(!string.IsNullOrEmpty(line))
				{
					var cols = line.Split('\t');
					var serial = cols[0];
					var type = cols[1];

					yield return new ADBDevice(this, serial, type);
				}
			}
		}
	}
}

public class ADBDevice
{
	ADB adb;
	public string Serial;
	public string Type;
	public ADBDevice(ADB adb, string serial, string type)
	{
		this.adb = adb;
		this.Serial = serial;
		this.Type = type;
	}

	public async Task PushFile(string from, string to)
	{
		from = from.Replace("\"", "\\\"");
		to = to.Replace("\"", "\\\"");

		Process p = new();
		ProcessStartInfo si = new ProcessStartInfo(adb.adbPath, $"push \"{from}\" \"{to}\"");
		//si.RedirectStandardOutput = true;
		si.EnvironmentVariables.Add("ANDROID_SERIAL", Serial);
		p.StartInfo = si;
		
		p.Start();
		await p.WaitForExitAsync();
	}

	public Process Shell(string cmd)
	{
		Process p = new();
		ProcessStartInfo si = new ProcessStartInfo(adb.adbPath, $"shell {cmd}");
		si.RedirectStandardOutput = true;
		si.EnvironmentVariables.Add("ANDROID_SERIAL", Serial);
		p.StartInfo = si;

		return p;
	}

	public override string ToString()
	{
		return $"{Serial}\t{Type}";
	}
}