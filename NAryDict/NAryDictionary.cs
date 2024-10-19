using NAryDict;

namespace NAryDict
{
    public class NAryDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull { }
    public class NAryDictionary<TKey1, TKey2, TValue> : Dictionary<TKey1, NAryDictionary<TKey2, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TValue> : Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TKey4, TValue> : Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TKey4, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TValue> : Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TKey4, TKey5, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue> :
        Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue> :
        Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
        where TKey7 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> :
        Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
        where TKey7 : notnull
        where TKey8 : notnull
    { }

    public class NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TKey9, TValue> :
        Dictionary<TKey1, NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TKey9, TValue>>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
        where TKey7 : notnull
        where TKey8 : notnull
        where TKey9 : notnull
    { }
}

public static class NAryDictExtensions
{
    public static NAryDictionary<TKey2, TValue>
        New<TKey1, TKey2, TValue>(
            this NAryDictionary<TKey1, TKey2, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TValue>
        New<TKey1, TKey2, TKey3, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TKey4, TValue>
        New<TKey1, TKey2, TKey3, TKey4, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TKey4, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TKey4, TKey5, TValue>
        New<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>
        New<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>
        New<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
        where TKey7 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>
        New<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
        where TKey7 : notnull
        where TKey8 : notnull
    {
        return [];
    }

    public static NAryDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TKey9, TValue>
        New<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TKey9, TValue>(
            this NAryDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TKey9, TValue> dictionary
        )
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
        where TKey5 : notnull
        where TKey6 : notnull
        where TKey7 : notnull
        where TKey8 : notnull
        where TKey9 : notnull
    {
        return [];
    }
}