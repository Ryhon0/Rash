public static class Utils
{
	public static string BytesToString(this long byteCount)
	{
		string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		if (byteCount == 0)
			return "0" + suf[0];
		long bytes = Math.Abs(byteCount);
		int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
		double num = Math.Round(bytes / Math.Pow(1024, place), 1);
		return (Math.Sign(byteCount) * num).ToString() + suf[place];
	}

	public static void CreateDirectoryWithParents(string dir)
	{
		if(Directory.Exists(dir)) return;

		var parent = Directory.GetParent(dir);
		CreateDirectoryWithParents(parent.FullName);
		Directory.CreateDirectory(dir);
	}
}