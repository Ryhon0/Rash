using System.Text.Json.Serialization;
namespace SharpItch;
public partial class Itch
{
	public async Task<TagsResponse> GetTags(string classification = "all")
	{
		return await new RequestBuilder("https://itch.io/tags.json")
			.AddStringIfNotEmpty("classification", classification)
			.Get<TagsResponse>();
	}

	public async Task<TagsResponse> GetBrowseTags(string classification = "all")
	{
		return await new RequestBuilder("https://itch.io/tags.json")
			.AddStringIfNotEmpty("classification", classification)
			.AddString("format", "browse")
			.Get<TagsResponse>();
	}
}

public class TagsResponse : ItchResponse
{
	[JsonPropertyName("tags")]
	public List<Tag> Tags { get; set; }

	[JsonPropertyName("slugs")]
	public List<string> Slugs { get; set; }
}

public class Tag
{
	[JsonPropertyName("url")]
	public string URL { get; set; }
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("facets")]
	public TagFacets Facets { get; set; }
}

public class TagFacets
{
	[JsonPropertyName("tag")]
	public string Tag { get; set; }
}