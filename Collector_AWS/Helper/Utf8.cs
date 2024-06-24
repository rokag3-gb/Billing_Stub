namespace Tmon.Collector.Helper;

public static class Utf8
{
    static UTF8Encoding utf8Enc = new UTF8Encoding();

    public static string str_to_base64string(string str)
    {
        byte[] byteArr = utf8Enc.GetBytes(str);

        return Convert.ToBase64String(byteArr);
    }
}