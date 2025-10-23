namespace Futopia.UserService.Domain.Helpers;
public static class UserIdGenerator
{
    private const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    private static readonly Random _random = new Random();

    public static string GenerateId(int length = 8)
    {
        var chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = Alphabet[_random.Next(Alphabet.Length)];
        }

        return "#" + new string(chars);
    }
}
