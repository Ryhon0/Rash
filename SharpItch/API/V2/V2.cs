namespace SharpItch;
public partial class Itch
{
	public string BaseAPIURLV2 = "https://api.itch.io/";
	public string V2APIKey;
	public V2APITokenType V2APITokenType = V2APITokenType.Cookie;
}

public enum V2APITokenType
{
	Cookie,
	APIKey
}

public static class V2RequestBuilderExtensions
{
	public static RequestBuilder AddV2AcceptHeader(this RequestBuilder rb)
		=> rb.AddHeader("Accept", "application/vnd.itch.v2");
	
	public static RequestBuilder AddV2Token(this RequestBuilder rb, Itch itch)
	{
		if(itch.V2APIKey == null)
			throw new Exception("API Key not set");

		switch(itch.V2APITokenType)
		{
			default:
			case V2APITokenType.Cookie:
				return rb.AddCookie("itchio", itch.V2APIKey);
			case V2APITokenType.APIKey:
				return rb.AddHeader("Authorization", itch.V2APIKey);
		}
	}
}