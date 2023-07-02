using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dream.Models.WinSOE.UI
{
    public partial class ScenariosForm : Form
    {

        MainFormUI _mainFormUI;
        public int[][] Seeds;

        public ScenariosForm(MainFormUI mainFormUI)
        {
            _mainFormUI = mainFormUI;

            InitializeComponent();

            this.KeyPreview = true;

            Type enumType = typeof(EShock);
            var enumValues = enumType.GetEnumValues();

            foreach (var enumValue in enumValues)
            {
                if (enumValue.ToString() != "Nothing")
                    checkedListBoxScenarioShocks.Items.Add(enumValue);
            }

            labelScenariosTimeLeftValue.Text = "";
            labelScenariosTimeUsedValue.Text = "";
        }

        private void buttonScenariosCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void buttonScenariosRun_Click(object sender, EventArgs e)
        {
            List<EShock> shks = new();
            foreach (var itt in checkedListBoxScenarioShocks.CheckedItems)
                shks.Add((EShock)itt);

            int numberOfCycles = Convert.ToInt32(textBoxScenarioNumberCycles.Text);
            int numberOfThreads = Convert.ToInt32(textBoxScenarioNumberThreads.Text);

            if(checkBoxScenariosUseBaseRuns.Checked)
            {
                string[] files = Directory.GetFiles(@"C:\Users\b007566\Documents\Output\Scenarios\Macro", "base_*.txt");
                if(numberOfCycles*numberOfThreads!=files.Count())
                {
                    MessageBox.Show("Wrong number of base-files","Error",buttons:MessageBoxButtons.OK,  icon: MessageBoxIcon.Error);
                    return;
                }

                Seeds = new int[numberOfCycles][];
                for(int c = 0; c<numberOfCycles;c++)
                    Seeds[c] = new int[numberOfThreads];
                
                int j = 0;
                for(int c=0; c<numberOfCycles; c++)
                    for(int t=0; t<numberOfThreads; t++)
                    {
                        string[] parts = files[j].Split("_");
                        Seeds[c][t] = Convert.ToInt32(parts[1]);

                        j++;
                    }
            }

            for (int i = 0; i < numberOfThreads; i++)
                _mainFormUI.runBackgroundWorkerScenarios(new ArgsToWorker(i, shks, numberOfCycles, numberOfThreads));

        }
    }

    public class ArgsToWorker
    {
        public List<EShock> Shocks { get; set; }
        public int NumberOfCycles { get; set; }
        public int CurrentCycle { get; set; }

        public int NumberOfThreads { get; set; }
        public int ID { get; set; }

        public ArgsToWorker(int id, List<EShock> shocks, int numberOfCycles, int numberOfThreads)
        {
            Shocks = new(shocks);
            NumberOfCycles = numberOfCycles;
            NumberOfThreads = numberOfThreads;
            ID = id;
            CurrentCycle = 0;
        }
    }
}
