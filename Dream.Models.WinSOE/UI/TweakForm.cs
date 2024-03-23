using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

#if WIN_APP
namespace Dream.Models.WinSOE
{
    public partial class TweakForm : Form
    {
        Simulation _simulation;
        Settings _settings;
        Time _time;

        TrackBar[] _trackBars = new TrackBar[4];
        ComboBox[] _comboBoxes = new ComboBox[4];
        Label[] _labelsValue = new Label[4];
        Label[] _labelsMin = new Label[4];
        Label[] _labelsMax = new Label[4];

        List<PropertyInfo>? _props = null;

        List<string> _name = new List<string>();
        List<object> _max = new List<object>();
        List<object> _min = new List<object>();

        public TweakForm(Simulation simulation) : base()
        {
            InitializeComponent();

            _simulation = simulation;
            _settings = _simulation.Settings;
            _time = _simulation.Time;

            this.KeyPreview = true;

            loadDefaultSettings();

            _trackBars[0] = trackBarTweak1;
            _trackBars[1] = trackBarTweak2;
            _trackBars[2] = trackBarTweak3;
            _trackBars[3] = trackBarTweak4;

            foreach (TrackBar trackBar in _trackBars)
            {
                trackBar.Minimum = 0;
                trackBar.Maximum = 100;
            }

            _comboBoxes[0] = comboBox1;
            _comboBoxes[1] = comboBox2;
            _comboBoxes[2] = comboBox3;
            _comboBoxes[3] = comboBox4;

            foreach (var comboBox in _comboBoxes)
                comboBox.Items.AddRange(_name.ToArray());

            _labelsValue[0] = labelValue1;
            _labelsValue[1] = labelValue2;
            _labelsValue[2] = labelValue3;
            _labelsValue[3] = labelValue4;

            _labelsMin[0] = labelMin1;
            _labelsMin[1] = labelMin2;
            _labelsMin[2] = labelMin3;
            _labelsMin[3] = labelMin4;

            _labelsMax[0] = labelMax1;
            _labelsMax[1] = labelMax2;
            _labelsMax[2] = labelMax3;
            _labelsMax[3] = labelMax4;

        }

        void loadDefaultSettings()
        {
            var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

            // Collecting tweakable properties from Settings
            _props = typeof(Settings).GetProperties(bindingFlags).
                                         Where(prop => Attribute.IsDefined(prop, typeof(TweakableAttribute))).ToList();

            foreach (var prop in _props)
            {
                _name.Add(prop.Name);

                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    TweakableAttribute tweakAttr = attr as TweakableAttribute;
                    if (tweakAttr != null)
                    {
                        _min.Add(tweakAttr.Minimum);
                        _max.Add(tweakAttr.Maximum);
                    }
                }
            }
        }

        private void buttonTweakClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void TweakForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
        }

        private void trackBarTweak_Scroll(object sender, EventArgs e)
        {
            // Not used

        }

        void comboBox_SelectedIndexChanged(object sender, int j)
        {
            ComboBox comboBox = (ComboBox)sender;

            int i_selection = comboBox.SelectedIndex;
            if (i_selection == -1)
                return;

            _labelsMin[j].Text = _min[i_selection].ToString();
            _labelsMax[j].Text = _max[i_selection].ToString();

            double x = Convert.ToDouble(_props[i_selection].GetValue(_settings));
            _labelsValue[j].Text = x.ToString();

            double min = Convert.ToDouble(_min[i_selection]);
            double max = Convert.ToDouble(_max[i_selection]);
            _trackBars[j].Value = (int)(100 * (x - min) / (max - min));

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            comboBox_SelectedIndexChanged(sender, 0);

        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox_SelectedIndexChanged(sender, 1);

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox_SelectedIndexChanged(sender, 2);

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox_SelectedIndexChanged(sender, 3);

        }


        void trackBarTweak_Scroll(object sender, int j)
        {
            TrackBar trackBar = (TrackBar)sender;

            int i_selection = _comboBoxes[j].SelectedIndex;
            if (i_selection == -1)
                return;

            double min = Convert.ToDouble(_min[i_selection]);
            double max = Convert.ToDouble(_max[i_selection]);
            double x = min + 1.0 * trackBar.Value * (max - min) / 100;
            _labelsValue[j].Text = x.ToString();

            if (_props[i_selection].PropertyType == typeof(double))
                _props[i_selection].SetValue(_settings, x);

            if (_props[i_selection].PropertyType == typeof(int))
                _props[i_selection].SetValue(_settings, (int)Math.Round(x));

        }

        private void trackBarTweak1_Scroll(object sender, EventArgs e)
        {

            trackBarTweak_Scroll(sender, 0);

        }

        private void trackBarTweak2_Scroll(object sender, EventArgs e)
        {
            trackBarTweak_Scroll(sender, 1);

        }

        private void trackBarTweak3_Scroll(object sender, EventArgs e)
        {
            trackBarTweak_Scroll(sender, 2);

        }

        private void trackBarTweak4_Scroll(object sender, EventArgs e)
        {
            trackBarTweak_Scroll(sender, 3);

        }

        private void TweakForm_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            //this.BackColor = Color.LimeGreen;
            //this.TransparencyKey = Color.LimeGreen;
        }

        private void TweakForm_KeyUp(object sender, KeyEventArgs e)
        {
            Keys[] arrowKeys = { Keys.Up, Keys.Down, Keys.Left, Keys.Right };
            
            if(!arrowKeys.Contains(e.KeyCode))
                MainFormUI.MainFormUIInstance.MainFormUI_KeyUp(sender, e);
        }

        public Simulation Simulation
        {
            get { return _simulation; }
            set { _simulation = value; }
        }

    }
}
#endif
