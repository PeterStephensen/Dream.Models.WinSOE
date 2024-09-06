using System.ComponentModel;
#if WIN_APP
using Dream.Models.WinSOE.UI;
#endif

namespace Dream.Models.WinSOE
{

    #region Class WinFormElements
    /// <summary>
    /// Class used to communicate with backgroundworker
    /// </summary>
    public class WinFormElements
    {
#if WIN_APP
        public MainFormUI MainFormUI { get; set; }
        public DoWorkEventArgs DoWorkEventArgs { get; set; }
        public ArgsToWorker? ArgsToWorkerScenario  { get; set; }

        public WinFormElements(MainFormUI mainFormUI, DoWorkEventArgs doWorkEventArgs) 
        { 
            MainFormUI = mainFormUI;
            DoWorkEventArgs = doWorkEventArgs;
        }
#endif
    }
    #endregion

#if !WIN_APP
    //Dummy implementation
    public class ArgsToWorker
    {
    }
#endif

    public class SimulationRunner
    {
        public SimulationRunner(bool saveScenario = false, WinFormElements? winFormElements = null,
                                EShock shock = EShock.Base, int seed = -1, ArgsToWorker? atw=null, string outputDir="")
        {
           
            Settings settings = new();
            settings.SaveScenario = saveScenario;

            ArgsToWorker? argsToWorker = null;


            if(saveScenario)
            {
                settings.Shock = shock;
                settings.RandomSeed = seed;

#if WIN_APP

                if (winFormElements != null) 
                    winFormElements.ArgsToWorkerScenario = atw;
#endif
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

            settings.FirmParetoMinPhiInitial = 1.9;  //1.9

            settings.FirmAlpha = 0.5;
            settings.FirmFi = 2;   //2

            //-----
            double mark = 0.05; // 0.15
            double sens = 1 / 0.75;   //0.25 * 1 / 0.75

            // Wage ----------------------------------
            settings.FirmWageMarkup = 1 * mark; //1                                              
            settings.FirmWageMarkupSensitivity = 2 * sens;//10
            settings.FirmWageMarkdown = 0.5 * mark;   //1            Overvej 0: Kører renten mod 0 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            settings.FirmWageMarkdownSensitivity = 2 * sens;//10

            // In zone
            settings.FirmWageMarkupInZone = 0 * mark; //1                                     
            settings.FirmWageMarkupSensitivityInZone = 1 * sens;//1
            settings.FirmWageMarkdownInZone = 0 * mark; //1    
            settings.FirmWageMarkdownSensitivityInZone = 1 * sens;//1

            settings.FirmProbabilityRecalculateWage = 1.0;   //1.0
            settings.FirmProbabilityRecalculateWageInZone = 2.0 / 12; //

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
            settings.FirmComfortZoneEmployment = 0.010;
            settings.FirmComfortZoneSales = 0.010;

            settings.FirmProbabilityRecalculatePrice = 1.0;
            settings.FirmProbabilityRecalculatePriceInZone = 2.0/12; // 0.5

            settings.FirmExpectedExcessPotentialSales = 1.0; // 
            settings.FirmExpectedSalesFraction = 0.9;       //0.8   zzz

            settings.FirmPriceMechanismStart = 12 * 1;

            //-----
            settings.FirmDefaultProbabilityNegativeProfit = 0.5;
            settings.FirmDefaultStart = 12 * 5;
            settings.FirmNegativeProfitOkAge = 12 * 2;

            settings.FirmExpectationSmooth = 0.95; //0.4  
            settings.FirmMaxEmployment = 100000;  // 1000            !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            settings.FirmEmploymentMarkup = 1.025;   // 1.5    zzz

            settings.FirmNumberOfGoodAdvertisements = 100; // 25 
            settings.FirmNumberOfJobAdvertisements = 15;   // 15!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            settings.FirmVacanciesShare = 1.0; //1.0
            settings.FirmMinRemainingVacancies = 5;

            settings.FirmProfitLimitZeroPeriod = (2040 - 2014) * 12;

            settings.FirmProductivityGrowth = 0.02;
           
            settings.FirmStockDepreciation = 0.25; //0.25

            // Households
            settings.HouseholdReservationWageReduction = 0.98;  // 0.9 !!!!!!!!!!!!!!!!!!!!!!
            settings.HouseholdNumberFirmsSearchJob = 4;  //15              
            settings.HouseholdNumberFirmsSearchJobNew = 4;  //15              
            settings.HouseholdNumberFirmsSearchShop = 15;       //15
            settings.HouseholdProbabilityQuitJob = 0.05;        // 0.02   // Defines unemplyment rate !!!!!!!!!!!!!!!!!
            settings.HouseholdProbabilityOnTheJobSearch = 0.15;   //0.05  zzz                        
            settings.HouseholdProbabilitySearchForShop = 0.15;     //0.25                    
            settings.HouseholdProductivityLogSigmaInitial = 0.6;
            settings.HouseholdProductivityLogMeanInitial = -0.5 * Math.Pow(settings.HouseholdProductivityLogSigmaInitial, 2); // Sikrer at forventet produktivitet er 1
            settings.HouseholdProductivityErrorSigma = 0.02;
            settings.HouseholdCES_Elasticity = 0.7;
            settings.HouseholdDisSaveRatePensioner = 0.01;
            settings.HouseholdDisSaveRateUnemployed = 0.05;
            settings.HouseholdSaveRate = 0.01;
            settings.NumberOfInheritors = 2;   // 5       
            settings.HouseholdMaxNumberShops = 15; // 5 When your supplier can not deliver: how many to seach for
            settings.HouseholdProbabilityReactOnAdvertisingJob = 0.25; //0.25   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            settings.HouseholdProbabilityReactOnAdvertisingGood = 0.05; //1
            settings.HouseholdPensionAge = 67 * 12;
            settings.HouseholdStartAge = 18 * 12;
            settings.HouseholdNumberFirmsLookingForGoods = 15;
            
            //settings.HouseholdProbabilityRecalculateBudget=0.1;

            settings.HouseholdMPCWealth = 0.005;   //0.25
            settings.HouseholdMPCIncome = 0.95;   //0.75
            settings.HouseholdMPCCapitalIncome = 0.1;   //0.75

            //settings.HouseholdMPCWealth = 0.0;
            //settings.HouseholdMPCIncome = 1.0;
            //settings.HouseholdMPCCapitalIncome = 1.0;
            settings.SimplificationConsumption = false;
            settings.SimplificationInterestRate = false;

            settings.HouseholdTargetWealthIncomeRatio = 30;   // 20
            settings.HouseholdIncomeSmooth = 0.95;  //0.98

            // Statistics
            settings.StatisticsInitialMarketPrice = 1.0;  //1.2
            settings.StatisticsInitialMarketWage = 1.0;   //0.2 
            //settings.StatisticsInitialInterestRate = Math.Pow(1 + 0.04, 1.0 / 12) - 1; // 3% p.a.
            settings.StatisticsInitialInterestRate = Math.Pow(1 + 0.1, 1.0 / 12) - 1; // 3% p.a.

            settings.StatisticsFirmReportSampleSize = 0.05 * 5 / scale;//0.1
            settings.StatisticsHouseholdReportSampleSize = 0.0051 * 5 / scale;

            settings.StatisticsExpectedSharpeRatioSmooth = 0.95; //0.7   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   


            if(outputDir=="")
            {
#if !LINUX_APP
                if (Environment.MachineName == "C1709161") // PSP's gamle maskine
                {
                    settings.OutputDir = @"C:\test\Dream.AgentBased.MacroModel";
                    settings.RExe = @"C:\Program Files\R\R-4.0.3\bin\x64\R.exe";
                }
                if (Environment.MachineName == "C2210098") // PSP's nye maskine
                {
                    settings.OutputDir = @"C:\Users\B007566\Documents\Output";
                    settings.RExe = @"C:\Program Files\R\R-4.3.0\bin\x64\R.exe";
                    //settings.RExe = @"C:\Program Files\R\R-4.2.3\bin\R.exe";
                }


                if (Environment.MachineName == "VDI00316") // Fjernskrivebord
                {
                    settings.OutputDir = @"C:\Users\B007566\Documents\Output";
                    settings.RExe = @"C:\Users\B007566\Documents\R\R-4.1.2\bin\x64\R.exe";
                }

                if (Environment.MachineName == "VDI00382") // Fjernskrivebord til Agentbased projekt
                {
                    //settings.ROutputDir = @"C:\Users\B007566\Documents\Output";
                    settings.OutputDir = @"H:\AgentBased\SOE\Output";
                    settings.RExe = @"C:\Users\B007566\Documents\R\R-4.1.3\bin\x64\R.exe";
                }
#else
            settings.OutputDir = "/dpdream/home/dpetste/Projects/Output";
            settings.RExe = "";           
#endif
            }
            else
            {
                settings.OutputDir = outputDir;
            }
            
            // Time and random seed           
            settings.StartYear = 0; 
            settings.EndYear = 1300;  //300         //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!      
            settings.PeriodsPerYear = 12;
            settings.ShockPeriod = 200 * 12;

            settings.ConsoleOutput = EConsoleOutput.EventDistribution;

            settings.StatisticsOutputPeriode = 60 * 12;   
            settings.StatisticsGraphicsPlotInterval = 1;

            settings.StatisticsGraphicsStartPeriod = 65 * 12 * 100;     
            if (settings.SaveScenario)
                settings.StatisticsGraphicsStartPeriod = 12 * 500;

            settings.UIChartUpdateInterval = 1 * 12;  //5 * 12 
            settings.UIChartTimeWindow = 50 * 12;

            if(!settings.SaveScenario)
            {
                //settings.Shock = EShock.Productivity;
                //settings.Shock = EShock.Tsunami;
                settings.ShockSize = 0.25;
                settings.ShockPeriod = 1400 * 12;
            }

            settings.HouseholdTheory = EHouseholdTheory.BufferStock;

            //if (!saveScenario)
            //    settings.RandomSeed = 123;

            settings.BurnInPeriod1 = 25 * 12;    
            settings.BurnInPeriod2 = 40 * 12;    
            settings.BurnInPeriod3 = 100 * 12;  // 100    
            settings.StatisticsWritePeriode = 60000 * 12;

            settings.InvestorBuildUpPeriods = settings.BurnInPeriod3;

            // !!!!! Remember some settings are changed in Simulation after BurnIn1 !!!!!

            #region RandomParameters
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
            #endregion

#if !WIN_APP
            settings.EndYear = 300;
            settings.ShockPeriod = 200 * 12;
            settings.Shock = shock;
#endif


            settings.NewScenarioDirs = true;
            Time time = new Time(0, (1 + settings.EndYear - settings.StartYear) * settings.PeriodsPerYear - 1);

            // Run the simulation
            new Simulation(settings, time, winFormElements);

        }

    }
}
