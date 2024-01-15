using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SlideShow;

public static class Extensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        var span = CollectionsMarshal.AsSpan(list);
        RandomNumberGenerator.Shuffle(span);
    }
}
