using System.Text.Json;
using System.Text.Json.Nodes;
using System.Reflection;
using System.Net;
using System.Text.Json.Serialization;

namespace SharpItch;

public class RequestBuilder
{
	string Url;
	List<(string Key, string Value)> Parameters = new();

	public string BuildURL()
	{
		if (Parameters.Any())
			return Url + "?" + string.Join('&', Parameters.Select(p => p.Key + "=" + FormatValue(p.Value)));
		else return Url;
	}

	static string FormatValue(object o)
	{
		return o.ToString();
	}

	public RequestBuilder(string url, params string[] url_params)
	{
		Url = string.Format(url, url_params);
	}

	public RequestBuilder AddLong(string key, long value)
	{
		Parameters.Add((key, value.ToString()));
		return this;
	}

	public RequestBuilder AddLongIfNotZero(string key, long value)
	{
		if (value != 0)
			Parameters.Add((key, value.ToString()));
		return this;
	}

	public RequestBuilder AddString(string key, string value)
	{
		Parameters.Add((key, value));
		return this;
	}

	public RequestBuilder AddStringIfNotEmpty(string key, string value)
	{
		if (!String.IsNullOrEmpty(value))
			Parameters.Add((key, value));
		return this;
	}

	public RequestBuilder AddBool(string key, bool value)
	{
		Parameters.Add((key, value.ToString()));
		return this;
	}

	public RequestBuilder AddBoolIfTrue(string key, bool value)
	{
		if (value)
			Parameters.Add((key, value.ToString()));
		return this;
	}

	T HandleResponse<T>(HttpResponseMessage res)
	{
			JsonSerializerOptions opts = new();
			opts.Converters.Add(new MyArrayConverter());

			return JsonSerializer.Deserialize<T>(res.Content.ReadAsStringAsync().Result, opts);
	}
	public async Task<T> Get<T>(HttpClient http = null)
	{
		if (http == null) http = new();

		var res = await http.GetAsync(BuildURL());
		return HandleResponse<T>(res);
	}

	public async Task<T> Post<T>(HttpClient http = null)
	{
		if (http == null) http = new();

		var res = await http.PostAsync(BuildURL(), null);
		return HandleResponse<T>(res);
	}
}

public class MyArrayConverter : JsonConverter<object>
{
	public override bool CanConvert(Type t)
	{
		if (!t.IsGenericType) return false;

		var can = t.GetGenericTypeDefinition() == typeof(List<>);
		return can;
	}

	public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.StartObject)
		{
			var d = reader.CurrentDepth;
			reader.Read();
			while(reader.CurrentDepth != d)
			{
				reader.Read();
			}
			return Activator.CreateInstance(typeToConvert);
		}

		return JsonSerializer.Deserialize(ref reader, typeToConvert);
	}

	public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, options);
	}
}