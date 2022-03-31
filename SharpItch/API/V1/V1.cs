namespace SharpItch;
public partial class Itch
{
	public string V1APIKey;
	public string BaseAPIURLV1 = "https://itch.io/api/1";
	public V1APITokenType V1APITokenType = V1APITokenType.URL;

	string BuildV1ApiURL(string endpoint)
	{
		string url = BaseAPIURLV1 + "/";
		switch(V1APITokenType)
		{
			case V1APITokenType.URL:
				url += $"{V1APIKey}";
				break;
			case V1APITokenType.KEY:
				url += $"key";
				break;
			case V1APITokenType.JWT:
				url += $"jwt";
				break;
		}

		return $"{url}/{endpoint}";
	}
}

public enum V1APITokenType
{
	URL,
	KEY,
	JWT
}

public static class V1RequestBuilderExtensions
{
	public static RequestBuilder AddV1Token(this RequestBuilder rb, Itch itch)
	{
		if(itch.V1APIKey == null)
			throw new Exception("API Key not set");

		if(itch.V1APITokenType != V1APITokenType.URL)
			return rb.AddHeader("Authorization", "Bearer " + itch.V1APIKey);
		else
			return rb;
	}
}