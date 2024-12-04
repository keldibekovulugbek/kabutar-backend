namespace Kabutar.Service.Security;

public static class PasswordHasher
{
    // Hashes the password using BCrypt and automatically generates a salt.
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verifies the password against a given hash using BCrypt's built-in salt handling.
    public static bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
