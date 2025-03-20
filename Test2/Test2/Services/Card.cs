using Stripe;

namespace Test2.Services
{
    public class Card
    {
        static async Task Main()
        {
            StripeConfiguration.ApiKey = "sk_test_51Qx6deCyL05b1ICGxSbGhc1RQVRKrbte6ppjFfMxojs5QXWb4JjvzCzRhY1s2IwI0T3qbNyA2CUb5rSatZnJsMQ900rM4ELdoM";

            var options = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = "4242424242424242",
                    ExpMonth = "12",
                    ExpYear = "2025",
                    Cvc = "123",
                },
            };
            var service = new TokenService();
            Token token = await service.CreateAsync(options);
            Console.WriteLine("Token testowy: " + token.Id);
        }
    }
}

