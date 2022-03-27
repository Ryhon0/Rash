using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SharpItch;
public partial class Itch
{
	public Itch()
	{
		HttpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.itch.v2");
	}

	public string BaseAPIPath = "https://api.itch.io/";

	public HttpClient HttpClient = new();
}

public class ItchException : Exception
{
	public ItchException(string[] errors)
	{
		Errors = errors;
	}

	public string[] Errors;

	public override string ToString()
	{
		return string.Join("\n", Errors);
	}
}

public class ItchResponse
{
	[JsonPropertyName("success")]
	public bool Success { get; set; } = true;
	[JsonPropertyName("errors")]
	public string[] Errors { get; set; }
}