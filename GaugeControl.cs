using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace _737OverflowValve
{
    public class GaugeControl : Control
    {
        public double GaugeValue { get; set; } = 0.0; // 0–100
        public string ConfigIP { get; set; }
        public int ConfigLineSize { get; set; } = 4;
        public int ConfigArchSize { get; set; } = 2;
        public string ConfigArchHexColor { get; set; }

        private ContextMenuStrip contextMenu;
        private double _targetValue = 0.0;
        private Timer _smoothingTimer;
        private const int TimerInterval = 2; // ms
        private const double StepSize = 0.1;  // adjust for speed/smoothness

        public GaugeControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.Black;

            _smoothingTimer = new Timer();
            _smoothingTimer.Interval = TimerInterval;
            _smoothingTimer.Tick += SmoothingTimer_Tick;

            contextMenu = new ContextMenuStrip();
            contextMenu.Opening += ContextMenu_Opening;
            this.ContextMenuStrip = contextMenu;
        }

        public void SetGaugeValueSmooth(double value)
        {
            _targetValue = value;
            Console.WriteLine("smooth to " + _targetValue);
            _smoothingTimer.Start();
            
        }

        private void SmoothingTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("SmoothingTimer_Tick");

            double delta = _targetValue - GaugeValue;
            if (Math.Abs(delta) < 0.01)
            {
                GaugeValue = _targetValue;
                _smoothingTimer.Stop();
            }
            else
            {
                GaugeValue += Math.Sign(delta) * Math.Min(Math.Abs(delta), StepSize);
            }
            Console.WriteLine(GaugeValue);
            Console.WriteLine("inval");


            Invalidate(); // Trigger redraw
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            contextMenu.Items.Clear(); // Clear any previous items
            var ipItem = new ToolStripMenuItem($"IP: {ConfigIP}");
            ipItem.Enabled = false; // Make it read-only

            var vItem = new ToolStripMenuItem($"Version: 1.0.0");
            vItem.Enabled = false;

            contextMenu.Items.Add(vItem);
            contextMenu.Items.Add(ipItem);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            int size = Math.Min(w, h);
            int radius = (int)(size * 0.7);
            Point center = new Point(w / 2, (int)(h / 1.1));

            // Compute needle angle
            double angleDeg = 160 - (GaugeValue * 140 / 100);
            double angleRad = angleDeg * Math.PI / 180;

            double innerRadius = radius * 0.5;
            double outerRadius = radius;

            Point needleStart = new Point(
                center.X + (int)(innerRadius * Math.Cos(angleRad)),
                center.Y - (int)(innerRadius * Math.Sin(angleRad)));

            Point needleEnd = new Point(
                center.X + (int)(outerRadius * Math.Cos(angleRad)),
                center.Y - (int)(outerRadius * Math.Sin(angleRad)));

            // Needle
            using (Pen needlePen = new Pen(Color.White, ConfigLineSize))
            {
                g.DrawLine(needlePen, needleStart, needleEnd);
            }

            // Draw Arch
            Rectangle arcRect = new Rectangle(
                center.X - radius,
                center.Y - radius,
                radius * 2,
                radius * 2);

            using (Pen arcPen = new Pen(
                    ColorTranslator.FromHtml(ConfigArchHexColor)
                    , ConfigArchSize))
            {
                g.DrawArc(arcPen, arcRect, 180, 180);
            }
        }
    }
}
