using System.Text.Json.Serialization;
namespace SharpItch;

public class User
{
	[JsonPropertyNameAttribute("display_name")]
	public string DisplayName { get; set; }
	[JsonPropertyNameAttribute("id")]
	public long ID { get; set; }
	[JsonPropertyNameAttribute("url")]
	public string URL { get; set; }
	[JsonPropertyNameAttribute("cover_url")]
	public string CoverURL { get; set; }
	[JsonPropertyNameAttribute("username")]
	public string Username { get; set; }
}