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
	List<(string Key, string Value)> Headers = new();
	List<(string Key, string Value)> Cookies = new();
	byte[] Body;
	JsonSerializerOptions JsonOptions = new();

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

	public RequestBuilder AddConverter(JsonConverter conv)
	{
		JsonOptions.Converters.Add(conv);
		return this;
	}

	public RequestBuilder AddCookie(string key, string value)
	{
		Cookies.Add((key, value));
		return this;
	}

	public RequestBuilder AddHeader(string key, string value)
	{
		Headers.Add((key, value));
		return this;
	}

	public RequestBuilder WithBody(byte[] body)
	{
		Body = body;
		return this;
	}

	public RequestBuilder WithBody(string body)
	{
		Body = System.Text.Encoding.UTF8.GetBytes(body);
		return this;
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
		JsonOptions.Converters.Add(new LuaArrayConverter());
		var str = res.Content.ReadAsStringAsync().Result;
		return JsonSerializer.Deserialize<T>(str, JsonOptions);
	}

	public HttpRequestMessage Build()
	{
		HttpRequestMessage req = new();
		req.RequestUri = new Uri(BuildURL());
		
		foreach (var h in Headers)
			req.Headers.Add(h.Key, h.Value);

		if (Cookies.Any())
			req.Headers.Add("Cookie", string.Join("; ", Cookies.Select(c => $"{c.Key}={c.Value}")));

		if (Body != null && Body.Length != 0)
			req.Content = new ByteArrayContent(Body);

		return req;
	}

	public async Task<T> Get<T>(HttpClient http = null)
	{
		var req = Build();
		req.Method = HttpMethod.Get;

		if (http == null) http = new();

		var res = await http.SendAsync(req);
		return HandleResponse<T>(res);
	}

	public async Task<T> Post<T>(HttpClient http = null)
	{
		var req = Build();
		req.Method = HttpMethod.Post;

		if (http == null) http = new();

		var res = await http.SendAsync(req);
		return HandleResponse<T>(res);
	}
}

/// <summary>
/// itch.io API returns empty JSON objects instead of empty arrays, this is a workaround.
/// </summary>
public class LuaArrayConverter : JsonConverter<object>
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
			while (reader.CurrentDepth != d)
			{
				reader.Read();
			}
			return Activator.CreateInstance(typeToConvert);
		}
		// Oh my fucking god this is ugly
		else if(reader.TokenType == JsonTokenType.StartArray)
		{
			reader.Read();
			var t = typeToConvert.GetGenericArguments()[0];
			var lt = typeToConvert.GetGenericTypeDefinition().MakeGenericType(t);
			var constr = lt.GetConstructors()[0];
			var add = lt.GetMethod("Add");
			var list = constr.Invoke(new object[]{});
			
			while (reader.TokenType != JsonTokenType.EndArray)
			{
				add.Invoke(list, new object[]{ JsonSerializer.Deserialize(ref reader, t, options) });
				if(reader.TokenType == JsonTokenType.EndObject) reader.Read();
			}
			return list;
		}
		return JsonSerializer.Deserialize(ref reader, typeToConvert);
	}

	public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, options);
	}
}

public class V1DateConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString();
		// Bruh, this doesn't need to be so accurate
		var idx = str.IndexOf('.');
		if(idx != -1)
			str = str.Substring(0, idx);

		return DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", null);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.ToString("yyyy-MM-dd HH:MM:ss"), options);
	}
}