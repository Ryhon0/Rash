namespace SharpItch;
using System.Text.Json.Serialization;

partial class Itch
{
	public async Task<GraphsResponse> GetGraphs()
	{
		return await new RequestBuilder(BuildV1ApiURL("my-games/graphs"))
			.AddV1Token(this)
			.Get<GraphsResponse>(HttpClient);
	}
}

public class GraphsResponse : ItchResponse
{
	[JsonPropertyName("views")]
	public List<GraphPoint> Views { get; set; }
	[JsonPropertyName("purchases")]
	public List<GraphPoint> Purchases { get; set; }
	[JsonPropertyName("downloads")]
	public List<GraphPoint> Downloads { get; set; }
}

public class GraphPoint
{
	[JsonPropertyName("count")]
	public long Count { get; set; }
	[JsonPropertyName("date")]
	public DateTime Date { get; set; }
}