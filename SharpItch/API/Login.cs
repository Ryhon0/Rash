namespace SharpItch;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public class ItchLoginResponse : ItchResponse
{
	[JsonPropertyName("recaptcha_needed")]
	public bool RecaptchaNeeded { get; set; }
	[JsonPropertyName("recaptcha_url")]
	public string RecaptchaURL { get; set; }
	[JsonPropertyName("totp_needed")]
	public bool TOTPNeeded { get; set; }
	[JsonPropertyName("token")]
	public string Token { get; set; }

	[JsonPropertyName("key")]
	public ItchAPIKey Key { get; set; }
	[JsonPropertyName("cookie")]
	public Dictionary<string, string> Cookies { get; set; }
}

public class ItchAPIKey
{
	[JsonPropertyName("id")]
	public long ID { get; set; }
	[JsonPropertyName("user_id")]
	public long UserID { get; set; }
	[JsonPropertyName("key")]
	public string Key { get; set; }
	[JsonPropertyName("created_at")]
	public DateTime CreatedAt { get; set; }
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAt { get; set; }
	[JsonPropertyName("source_version")]
	public string SourceVersion { get; set; }
}

public partial class Itch
{
	public async Task<ItchLoginResponse> Login(string username, string password,
		string recaptcha_response = null, bool force_recaptcha = false,
		string source = "desktop")
	{
		return await new RequestBuilder(BaseAPIPath + "login")
			.AddString("source", source)
			.AddString("username", username)
			.AddString("password", password)
			.AddStringIfNotEmpty("recaptcha_response", recaptcha_response)
			.AddBoolIfTrue("force_recaptcha", force_recaptcha)
			.Post<ItchLoginResponse>(HttpClient);
	}

	public void SetCookieToken(string token)
	{
		HttpClient.DefaultRequestHeaders.Add("Cookie", "itchio="+token);
	}
}