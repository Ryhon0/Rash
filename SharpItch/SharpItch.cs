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
		
	}
	
	public HttpClient HttpClient = new();
}

public class ItchResponse
{
	[JsonPropertyName("errors")]
	public string[] Errors { get; set; }

	[JsonPropertyName("details")]
	public string Details { get; set; }

	[JsonIgnore]
	public bool Successful => Errors == null || !Errors.Any();
}