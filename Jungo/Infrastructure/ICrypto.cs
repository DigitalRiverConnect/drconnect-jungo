namespace Jungo.Infrastructure
{
    public interface ICrypto
    {
        bool TryEncrypt(string input, out string output);
        bool TryDecrypt(string input, out string output);
        string Encrypt(string input);
        byte[] Encrypt(byte[] input);
        string Decrypt(string input);
        byte[] Decrypt(byte[] input);
    }
}
