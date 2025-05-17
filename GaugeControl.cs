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

        public GaugeControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.Black;

            contextMenu = new ContextMenuStrip();
            contextMenu.Opening += ContextMenu_Opening;
            this.ContextMenuStrip = contextMenu;
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            contextMenu.Items.Clear(); // Clear any previous items
            var ipItem = new ToolStripMenuItem($"IP: {ConfigIP}");
            ipItem.Enabled = false; // Make it read-only
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
            int radius = (int)(size * 0.4);
            Point center = new Point(w / 2, (int)(h / 1.5));

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
