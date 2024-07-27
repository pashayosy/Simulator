using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace FlightSimulator
{
    class Program
    {
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        private static readonly HttpClient client = new HttpClient(clientHandler);

        static async Task Main(string[] args)
        {
            client.BaseAddress = new Uri("http://localhost:5084");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            while (true)
            {
                var flight = CreateRandomFlight();
                var json = JsonConvert.SerializeObject(flight);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/FlightOperations/add-flight", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Flight successfully added:");
                }
                else
                {
                    Console.WriteLine("Failed to add flight:");
                    Console.WriteLine(response.StatusCode);
                }

                await GetFlights();
                await Task.Delay(5000);
                //Console.WriteLine("\nPress any key to add another flight...");
                //Console.ReadLine();
            }
        }

        private static async Task GetFlights()
        {
            var getFlight = await client.GetAsync("/api/FlightOperations/flights");

            if (getFlight.IsSuccessStatusCode)
            {
                Console.WriteLine("Flights getted successfully");
                var flights = await getFlight.Content.ReadAsStringAsync();

                var allFlights = JsonConvert.DeserializeObject<IEnumerable<Flight>>(flights);

                foreach (var flight in allFlights)
                {
                    if (flight != null && flight.CurrentLeg.Id != 10 && flight.CurrentLeg.Id != -1)
                    {
                        Console.Write($"{flight.Id} Flight {flight.Number} ");

                        if (flight.CurrentLeg != null)
                        {
                            Console.Write($"- Current {flight.CurrentLeg.Name}");
                        }
                        Console.WriteLine("\n");
                    }

                }

            }
            else
            {
                Console.WriteLine("Faild to get log: " + getFlight.StatusCode);
            }

        }

        private static async Task GetLog()
        {
            var getLog = await client.GetAsync("/api/FlightOperations/logs");

            if (getLog.IsSuccessStatusCode)
            {
                Console.WriteLine("Log getted successfully");
                var log = await getLog.Content.ReadAsStringAsync();

                var flightLogs = JsonConvert.DeserializeObject<IEnumerable<FlightLog>>(log);

                foreach (var flightLog in flightLogs)
                {
                    Console.Write($"Fligh log {flightLog.Id} | Fligh {flightLog.FlightId}: In - {flightLog.In} ");
                    if (flightLog.Out != null)
                        Console.Write($"out - {flightLog.Out}\n");
                    Console.WriteLine("\n");
                }

            }
            else
            {
                Console.WriteLine("Faild to get log: " + getLog.StatusCode);
            }

        }


        private static Flight CreateRandomFlight()
        {
            var random = new Random();
            return new Flight
            {
                Number = "Flight-" + random.Next(1000, 9999),
                PassengersCount = random.Next(50, 200),
                Brand = "Brand-" + random.Next(1, 10),
                Status = FlightStatus.Landing,
                LegId = null
            };
        }
    }

    public class Flight
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int PassengersCount { get; set; }
        public string Brand { get; set; }
        public FlightStatus Status { get; set; }
        public virtual Leg CurrentLeg { get; set; }
        public int? LegId { get; set; }
    }

    public class FlightLog
    {
        public int Id { get; set; }

        public int FlightId { get; set; }

        public virtual Flight? Flight { get; set; }

        public int LegId { get; set; }
        public virtual Leg? Leg { get; set; }

        public DateTime In { get; set; }

        public DateTime? Out { get; set; }
    }

    public class Leg
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Capacity { get; set; }

        public FlightStatus Type { get; set; }

        public int CrossingTime { get; set; }

        public ICollection<Flight>? CurrectFlights { get; set; }
    }

    [Flags]
    public enum FlightStatus
    {
        None = 0,
        Landing = 1 << 0,
        Arrival = 1 << 1,
        Departure = 1 << 2
    }
}
