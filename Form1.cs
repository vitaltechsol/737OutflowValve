using ProSimSDK;
using System;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace _737OverflowValve
{
    public partial class MainForm : Form
    {
        private const string SettingsFile = "settings.config";
        private const string XmlFile = "config.xml";
        private GaugeControl gaugeControl;
        private readonly ProSimConnect _connection = new ProSimConnect();
        public MainForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.BackColor = System.Drawing.Color.Black;
            this.UpdateStyles(); // Apply the changes
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            // Register to receive connect and disconnect events
            _connection.onConnect += Connection_onConnect;
            _connection.onDisconnect += Connection_onDisconnect;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gaugeControl = new GaugeControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(gaugeControl);
            this.Shown += new EventHandler(Form1_Shown);
        }

        private void Form1_Shown(Object sender, EventArgs e)
        {
            Invoke(new MethodInvoker(LoadSettings));
            Invoke(new MethodInvoker(LoadConfigFromXML));

            Console.WriteLine("IP from XML: " + gaugeControl.ConfigIP);
            try
            {
                _connection.Connect(gaugeControl.ConfigIP);
            }
            catch (Exception ex)
            {
                Invoke(new MethodInvoker(NotConnected));
                //  MessageBox.Show("Error connecting to ProSim System: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                string[] lines = File.ReadAllLines(SettingsFile);
                if (lines.Length == 4)
                {
                    int x = int.Parse(lines[0]);
                    int y = int.Parse(lines[1]);
                    int w = int.Parse(lines[2]);
                    int h = int.Parse(lines[3]);
                    this.SetBounds(x, y, w, h);
                }
            }
        }

        private void SaveSettings()
        {
            File.WriteAllLines(SettingsFile, new[]
            {
                this.Left.ToString(),
                this.Top.ToString(),
                this.Width.ToString(),
                this.Height.ToString()
            });
        }

        private void LoadConfigFromXML()
        {
            if (!File.Exists(XmlFile)) return;

            XDocument doc = XDocument.Load(XmlFile);
            gaugeControl.ConfigIP = doc.Root?.Element("IP")?.Value;
            gaugeControl.ConfigLineSize = Convert.ToInt32(
                doc.Root?.Element("LineSize")?.Value);
            gaugeControl.ConfigArchSize = Convert.ToInt32(
                 doc.Root?.Element("ArchSize")?.Value);
            gaugeControl.ConfigArchHexColor = doc.Root?
                .Element("ArchHexColor")?.Value;
        }

        private void Connection_onConnect()
        {
            Invoke(new MethodInvoker(Connected));
            Invoke(new MethodInvoker(GetData));
        }

        private void GetData()
        {
            var dataRefOutflow = new DataRef("system.gauge.G_OH_OUTFLOW_VALVE", 5, _connection);
            dataRefOutflow.onDataChange += DataRef_outflow_onDataChange;
            Console.WriteLine("connected to prosim");
        }

        /// <summary>
        ///     When we disconnect from ProSim System, update the status label
        /// </summary>
        private void Connection_onDisconnect()
        {
            Console.WriteLine("disconnected");
            Invoke(new MethodInvoker(NotConnected));
        }

        private void Connected()
        {
            lblStatus.Text = String.Empty;
            lblStatus.Visible = false;
        }

        private void NotConnected()
        {
            lblStatus.Visible = true;
            lblStatus.Text = "INOP";
        }

        private void DataRef_outflow_onDataChange(DataRef dataRef)
        {
            var value = Math.Round(Convert.ToDouble(dataRef.value), 3);
            Console.WriteLine("ref " + value);
            //gaugeControl.SetGaugeValueSmooth(value);
            gaugeControl.Invoke(new Action(() =>
            {
                gaugeControl.SetGaugeValueSmooth(value);
            }));
        }

        //private void DataRef_outflow_onDataChange(DataRef dataRef)
        //{
        //    var value = Math.Round(Convert.ToDouble(dataRef.value), 3);

        //    //Console.WriteLine("ref " + value);
        //    gaugeControl.GaugeValue = value;
        //    gaugeControl.Invalidate();
        //}
    }
}
