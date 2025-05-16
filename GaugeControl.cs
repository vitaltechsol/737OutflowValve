using System;
using System.Drawing;
using System.Windows.Forms;

namespace _737OverflowValve
{
    public class GaugeControl : Control
    {
        public double GaugeValue { get; set; } = 0.0; // 0–100

        public GaugeControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
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

            // Draw semicircle
            Rectangle arcRect = new Rectangle(
                center.X - radius,
                center.Y - radius,
                radius * 2,
                radius * 2);

            using (Pen arcPen = new Pen(Color.White, 20))
            {
                g.DrawArc(arcPen, arcRect, 180, 180);
            }

            // Compute needle angle (same logic as before)
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

            using (Pen needlePen = new Pen(Color.White, 8))
            {
                g.DrawLine(needlePen, needleStart, needleEnd);
            }
        }
    }
}
