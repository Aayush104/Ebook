namespace BookProject.Service
{
    public class OtpGenerate
    {
        private static readonly Random R = new Random();

        public static string GenerateOtp(int length = 6)
        {
            const string chars = "0123456789";
            char[] otp = new char[length];   

            for (int i = 0; i < length; i++)
            {
                otp[i] = chars[R.Next(chars.Length)]; 

            }

            return new string(otp);

        }
    }
}
