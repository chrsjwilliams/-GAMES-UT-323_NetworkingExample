using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class Utils
{
    public static void Shuffle<T>(this IList<T> list, Random rnd)
    {
        for (var i = list.Count - 1; i >= 0; i--)
            list.Swap(i, rnd.Next(0, i));
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    public static byte[] ToBytes(System.Object obj)
    {
        if (obj == null) return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);

        return ms.ToArray();
    }

    public static System.Object ToObject(byte[] arrBytes)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        ms.Write(arrBytes, 0, arrBytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        System.Object obj = (System.Object)bf.Deserialize(ms);

        return obj;
    }
}