namespace SharpItch;

partial class Itch
{
	/// <summary>
	/// Same as V2, but returns 500 keys per page instead of 50.
	/// </summary>
	/// <param name="page"></param>
	/// <returns></returns>
	public async Task<OwnedKeysResult> GetOwnedKeysV1(long page = 1)
	{
		return await new RequestBuilder(BuildV1ApiURL("my-owned-keys"))
			.AddV1Token(this)
			.AddLong("page", page)
			.AddConverter(new V1DateConverter())
			.Get<OwnedKeysResult>(HttpClient);
	}
}