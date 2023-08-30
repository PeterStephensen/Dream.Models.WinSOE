using Dream.Models.WinSOE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Dream.Models.WinSOE.UI;

namespace Dream.Models.WinSOE
{
    
    /// <summary>
    /// Class used to communicate with backgroundworker
    /// </summary>
    public class WinFormElements
    {
        public MainFormUI MainFormUI { get; set; }
        public DoWorkEventArgs DoWorkEventArgs { get; set; }
        public ArgsToWorker? ArgsToWorkerScenario  { get; set; }

    public WinFormElements(MainFormUI mainFormUI, DoWorkEventArgs doWorkEventArgs) 
        { 
            MainFormUI = mainFormUI;
            DoWorkEventArgs = doWorkEventArgs;
        }

    }
    
    public class SimulationRunner
    {
        public SimulationRunner(bool saveScenario = false, WinFormElements? winFormElements = null,
                                EShock shock = EShock.Nothing, int seed = -1, ArgsToWorker? atw=null)
        {
           
            Settings settings = new();
            settings.SaveScenario = saveScenario;
            ArgsToWorker? argsToWorker = null;
            if(saveScenario)
            {
                settings.Shock = shock;
                settings.RandomSeed = seed;
                
                if(winFormElements != null) 
                    winFormElements.ArgsToWorkerScenario = atw;
            }

            // Scale
            double scale = 5 * 1.0; //5

            settings.NumberOfSectors = 1;
            settings.NumberOfFirms = (int)(150 * scale); 
            settings.NumberOfHouseholdsPerFirm = 1*5;  //5
            settings.HouseholdNewBorn = (int)(6 * scale);   //5   
            settings.InvestorInitialInflow = (int)(10 * scale);
            settings.HouseholdNumberShoppingsPerPeriod = 4; // Weekly consumption

            //Firms
            settings.FirmParetoMinPhi = 0.5;   //0.5
            settings.FirmPareto_k = 2.5;  // 2.5 k * (1 - alpha) > 1     
            settings.FirmParetoCensorTop = 0.001;

            settings.FirmParetoMinPhiInitial = 1.9;

            settings.FirmAlpha = 0.5;
            settings.FirmFi = 2;

            //-----
            double mark = 0.05; // 0.2
            double sens = 1 / 0.75;   //1/0.1


            // Wage ----------------------------------
            settings.FirmWageMarkup = 1 * mark; //1                                              
            settings.FirmWageMarkupSensitivity = 2 * sens;//10
            settings.FirmWageMarkdown = 1 * mark;   //1          
            settings.FirmWageMarkdownSensitivity = 2 * sens;//10

            // In zone
            settings.FirmWageMarkupInZone = 0 * mark; //1                                     
            settings.FirmWageMarkupSensitivityInZone = 1 * sens;//1
            settings.FirmWageMarkdownInZone = 0 * mark; //1    
            settings.FirmWageMarkdownSensitivityInZone = 1 * sens;//1

            settings.FirmProbabilityRecalculateWage = 1.0;
            settings.FirmProbabilityRecalculateWageInZone = 2.0/12; //0.5

            // Price ----------------------------------
            settings.FirmPriceMarkup = 1 * mark; //1
            settings.FirmPriceMarkupSensitivity = 2 * sens; //10
            settings.FirmPriceMarkdown = 1 * mark;
            settings.FirmPriceMarkdownSensitivity = 2 * sens;  //10  

            // In zone
            settings.FirmPriceMarkupInZone = 0 * mark; //1
            settings.FirmPriceMarkupSensitivityInZone = 1 * sens;
            settings.FirmPriceMarkdownInZone = 0 * mark;  //0             
            settings.FirmPriceMarkdownSensitivityInZone = 1 * sens;

            //-----
            settings.FirmComfortZoneEmployment = 0.10;
            settings.FirmComfortZoneSales = 0.10;

            settings.FirmProbabilityRecalculatePrice = 1.0;
            settings.FirmProbabilityRecalculatePriceInZone = 2.0/12; // 0.5

            settings.FirmExpectedExcessPotentialSales = 1.0; // 
            settings.FirmGamma_y = 0.8; //1.0   !!!!!!!!!!!!!!!!!!!!!!!!!

            settings.FirmPriceMechanismStart = 12 * 1;


            //-----
            settings.FirmDefaultProbabilityNegativeProfit = 0.5;
            settings.FirmDefaultStart = 12 * 5;
            settings.FirmNegativeProfitOkAge = 12 * 2;

            settings.FirmExpectationSmooth = 0.95; //0.4  
            settings.FirmMaxEmployment = 100000;  // 1000            !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            settings.FirmEmploymentMarkup = 1.5;

            settings.FirmNumberOfGoodAdvertisements = 100; // 25 
            settings.FirmNumberOfJobAdvertisements = 25;

            settings.FirmVacanciesShare = 1.0;
            settings.FirmMinRemainingVacancies = 5;

            settings.FirmProfitLimitZeroPeriod = (2040 - 2014) * 12;

            settings.FirmProductivityGrowth = 0.02;

            settings.FirmStockDepreciation = 0.25;

            // Households
            settings.HouseholdNumberFirmsSearchJob = 15;  //15              
            settings.HouseholdNumberFirmsSearchJobNew = 15;  //15              
            settings.HouseholdNumberFirmsSearchShop = 15;       //15
            settings.HouseholdProbabilityQuitJob = 0.02;        // 0.01
            settings.HouseholdProbabilityOnTheJobSearch = 0.05;   //0.25                        
            settings.HouseholdProbabilitySearchForShop = 0.15;     //0.25                    
            settings.HouseholdProductivityLogSigmaInitial = 0.6;
            settings.HouseholdProductivityLogMeanInitial = -0.5 * Math.Pow(settings.HouseholdProductivityLogSigmaInitial, 2); // Sikrer at forventet produktivitet er 1
            settings.HouseholdProductivityErrorSigma = 0.02;
            settings.HouseholdCES_Elasticity = 0.7;
            settings.HouseholdDisSaveRatePensioner = 0.01;
            settings.HouseholdDisSaveRateUnemployed = 0.05;
            settings.HouseholdSaveRate = 0.01;
            settings.NumberOfInheritors = 5;
            settings.HouseholdMaxNumberShops = 15; // 5 When your supplier can not deliver: how many to seach for
            settings.HouseholdProbabilityReactOnAdvertisingJob = 0.25; //1
            settings.HouseholdProbabilityReactOnAdvertisingGood = 0.05; //1
            settings.HouseholdPensionAge = 67 * 12;
            settings.HouseholdStartAge = 18 * 12;
            settings.HouseholdNumberFirmsLookingForGoods = 15;
            settings.HouseholdMPCWealth = 0.25;
            settings.HouseholdMPCIncome = 0.75;
            settings.HouseholdTargetWealthIncomeRatio = 20;   // 6
            settings.HouseholdIncomeSmooth = 0.98;

            settings.HouseholdProfitShare = 1; // Should be 1 !!!!!!!!!!!!!!!!!!

            // Investor
            //settings.InvestorProfitSensitivity = 0.05; //0.15               

            // Statistics
            settings.StatisticsInitialMarketPrice = 1.0;  //1.2
            settings.StatisticsInitialMarketWage = 1.0;   //0.2 
            settings.StatisticsInitialInterestRate = Math.Pow(1 + 0.2, 1.0 / 12) - 1; // 5% p.a.

            settings.StatisticsFirmReportSampleSize = 0.05 * 5 / scale;//0.1
            settings.StatisticsHouseholdReportSampleSize = 0.0051 * 5 / scale;

            settings.StatisticsExpectedSharpeRatioSmooth = 0.7;

            // R-stuff
            if (Environment.MachineName == "C1709161") // PSP's gamle maskine
            {
                settings.ROutputDir = @"C:\test\Dream.AgentBased.MacroModel";
                settings.RExe = @"C:\Program Files\R\R-4.0.3\bin\x64\R.exe";
            }
            if (Environment.MachineName == "C2210098") // PSP's nye maskine
            {
                settings.ROutputDir = @"C:\Users\B007566\Documents\Output";
                settings.RExe = @"C:\Program Files\R\R-4.3.0\bin\x64\R.exe";
                //settings.RExe = @"C:\Program Files\R\R-4.2.3\bin\R.exe";
            }

            if (Environment.MachineName == "VDI00316") // Fjernskrivebord
            {
                settings.ROutputDir = @"C:\Users\B007566\Documents\Output";
                settings.RExe = @"C:\Users\B007566\Documents\R\R-4.1.2\bin\x64\R.exe";
            }

            if (Environment.MachineName == "VDI00382") // Fjernskrivebord til Agentbased projekt
            {
                //settings.ROutputDir = @"C:\Users\B007566\Documents\Output";
                settings.ROutputDir = @"H:\AgentBased\SOE\Output";
                settings.RExe = @"C:\Users\B007566\Documents\R\R-4.1.3\bin\x64\R.exe";
            }

            // Time and randomseed           
            settings.StartYear = 0; 
            settings.EndYear = 300;            // 300  
            settings.PeriodsPerYear = 12;

            settings.ShockPeriod = 200 * 12;   // 200

            settings.StatisticsOutputPeriode = 60 * 12;   
            settings.StatisticsGraphicsPlotInterval = 1;

            settings.StatisticsGraphicsStartPeriod = 65 * 12 * 100;     //!!!!!!!!!!!!!!!!!!!!!!!
            if (settings.SaveScenario)
                settings.StatisticsGraphicsStartPeriod = 12 * 500;

            if (!saveScenario)
            {
                settings.RandomSeed = 100;  
            }


            settings.BurnInPeriod1 = 25 * 12;    // (2030 - 2014) * 12;  //35
            settings.BurnInPeriod2 = 40 * 12;    // (2035 - 2014) * 12;  //50
            settings.BurnInPeriod3 = 100 * 12;    //(2035 - 2014) * 12;  //50
            settings.StatisticsWritePeriode = 60000 * 12;     //(2075 - 2014) * 12;

            // !!!!! Remember some settings are changed in Simulation after BurnIn1 !!!!!
            
            //settings.BurnInPeriod1 = 1;
            ////settings.BurnInPeriod2 = 112 * 5;
            //settings.FirmProfitLimitZeroPeriod = 1;
            //settings.FirmDefaultStart = 1;
            //settings.LoadDatabase = true;

            settings.RandomParameters = false;
            //if (settings.RandomParameters)
            //{

            //    if (args.Length != 1)   // Base-run
            //    {
            //        Random rnd = new();
            //        settings.InvestorProfitSensitivity = rnd.NextDouble(0.2, 0.8);

            //        double m = rnd.NextDouble(0.05, 0.25);
            //        double s = rnd.NextDouble(5.0, 20);
            //        settings.FirmPriceMarkup = m;
            //        settings.FirmPriceMarkupInZone = m;
            //        settings.FirmPriceMarkupSensitivity = s;
            //        settings.FirmPriceMarkupSensitivityInZone = s;

            //        m = rnd.NextDouble(0.05, 0.25);
            //        s = rnd.NextDouble(5.0, 20);
            //        settings.FirmPriceMarkdown = m;
            //        settings.FirmPriceMarkdownInZone = m;
            //        settings.FirmPriceMarkdownSensitivity = s;
            //        settings.FirmPriceMarkdownSensitivityInZone = s;

            //        m = rnd.NextDouble(0.05, 0.25);
            //        s = rnd.NextDouble(10, 30);
            //        settings.FirmWageMarkup = m;
            //        settings.FirmWageMarkupInZone = m;
            //        settings.FirmWageMarkupSensitivity = s;
            //        settings.FirmWageMarkupSensitivityInZone = s;

            //        m = rnd.NextDouble(0.05, 0.25);
            //        s = rnd.NextDouble(10, 30);
            //        settings.FirmWageMarkdown = m;
            //        settings.FirmWageMarkdownInZone = m;
            //        settings.FirmWageMarkdownSensitivity = s;
            //        settings.FirmWageMarkdownSensitivityInZone = s;

            //    }
            //    else   // Counterfactuals
            //    {
            //        //settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settings.ROutputDir + "\\last_json.json"));
            //        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //    }
            //}

            //if (args.Length == 1)
            //{
            //    //settings.Shock = EShock.Tsunami;
            //    //settings.IDScenario = Int32.Parse(args[0]);
            //    settings.Shock = (EShock)Int32.Parse(args[0]);
            //    settings.ShockPeriod = (2105 - 2014) * 12;
            //}

            //settings.Shock = EShock.LaborSupply;


            settings.NewScenarioDirs = true;
            var t0 = DateTime.Now;
            Time time = new Time(0, (1 + settings.EndYear - settings.StartYear) * settings.PeriodsPerYear - 1);

            // Run the simulation
            new Simulation(settings, time, winFormElements);
            //new Simulation(settings, time, winFormElements, baseRun);

            Console.Write("\n");
            Console.WriteLine(DateTime.Now - t0);



        }

    }
}
