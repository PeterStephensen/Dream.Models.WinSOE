using Dream.Models.WinSOE;

namespace ConsoleSOE
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string outputDir = "";
            if(args.Length > 0)
            {
                outputDir = args[0];
            }
            
            int seed = (new Random()).Next();

            new SimulationRunner(saveScenario: true, winFormElements: null, shock: EShock.Base, seed: seed, atw: null, outputDir);

            new SimulationRunner(saveScenario: true, winFormElements: null, shock: EShock.Productivity, seed: seed, atw: null, outputDir);

            new SimulationRunner(saveScenario: true, winFormElements: null, shock: EShock.Tsunami, seed: seed, atw: null, outputDir);


        }
    }
}
