using System.Text.Json;

public static class Log
{
	public static event OnLogDelegate OnLog;
	public delegate void OnLogDelegate(string message);

	static Log()
	{
		OnLog += Console.WriteLine;
	}

	public static void Write(object o)
	{
		if(o.GetType() == typeof(string))
			OnLog?.Invoke((string)o);
		else
		{
			OnLog?.Invoke(JsonSerializer.Serialize(o));
		}
	}
}