namespace SharpItch;
using System.Text.Json.Serialization;

partial class Itch
{
	/// Returns the current profile.
	public async Task<GetProfileResponse> GetProfile()
	{
		return await new RequestBuilder(BaseAPIURLV2 + "profile")
			.AddV2AcceptHeader()
			.AddItchCookie(ItchCookie)
			.Get<GetProfileResponse>(HttpClient);
	}
}

public class GetProfileResponse : ItchResponse
{
	[JsonPropertyName("user")]
	public User User { get; set; }
}