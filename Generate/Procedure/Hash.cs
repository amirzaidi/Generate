namespace Generate.Procedure
{
    class Hash
    {
        internal static byte[] MD5(byte[] Input)
        {
            return System.Security.Cryptography.MD5.Create().ComputeHash(Input);
        }
    }
}
