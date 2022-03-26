using SharpItch;
namespace Rash;
public static class RashClient
{
	public static Itch Itch = new(null);
	public static List<OwnedKey> OwnedKeys = new();
	public static bool OwnedKeysFinished = false;
}