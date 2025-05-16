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
        private ContextMenuStrip contextMenu;
        private readonly ProSimConnect _connection = new ProSimConnect();


        public MainForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);

            this.BackColor = System.Drawing.Color.Black;
            this.UpdateStyles(); // Apply the changes
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            InitializeContextMenu();
            this.MouseUp += MainForm_MouseUp;

            // Register to receive connect and disconnect events
            _connection.onConnect += Connection_onConnect;
            _connection.onDisconnect += Connection_onDisconnect;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            string ip = LoadIpFromXml();
            Console.WriteLine("IP from XML: " + ip);

            try
            {
                _connection.Connect(ip);
            }
            catch (Exception ex)
            {
                Invoke(new MethodInvoker(NotConnected));
                //  MessageBox.Show("Error connecting to ProSim System: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



            gaugeControl = new GaugeControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(gaugeControl);
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

        private string LoadIpFromXml()
        {
            if (!File.Exists(XmlFile)) return "127.0.0.1";
            XDocument doc = XDocument.Load(XmlFile);
            return doc.Root?.Element("IP")?.Value ?? "127.0.0.1";
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
        }

        private void NotConnected()
        {
            lblStatus.Text = "INOP";
        }

        private void DataRef_outflow_onDataChange(DataRef dataRef)
        {
            var value = Math.Round(Convert.ToDouble(dataRef.value), 3);

            //Console.WriteLine("ref " + value);
            gaugeControl.GaugeValue = value;
            gaugeControl.Invalidate();
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            var ipItem = new ToolStripMenuItem("Show IP Address");
            ipItem.Click += ShowIPAddress_Click;
            contextMenu.Items.Add(ipItem);
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("right click");
                contextMenu.Show(this, e.Location);
            }
        }

        private void ShowIPAddress_Click(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(XmlFile);
                string ip = doc.SelectSingleNode("/Configuration/IPAddress")?.InnerText ?? "Not Found";
                MessageBox.Show($"IP Address: {ip}", "IP Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load IP address:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
