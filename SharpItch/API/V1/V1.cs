namespace SharpItch;
public partial class Itch
{
	public string APIKey;
	public string BaseAPIURLV1 = "https://itch.io/api/1";
	public AuthenticationType V1APIAuthenticationType = AuthenticationType.KEY;

	string BuildServerURL(string endpoint)
	{
		string url = BaseAPIURLV1 + "/";
		switch(V1APIAuthenticationType)
		{
			case AuthenticationType.URL:
				url += $"{APIKey}/";
				break;
			case AuthenticationType.KEY:
				url += $"key/";
				break;
			case AuthenticationType.JWT:
				url += $"jwt/";
				break;
		}

		return $"{BaseAPIURLV1}/{endpoint}";
	}
}

public enum AuthenticationType
{
	URL,
	KEY,
	JWT
}

public static class V1RequestBuilderExtensions
{
	public static RequestBuilder AddToken(this RequestBuilder rb, Itch itch)
	{
		if(itch.APIKey == null)
			throw new Exception("API Key not set");

		if(itch.V1APIAuthenticationType != AuthenticationType.URL)
			return rb.AddHeader("Authorization", "Bearer " + itch.APIKey);
		else
			return rb;
	}
}