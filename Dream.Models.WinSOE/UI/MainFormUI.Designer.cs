namespace Dream.Models.WinSOE
{
    partial class MainFormUI
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFormUI));
            statusStripMainForm = new StatusStrip();
            toolStripStatusLabelMainForm = new ToolStripStatusLabel();
            toolStripStatusLabelYear = new ToolStripStatusLabel();
            menuStripMainForm = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem = new ToolStripMenuItem();
            runModelF5ToolStripMenuItem = new ToolStripMenuItem();
            runChokF6ToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            tweakToolStripMenuItem = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            formsPlot1 = new ScottPlot.FormsPlot();
            formsPlot2 = new ScottPlot.FormsPlot();
            formsPlot3 = new ScottPlot.FormsPlot();
            formsPlot4 = new ScottPlot.FormsPlot();
            formsPlot5 = new ScottPlot.FormsPlot();
            formsPlot6 = new ScottPlot.FormsPlot();
            formsPlot7 = new ScottPlot.FormsPlot();
            formsPlot8 = new ScottPlot.FormsPlot();
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            formsPlot9 = new ScottPlot.FormsPlot();
            formsPlot10 = new ScottPlot.FormsPlot();
            formsPlot11 = new ScottPlot.FormsPlot();
            formsPlot12 = new ScottPlot.FormsPlot();
            formsPlot13 = new ScottPlot.FormsPlot();
            formsPlot14 = new ScottPlot.FormsPlot();
            formsPlot15 = new ScottPlot.FormsPlot();
            formsPlot16 = new ScottPlot.FormsPlot();
            labelPeriods = new Label();
            toolTipLabelPeriods = new ToolTip(components);
            labelTimeUsePerYear = new Label();
            toolTipLabelTimeUsePerYear = new ToolTip(components);
            labelBuffer = new Label();
            labelMainText = new Label();
            pictureBoxDREAM = new PictureBox();
            backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            runScenariosF7ToolStripMenuItem = new ToolStripMenuItem();
            statusStripMainForm.SuspendLayout();
            menuStripMainForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxDREAM).BeginInit();
            SuspendLayout();
            // 
            // statusStripMainForm
            // 
            statusStripMainForm.BackColor = SystemColors.Control;
            statusStripMainForm.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMainForm, toolStripStatusLabelYear });
            statusStripMainForm.Location = new Point(0, 591);
            statusStripMainForm.Name = "statusStripMainForm";
            statusStripMainForm.Size = new Size(1099, 22);
            statusStripMainForm.TabIndex = 0;
            statusStripMainForm.Text = "statusStrip1";
            // 
            // toolStripStatusLabelMainForm
            // 
            toolStripStatusLabelMainForm.Name = "toolStripStatusLabelMainForm";
            toolStripStatusLabelMainForm.Size = new Size(0, 17);
            // 
            // toolStripStatusLabelYear
            // 
            toolStripStatusLabelYear.BorderStyle = Border3DStyle.Sunken;
            toolStripStatusLabelYear.Name = "toolStripStatusLabelYear";
            toolStripStatusLabelYear.Size = new Size(0, 17);
            // 
            // menuStripMainForm
            // 
            menuStripMainForm.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, runToolStripMenuItem, toolsToolStripMenuItem, windowToolStripMenuItem });
            menuStripMainForm.Location = new Point(0, 0);
            menuStripMainForm.Name = "menuStripMainForm";
            menuStripMainForm.Size = new Size(1099, 24);
            menuStripMainForm.TabIndex = 1;
            menuStripMainForm.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "&View";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(122, 22);
            settingsToolStripMenuItem.Text = "&Settings..";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // runToolStripMenuItem
            // 
            runToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { runModelF5ToolStripMenuItem, runChokF6ToolStripMenuItem, runScenariosF7ToolStripMenuItem });
            runToolStripMenuItem.Name = "runToolStripMenuItem";
            runToolStripMenuItem.Size = new Size(40, 20);
            runToolStripMenuItem.Text = "&Run";
            // 
            // runModelF5ToolStripMenuItem
            // 
            runModelF5ToolStripMenuItem.Name = "runModelF5ToolStripMenuItem";
            runModelF5ToolStripMenuItem.Size = new Size(180, 22);
            runModelF5ToolStripMenuItem.Text = "&Run model (F5)";
            runModelF5ToolStripMenuItem.Click += runModelF5ToolStripMenuItem_Click;
            // 
            // runChokF6ToolStripMenuItem
            // 
            runChokF6ToolStripMenuItem.Name = "runChokF6ToolStripMenuItem";
            runChokF6ToolStripMenuItem.Size = new Size(180, 22);
            runChokF6ToolStripMenuItem.Text = "Run &Shock (F6)";
            runChokF6ToolStripMenuItem.Click += runChokF6ToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tweakToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 20);
            toolsToolStripMenuItem.Text = "&Tools";
            // 
            // tweakToolStripMenuItem
            // 
            tweakToolStripMenuItem.Enabled = false;
            tweakToolStripMenuItem.Name = "tweakToolStripMenuItem";
            tweakToolStripMenuItem.Size = new Size(112, 22);
            tweakToolStripMenuItem.Text = "&Tweak..";
            tweakToolStripMenuItem.Click += tweakToolStripMenuItem_Click;
            // 
            // windowToolStripMenuItem
            // 
            windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            windowToolStripMenuItem.Size = new Size(63, 20);
            windowToolStripMenuItem.Text = "&Window";
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(48, 387);
            formsPlot1.Margin = new Padding(5, 4, 5, 4);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(272, 167);
            formsPlot1.TabIndex = 2;
            // 
            // formsPlot2
            // 
            formsPlot2.Location = new Point(86, 390);
            formsPlot2.Margin = new Padding(5, 4, 5, 4);
            formsPlot2.Name = "formsPlot2";
            formsPlot2.Size = new Size(164, 135);
            formsPlot2.TabIndex = 3;
            // 
            // formsPlot3
            // 
            formsPlot3.Location = new Point(26, 376);
            formsPlot3.Margin = new Padding(5, 4, 5, 4);
            formsPlot3.Name = "formsPlot3";
            formsPlot3.Size = new Size(264, 168);
            formsPlot3.TabIndex = 4;
            // 
            // formsPlot4
            // 
            formsPlot4.Location = new Point(111, 396);
            formsPlot4.Margin = new Padding(5, 4, 5, 4);
            formsPlot4.Name = "formsPlot4";
            formsPlot4.Size = new Size(173, 71);
            formsPlot4.TabIndex = 5;
            // 
            // formsPlot5
            // 
            formsPlot5.Location = new Point(101, 412);
            formsPlot5.Margin = new Padding(5, 4, 5, 4);
            formsPlot5.Name = "formsPlot5";
            formsPlot5.Size = new Size(205, 96);
            formsPlot5.TabIndex = 6;
            // 
            // formsPlot6
            // 
            formsPlot6.Location = new Point(27, 387);
            formsPlot6.Margin = new Padding(5, 4, 5, 4);
            formsPlot6.Name = "formsPlot6";
            formsPlot6.Size = new Size(242, 160);
            formsPlot6.TabIndex = 7;
            // 
            // formsPlot7
            // 
            formsPlot7.Location = new Point(56, 387);
            formsPlot7.Margin = new Padding(5, 4, 5, 4);
            formsPlot7.Name = "formsPlot7";
            formsPlot7.Size = new Size(213, 157);
            formsPlot7.TabIndex = 8;
            // 
            // formsPlot8
            // 
            formsPlot8.Location = new Point(86, 406);
            formsPlot8.Margin = new Padding(5, 4, 5, 4);
            formsPlot8.Name = "formsPlot8";
            formsPlot8.Size = new Size(188, 88);
            formsPlot8.TabIndex = 9;
            // 
            // backgroundWorker
            // 
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            // 
            // formsPlot9
            // 
            formsPlot9.Location = new Point(56, 390);
            formsPlot9.Margin = new Padding(5, 4, 5, 4);
            formsPlot9.Name = "formsPlot9";
            formsPlot9.Size = new Size(213, 138);
            formsPlot9.TabIndex = 10;
            // 
            // formsPlot10
            // 
            formsPlot10.Location = new Point(26, 412);
            formsPlot10.Margin = new Padding(5, 4, 5, 4);
            formsPlot10.Name = "formsPlot10";
            formsPlot10.Size = new Size(161, 96);
            formsPlot10.TabIndex = 11;
            // 
            // formsPlot11
            // 
            formsPlot11.Location = new Point(86, 435);
            formsPlot11.Margin = new Padding(5, 4, 5, 4);
            formsPlot11.Name = "formsPlot11";
            formsPlot11.Size = new Size(173, 93);
            formsPlot11.TabIndex = 12;
            // 
            // formsPlot12
            // 
            formsPlot12.Location = new Point(45, 406);
            formsPlot12.Margin = new Padding(5, 4, 5, 4);
            formsPlot12.Name = "formsPlot12";
            formsPlot12.Size = new Size(239, 102);
            formsPlot12.TabIndex = 13;
            // 
            // formsPlot13
            // 
            formsPlot13.Location = new Point(48, 396);
            formsPlot13.Margin = new Padding(5, 4, 5, 4);
            formsPlot13.Name = "formsPlot13";
            formsPlot13.Size = new Size(239, 132);
            formsPlot13.TabIndex = 14;
            // 
            // formsPlot14
            // 
            formsPlot14.Location = new Point(72, 390);
            formsPlot14.Margin = new Padding(5, 4, 5, 4);
            formsPlot14.Name = "formsPlot14";
            formsPlot14.Size = new Size(218, 130);
            formsPlot14.TabIndex = 15;
            // 
            // formsPlot15
            // 
            formsPlot15.Location = new Point(42, 390);
            formsPlot15.Margin = new Padding(5, 4, 5, 4);
            formsPlot15.Name = "formsPlot15";
            formsPlot15.Size = new Size(264, 138);
            formsPlot15.TabIndex = 16;
            // 
            // formsPlot16
            // 
            formsPlot16.Location = new Point(97, 407);
            formsPlot16.Margin = new Padding(5, 4, 5, 4);
            formsPlot16.Name = "formsPlot16";
            formsPlot16.Size = new Size(193, 121);
            formsPlot16.TabIndex = 17;
            // 
            // labelPeriods
            // 
            labelPeriods.AutoSize = true;
            labelPeriods.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelPeriods.Location = new Point(788, 526);
            labelPeriods.Name = "labelPeriods";
            labelPeriods.Size = new Size(162, 37);
            labelPeriods.TabIndex = 19;
            labelPeriods.Text = "labelPeriods";
            labelPeriods.MouseHover += labelPeriods_MouseHover;
            // 
            // labelTimeUsePerYear
            // 
            labelTimeUsePerYear.AutoSize = true;
            labelTimeUsePerYear.Location = new Point(799, 462);
            labelTimeUsePerYear.Name = "labelTimeUsePerYear";
            labelTimeUsePerYear.Size = new Size(155, 21);
            labelTimeUsePerYear.TabIndex = 20;
            labelTimeUsePerYear.Text = "labelTimeUsePerYear";
            labelTimeUsePerYear.MouseHover += labelTimeUsePerYear_MouseHover;
            // 
            // labelBuffer
            // 
            labelBuffer.AutoSize = true;
            labelBuffer.BackColor = Color.Red;
            labelBuffer.ForeColor = SystemColors.ButtonHighlight;
            labelBuffer.Location = new Point(807, 423);
            labelBuffer.Name = "labelBuffer";
            labelBuffer.Size = new Size(292, 21);
            labelBuffer.TabIndex = 21;
            labelBuffer.Text = "Buffer problem! Set up charting interval. ";
            // 
            // labelMainText
            // 
            labelMainText.AutoSize = true;
            labelMainText.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelMainText.Location = new Point(793, 376);
            labelMainText.Name = "labelMainText";
            labelMainText.Size = new Size(181, 37);
            labelMainText.TabIndex = 22;
            labelMainText.Text = "labelMainText";
            // 
            // pictureBoxDREAM
            // 
            pictureBoxDREAM.Image = (Image)resources.GetObject("pictureBoxDREAM.Image");
            pictureBoxDREAM.Location = new Point(26, 43);
            pictureBoxDREAM.Name = "pictureBoxDREAM";
            pictureBoxDREAM.Size = new Size(130, 130);
            pictureBoxDREAM.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxDREAM.TabIndex = 23;
            pictureBoxDREAM.TabStop = false;
            // 
            // backgroundWorker2
            // 
            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.DoWork += backgroundWorker2_DoWork;
            backgroundWorker2.ProgressChanged += backgroundWorker2_ProgressChanged;
            backgroundWorker2.RunWorkerCompleted += backgroundWorker2_RunWorkerCompleted;
            // 
            // runScenariosF7ToolStripMenuItem
            // 
            runScenariosF7ToolStripMenuItem.Name = "runScenariosF7ToolStripMenuItem";
            runScenariosF7ToolStripMenuItem.Size = new Size(180, 22);
            runScenariosF7ToolStripMenuItem.Text = "Run Sc&enarios (F7)";
            // 
            // MainFormUI
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.White;
            ClientSize = new Size(1029, 630);
            Controls.Add(pictureBoxDREAM);
            Controls.Add(labelMainText);
            Controls.Add(labelBuffer);
            Controls.Add(labelTimeUsePerYear);
            Controls.Add(labelPeriods);
            Controls.Add(formsPlot16);
            Controls.Add(formsPlot15);
            Controls.Add(formsPlot14);
            Controls.Add(formsPlot13);
            Controls.Add(formsPlot12);
            Controls.Add(formsPlot11);
            Controls.Add(formsPlot10);
            Controls.Add(formsPlot9);
            Controls.Add(formsPlot8);
            Controls.Add(formsPlot7);
            Controls.Add(formsPlot6);
            Controls.Add(formsPlot5);
            Controls.Add(formsPlot4);
            Controls.Add(formsPlot3);
            Controls.Add(formsPlot2);
            Controls.Add(formsPlot1);
            Controls.Add(statusStripMainForm);
            Controls.Add(menuStripMainForm);
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            MainMenuStrip = menuStripMainForm;
            Margin = new Padding(4);
            Name = "MainFormUI";
            Text = "Self-Organized Equilibrium. An Agent-Based Model, 2023 - v0.1";
            Load += MainFormUI_Load;
            KeyUp += MainFormUI_KeyUp;
            statusStripMainForm.ResumeLayout(false);
            statusStripMainForm.PerformLayout();
            menuStripMainForm.ResumeLayout(false);
            menuStripMainForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxDREAM).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStripMainForm;
        private MenuStrip menuStripMainForm;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabelMainForm;
        private ToolStripMenuItem runToolStripMenuItem;
        private ToolStripMenuItem runModelF5ToolStripMenuItem;
        private ScottPlot.FormsPlot formsPlot1;
        private ScottPlot.FormsPlot formsPlot2;
        private ScottPlot.FormsPlot formsPlot3;
        private ScottPlot.FormsPlot formsPlot4;
        private ScottPlot.FormsPlot formsPlot5;
        private ScottPlot.FormsPlot formsPlot6;
        private ScottPlot.FormsPlot formsPlot7;
        private ScottPlot.FormsPlot formsPlot8;

        public System.ComponentModel.BackgroundWorker backgroundWorker;
        private ToolStripStatusLabel toolStripStatusLabelYear;
        private ScottPlot.FormsPlot formsPlot9;
        private ScottPlot.FormsPlot formsPlot10;
        private ScottPlot.FormsPlot formsPlot11;
        private ScottPlot.FormsPlot formsPlot12;
        private ScottPlot.FormsPlot formsPlot13;
        private ScottPlot.FormsPlot formsPlot14;
        private ScottPlot.FormsPlot formsPlot15;
        private ScottPlot.FormsPlot formsPlot16;
        private Label labelPeriods;
        private ToolTip toolTipLabelPeriods;
        private Label labelTimeUsePerYear;
        private ToolTip toolTipLabelTimeUsePerYear;
        private Label labelBuffer;
        private Label labelMainText;
        private PictureBox pictureBoxDREAM;
        private ToolStripMenuItem tweakToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem runChokF6ToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private ToolStripMenuItem runScenariosF7ToolStripMenuItem;
    }
}