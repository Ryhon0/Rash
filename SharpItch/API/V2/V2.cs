namespace SharpItch;
public partial class Itch
{
	public string BaseAPIURLV2 = "https://api.itch.io/";
	public string ItchCookie;
}

public static class V2RequestBuilderExtensions
{
	public static RequestBuilder AddV2AcceptHeader(this RequestBuilder rb)
		=> rb.AddHeader("Accept", "application/vnd.itch.v2");
	
	public static RequestBuilder AddItchCookie(this RequestBuilder rb, string cookie)
	{
		if(cookie == null)
			throw new Exception("Itch cookie not set");

		return rb.AddCookie("itchio", cookie);
	}
}