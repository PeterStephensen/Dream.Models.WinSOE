#if WIN_APP
namespace Dream.Models.WinSOE.UI
{
    partial class ScenariosForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonScenariosRun = new Button();
            buttonScenariosCancel = new Button();
            labelScenarioNumberCycles = new Label();
            labelScenarioNumberThreads = new Label();
            textBoxScenarioNumberCycles = new TextBox();
            textBoxScenarioNumberThreads = new TextBox();
            checkedListBoxScenarioShocks = new CheckedListBox();
            labelScenarioShocks = new Label();
            progressBarScenario = new ProgressBar();
            checkBoxScenarioDeleteFiles = new CheckBox();
            labelScenariosTimeLeft = new Label();
            labelScenariosTimeLeftValue = new Label();
            labelScenariosTotalTime = new Label();
            labelScenariosTimeUsedValue = new Label();
            checkBoxScenariosUseBaseRuns = new CheckBox();
            SuspendLayout();
            // 
            // buttonScenariosRun
            // 
            buttonScenariosRun.Location = new Point(46, 438);
            buttonScenariosRun.Name = "buttonScenariosRun";
            buttonScenariosRun.Size = new Size(75, 23);
            buttonScenariosRun.TabIndex = 0;
            buttonScenariosRun.Text = "&Run";
            buttonScenariosRun.UseVisualStyleBackColor = true;
            buttonScenariosRun.Click += buttonScenariosRun_Click;
            // 
            // buttonScenariosCancel
            // 
            buttonScenariosCancel.Location = new Point(190, 438);
            buttonScenariosCancel.Name = "buttonScenariosCancel";
            buttonScenariosCancel.Size = new Size(75, 23);
            buttonScenariosCancel.TabIndex = 1;
            buttonScenariosCancel.Text = "&Cancel";
            buttonScenariosCancel.UseVisualStyleBackColor = true;
            buttonScenariosCancel.Click += buttonScenariosCancel_Click;
            // 
            // labelScenarioNumberCycles
            // 
            labelScenarioNumberCycles.AutoSize = true;
            labelScenarioNumberCycles.Location = new Point(46, 33);
            labelScenarioNumberCycles.Name = "labelScenarioNumberCycles";
            labelScenarioNumberCycles.Size = new Size(102, 15);
            labelScenarioNumberCycles.TabIndex = 2;
            labelScenarioNumberCycles.Text = "Number of Cycles";
            // 
            // labelScenarioNumberThreads
            // 
            labelScenarioNumberThreads.AutoSize = true;
            labelScenarioNumberThreads.Location = new Point(46, 57);
            labelScenarioNumberThreads.Name = "labelScenarioNumberThreads";
            labelScenarioNumberThreads.Size = new Size(109, 15);
            labelScenarioNumberThreads.TabIndex = 3;
            labelScenarioNumberThreads.Text = "Number of Threads";
            // 
            // textBoxScenarioNumberCycles
            // 
            textBoxScenarioNumberCycles.Location = new Point(169, 30);
            textBoxScenarioNumberCycles.Name = "textBoxScenarioNumberCycles";
            textBoxScenarioNumberCycles.Size = new Size(44, 23);
            textBoxScenarioNumberCycles.TabIndex = 4;
            textBoxScenarioNumberCycles.Text = "20";
            textBoxScenarioNumberCycles.TextAlign = HorizontalAlignment.Center;
            // 
            // textBoxScenarioNumberThreads
            // 
            textBoxScenarioNumberThreads.Location = new Point(169, 54);
            textBoxScenarioNumberThreads.Name = "textBoxScenarioNumberThreads";
            textBoxScenarioNumberThreads.Size = new Size(44, 23);
            textBoxScenarioNumberThreads.TabIndex = 4;
            textBoxScenarioNumberThreads.Text = "8";
            textBoxScenarioNumberThreads.TextAlign = HorizontalAlignment.Center;
            // 
            // checkedListBoxScenarioShocks
            // 
            checkedListBoxScenarioShocks.FormattingEnabled = true;
            checkedListBoxScenarioShocks.Location = new Point(45, 166);
            checkedListBoxScenarioShocks.Name = "checkedListBoxScenarioShocks";
            checkedListBoxScenarioShocks.Size = new Size(220, 148);
            checkedListBoxScenarioShocks.TabIndex = 5;
            // 
            // labelScenarioShocks
            // 
            labelScenarioShocks.AutoSize = true;
            labelScenarioShocks.Location = new Point(45, 147);
            labelScenarioShocks.Name = "labelScenarioShocks";
            labelScenarioShocks.Size = new Size(47, 15);
            labelScenarioShocks.TabIndex = 6;
            labelScenarioShocks.Text = "Shocks:";
            // 
            // progressBarScenario
            // 
            progressBarScenario.Location = new Point(45, 344);
            progressBarScenario.Name = "progressBarScenario";
            progressBarScenario.Size = new Size(220, 23);
            progressBarScenario.TabIndex = 7;
            // 
            // checkBoxScenarioDeleteFiles
            // 
            checkBoxScenarioDeleteFiles.AutoSize = true;
            checkBoxScenarioDeleteFiles.Checked = true;
            checkBoxScenarioDeleteFiles.CheckState = CheckState.Checked;
            checkBoxScenarioDeleteFiles.Location = new Point(47, 89);
            checkBoxScenarioDeleteFiles.Name = "checkBoxScenarioDeleteFiles";
            checkBoxScenarioDeleteFiles.Size = new Size(103, 19);
            checkBoxScenarioDeleteFiles.TabIndex = 8;
            checkBoxScenarioDeleteFiles.Text = "Delete old files";
            checkBoxScenarioDeleteFiles.UseVisualStyleBackColor = true;
            // 
            // labelScenariosTimeLeft
            // 
            labelScenariosTimeLeft.AutoSize = true;
            labelScenariosTimeLeft.Location = new Point(46, 381);
            labelScenariosTimeLeft.Name = "labelScenariosTimeLeft";
            labelScenariosTimeLeft.Size = new Size(88, 15);
            labelScenariosTimeLeft.TabIndex = 9;
            labelScenariosTimeLeft.Text = "Time left (h:m):";
            // 
            // labelScenariosTimeLeftValue
            // 
            labelScenariosTimeLeftValue.AutoSize = true;
            labelScenariosTimeLeftValue.Location = new Point(149, 381);
            labelScenariosTimeLeftValue.Name = "labelScenariosTimeLeftValue";
            labelScenariosTimeLeftValue.Size = new Size(16, 15);
            labelScenariosTimeLeftValue.TabIndex = 10;
            labelScenariosTimeLeftValue.Text = "...";
            // 
            // labelScenariosTotalTime
            // 
            labelScenariosTotalTime.AutoSize = true;
            labelScenariosTotalTime.Location = new Point(46, 402);
            labelScenariosTotalTime.Name = "labelScenariosTotalTime";
            labelScenariosTotalTime.Size = new Size(96, 15);
            labelScenariosTotalTime.TabIndex = 11;
            labelScenariosTotalTime.Text = "Time used (h:m):";
            // 
            // labelScenariosTimeUsedValue
            // 
            labelScenariosTimeUsedValue.AutoSize = true;
            labelScenariosTimeUsedValue.Location = new Point(149, 402);
            labelScenariosTimeUsedValue.Name = "labelScenariosTimeUsedValue";
            labelScenariosTimeUsedValue.Size = new Size(16, 15);
            labelScenariosTimeUsedValue.TabIndex = 12;
            labelScenariosTimeUsedValue.Text = "...";
            // 
            // checkBoxScenariosUseBaseRuns
            // 
            checkBoxScenariosUseBaseRuns.AutoSize = true;
            checkBoxScenariosUseBaseRuns.Location = new Point(45, 114);
            checkBoxScenariosUseBaseRuns.Name = "checkBoxScenariosUseBaseRuns";
            checkBoxScenariosUseBaseRuns.Size = new Size(142, 19);
            checkBoxScenariosUseBaseRuns.TabIndex = 8;
            checkBoxScenariosUseBaseRuns.Text = "Use existing base runs";
            checkBoxScenariosUseBaseRuns.UseVisualStyleBackColor = true;
            // 
            // ScenariosForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(317, 506);
            Controls.Add(labelScenariosTimeUsedValue);
            Controls.Add(labelScenariosTotalTime);
            Controls.Add(labelScenariosTimeLeftValue);
            Controls.Add(labelScenariosTimeLeft);
            Controls.Add(checkBoxScenariosUseBaseRuns);
            Controls.Add(checkBoxScenarioDeleteFiles);
            Controls.Add(progressBarScenario);
            Controls.Add(labelScenarioShocks);
            Controls.Add(checkedListBoxScenarioShocks);
            Controls.Add(textBoxScenarioNumberThreads);
            Controls.Add(textBoxScenarioNumberCycles);
            Controls.Add(labelScenarioNumberThreads);
            Controls.Add(labelScenarioNumberCycles);
            Controls.Add(buttonScenariosCancel);
            Controls.Add(buttonScenariosRun);
            Name = "ScenariosForm";
            Text = "Run Scenarios";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonScenariosRun;
        private Button buttonScenariosCancel;
        internal Label labelScenarioNumberCycles;
        private Label labelScenarioNumberThreads;
        private TextBox textBoxScenarioNumberCycles;
        private TextBox textBoxScenarioNumberThreads;
        private CheckedListBox checkedListBoxScenarioShocks;
        private Label labelScenarioShocks;
        public ProgressBar progressBarScenario;
        private CheckBox checkBoxScenarioDeleteFiles;
        private Label labelScenariosTimeLeft;
        public Label labelScenariosTimeLeftValue;
        private Label labelScenariosTotalTime;
        public Label labelScenariosTimeUsedValue;
        public CheckBox checkBoxScenariosUseBaseRuns;
    }
}
#endif