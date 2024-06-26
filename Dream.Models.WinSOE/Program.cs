namespace Dream.Models.WinSOE
{

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

#if WIN_APP
            ApplicationConfiguration.Initialize();
            Application.Run(new MainFormUI());
#endif

#if !WIN_APP
            var t0 = DateTime.Now;

            int seed = (new Random()).Next();

            new SimulationRunner(saveScenario: true, winFormElements: null, shock: EShock.Base,
                            seed: seed, atw: null);

            new SimulationRunner(saveScenario: true, winFormElements: null, shock: EShock.Productivity,
                            seed: seed, atw: null);

            Console.WriteLine("Time used: {0}", DateTime.Now-t0);


#endif


        }

    }
}