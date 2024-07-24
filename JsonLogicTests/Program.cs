using JsonLogic.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace JsonLogicTests
{
    internal class Program
    {
        private record TestCase(decimal MonthlyVolume, decimal AverageTicket);
        static void Main()
        {
            var testCases = new List<TestCase>()
            {
                new(1000000, 10000), //4
                new(40000, 6000), //2
                new(10000, 10), //1
                new(30000, 30000), //3
                new(2000000, 100), //4
            };
            for (int i = 1; i <= testCases.Count; i++)
            {
                var testCase = testCases[i - 1];
                var monthlyVolume = testCase.MonthlyVolume;
                var averageTicket = testCase.AverageTicket;
                var appLevel = GetAppLevel(monthlyVolume, averageTicket);
                Console.WriteLine(@$"Test Case {i}: {nameof(monthlyVolume)}={monthlyVolume}, {nameof(averageTicket)}={averageTicket}, result={appLevel}");
            }
        }

        static int GetAppLevel(decimal monthlyVolume, decimal averageTicket)
        {
            // Rule definition retrieved as JSON text
            var jsonText = @"{
                ""if"": [
                    {"">"": [{ ""var"":""monthlyVolume""}, 1000000] }, 4,
                    {""or"": [
                        { "">"": [{ ""var"":""monthlyVolume""}, 166666] },
                        { "">"": [{ ""var"":""averageTicket""}, 25000] }]
                    }, 3,
                    {""or"": [
                        { "">"": [{ ""var"":""monthlyVolume""}, 50000] },
                        { "">"": [{ ""var"":""averageTicket""}, 1000] }]
                    }, 2, 1]
            }";

            //string that would be stored in AWS param store:
            //{\"if\": [{\">\": [{ \"var\":\"monthlyVolume\"}, 2000000] }, 4, {\"or\": [{ \">\": [{ \"var\":\"monthlyVolume\"}, 166666] }, { \">\": [{ \"var\":\"averageTicket\"}, 25000] }]}, 3, {\"or\": [{ \">\": [{ \"var\":\"monthlyVolume\"}, 50000] }, { \">\": [{ \"var\":\"averageTicket\"}, 1000] }] }, 2, 1]}

            // Parse json into hierarchical structure
            var rule = JsonNode.Parse(jsonText);
            var data = JsonNode.Parse(JsonConvert.SerializeObject(new { monthlyVolume, averageTicket }, Formatting.None));
            JsonNode? result = Json.Logic.JsonLogic.Apply(rule, data);

            if (result != null)
                return (int)result.AsValue();
            else
                return 1;
        }



        static int GetAppLevel_old(decimal monthlyVolume, decimal averageTicket)
        {
            var data = new { monthlyVolume, averageTicket };

            // Rule definition retrieved as JSON text
            var jsonText = @"{
                ""if"": [
                    {"">"": [{ ""var"":""monthlyVolume""}, 1000000] }, 4,
                    {""or"": [
                        { "">"": [{ ""var"":""monthlyVolume""}, 166666] },
                        { "">"": [{ ""var"":""averageTicket""}, 25000] }]
                    }, 3,
                    {""or"": [
                        { "">"": [{ ""var"":""monthlyVolume""}, 50000] },
                        { "">"": [{ ""var"":""averageTicket""}, 1000] }]
                    }, 2, 1]
            }";

            //string that would be stored in AWS param store:
            //{\"if\": [{\">\": [{ \"var\":\"monthlyVolume\"}, 2000000] }, 4, {\"or\": [{ \">\": [{ \"var\":\"monthlyVolume\"}, 166666] }, { \">\": [{ \"var\":\"averageTicket\"}, 25000] }]}, 3, {\"or\": [{ \">\": [{ \"var\":\"monthlyVolume\"}, 50000] }, { \">\": [{ \"var\":\"averageTicket\"}, 1000] }] }, 2, 1]}

            // Parse json into hierarchical structure
            var rule = JObject.Parse(jsonText);

            // Create an evaluator with default operators.
            var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

            // Apply the rule to the data.
            var result = evaluator.Apply(rule, data);

            return Convert.ToInt32(result);
        }
    }
}
