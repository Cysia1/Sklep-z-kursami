using NuGet.Common;
using Stripe;

namespace AuthSystem.Card
{
    public class Program
    {
        static async Task Main(string[] args) // To jest główny punkt wejścia
        {
            StripeConfiguration.ApiKey = "";

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