using NuGet.Common;
using Stripe;

namespace AuthSystem.Card
{
    public class Card
    {
        static async Task Main()
        {
            StripeConfiguration.ApiKey = "sk_test_51Qx6deCyL05b1ICGxSbGhc1RQVRKrbte6ppjFfMxojs5QXWb4JjvzCzRhY1s2IwI0T3qbNyA2CUb5rSatZnJsMQ900rM4ELdoM";

            var options = new Stripe.TokenCreateOptions
            {
                Card = new Stripe.TokenCardOptions
                {
                    Number = "4242424242424242",
                    ExpMonth = "12",
                    ExpYear = "2025",
                    Cvc = "123",
                },
            };
            var service = new Stripe.TokenService();
            Stripe.Token token = await service.CreateAsync(options);
            Console.WriteLine("Token testowy: " + token.Id);
        }
    }
}