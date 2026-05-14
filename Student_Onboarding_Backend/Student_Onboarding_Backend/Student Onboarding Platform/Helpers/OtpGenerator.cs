using System.Security.Cryptography;

namespace Student_Onboarding_Platform.Helpers;

public static class OtpGenerator
{
    public static string Generate(int length = 6)
    {
        var max = (int)Math.Pow(10, length);
        var otp = RandomNumberGenerator.GetInt32(0, max);
        return otp.ToString().PadLeft(length, '0');
    }
}
