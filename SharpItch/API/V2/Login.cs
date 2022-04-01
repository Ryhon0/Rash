using System.Text.Json.Serialization;

namespace SharpItch;

public partial class Itch
{
	public async Task<ItchLoginResponse> Login(string username, string password,
		string recaptcha_response = null, bool force_recaptcha = false,
		string source = "desktop")
	{
		return await new RequestBuilder(BaseAPIURLV2 + "login")
			.AddString("source", source)
			.AddString("username", username)
			.AddString("password", password)
			.AddStringIfNotEmpty("recaptcha_response", recaptcha_response)
			.AddBoolIfTrue("force_recaptcha", force_recaptcha)
			.Post<ItchLoginResponse>(HttpClient);
	}

	public async Task<TOTPVerifyResponse> TOTPVerify(string token, string code)
	{
		return await new RequestBuilder(BaseAPIURLV2 + "totp/verify")
			.AddString("token", token)
			.AddString("code", code)
			.Post<TOTPVerifyResponse>(HttpClient);
	}
}

public class TOTPVerifyResponse : ItchResponse
{
	[JsonPropertyName("key")]
	public ItchAPIKey Key { get; set; }
	[JsonPropertyName("cookie")]
	public Dictionary<string, string> Cookies { get; set; }
}

public class ItchLoginResponse : TOTPVerifyResponse
{
	[JsonPropertyName("recaptcha_needed")]
	public bool RecaptchaNeeded { get; set; }
	[JsonPropertyName("recaptcha_url")]
	public string RecaptchaURL { get; set; }
	[JsonPropertyName("totp_needed")]
	public bool TOTPNeeded { get; set; }
	[JsonPropertyName("token")]
	public string Token { get; set; }
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