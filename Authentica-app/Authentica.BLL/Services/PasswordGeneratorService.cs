using Authentica.BLL.Interfaces;
using System.Security.Cryptography;

namespace Authentica.BLL.Services
{
    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits    = "0123456789";
        private const string Symbols   = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        private const string AllChars  = Uppercase + Lowercase + Digits + Symbols;

        public string Generate(int length)
        {
            if (length < 8 || length > 128)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 8 and 128.");

            var result = new char[length];

            result[0] = Pick(Uppercase);
            result[1] = Pick(Lowercase);
            result[2] = Pick(Digits);
            result[3] = Pick(Symbols);

            for (int i = 4; i < length; i++)
                result[i] = Pick(AllChars);

            for (int i = length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return new string(result);
        }

        private static char Pick(string charset) =>
            charset[RandomNumberGenerator.GetInt32(charset.Length)];
    }
}
