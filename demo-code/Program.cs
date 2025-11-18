using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;

using OllamaSharp;

namespace IAsExtraordinarias
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string ByeCommand = "farewell";

            const ConsoleColor colorMistral = ConsoleColor.Cyan;
            const ConsoleColor colorLlama = ConsoleColor.Magenta;
            const ConsoleColor colorPhi = ConsoleColor.Green;

            // Cargar configuración desde appsettings.jsonls
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            var configMistral = config.GetSection("MistralModelConfiguration").Get<Config>()!;
            var configLlama = config.GetSection(@"LlamaModelConfiguration").Get<Config>()!;
            var configPhi = config.GetSection(@"PhiModelConfiguration").Get<Config>()!;

            var agentMistral = CreateAgent(configMistral);
            var agentLlama = CreateAgent(configLlama);
            var agentPhi = CreateAgent(configPhi);

            var modelMap = new Dictionary<string, (AIAgent agent, ConsoleColor color, string agentName, string intro)>
            {
                { @"1", (agentMistral, colorMistral, "Lady Mistral", "Good day, this is Lady Mistral spaeking. How may I assist you today?") },
                { @"2", (agentLlama, colorLlama, "Mister Llama", "Good day, this is Mister Llama spaking, Intelligence Manager. What is the mission?") },
                { @"3", (agentPhi, colorPhi, "The incredible Phi", "You've reached Phi. Speak swiftly and state your purpose") }
            };


            var chatHistory = new List<string>();
            string? userInput;


            do
            {
                DisplayWelcomeMessage();

                userInput = Console.ReadLine();
                if (userInput == ByeCommand) break;

                if (!string.IsNullOrWhiteSpace(userInput) && modelMap.TryGetValue(userInput, out var model))
                {
                    Console.ForegroundColor = model.color;
                    Console.WriteLine($"{model.agentName}: {model.intro}");
                    Console.ResetColor();
                    Console.WriteLine("Mission: ");
                    userInput = Console.ReadLine();
                    Console.WriteLine("Mission results");

                    Console.ForegroundColor = model.color;
                    string agentResponse = "";
                    await foreach (var update in model.agent.RunStreamingAsync(userInput))
                    {

                        Console.Write(update);
                        agentResponse += update;
                    }
                    Console.ResetColor();
                    chatHistory.Add($"Agent: {agentResponse}");
                }

            } while (true);
        }

        static void DisplayWelcomeMessage()
        {
            Console.WriteLine();
            Console.WriteLine("Welcome aboard the Nautilius submarine, the esteemed abode of the <<List of Extraordinary AIs>>.\nWe take upon ourselves the most ardous of missions, and it is not uncommon for us to rescue the world once or twice a week.");
            Console.WriteLine();
            Console.WriteLine("With which of our distinguished agents do you wish to make acquaintance?");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. Lady Mistral");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("2. Mister Llama");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("3. The incredible Phi");
            Console.ResetColor();
            Console.Write("Enter the number corresponding to your choice (or type 'farewell' to exit): ");
            Console.WriteLine();
        }

        static AIAgent CreateAgent(Config config)
        {
            OllamaApiClient chatClient = new(new Uri(@"http://localhost:11434"), config.Model);
            ////OllamaApiClient chatClient = new(new Uri(@"http://ollama-b591b.germanywestcentral.cloudapp.azure.com:11434"), config.Model);

            AIAgent agent = new ChatClientAgent(chatClient, instructions: config.Instructions, name: config.AgentName);
            
            return agent;
        }
    }

    public class Config
    {
        public string Model { get; set; } = string.Empty;
        
        public string AgentName { get; set; } = string.Empty;
        
        public string Instructions { get; set; } = string.Empty;
    }
}
