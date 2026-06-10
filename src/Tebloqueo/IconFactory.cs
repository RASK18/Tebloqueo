using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Tebloqueo;

internal static class IconFactory
{
    public static Icon Create(string text, Color background)
    {
        using var bitmap = new Bitmap(64, 64);
        using var graphics = Graphics.FromImage(bitmap);
        using var backgroundBrush = new SolidBrush(background);
        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI", text == "?" ? 39 : 25, FontStyle.Bold, GraphicsUnit.Pixel);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        graphics.Clear(Color.Transparent);
        graphics.FillRoundedRectangle(backgroundBrush, new Rectangle(2, 2, 60, 60), 13);

        var size = graphics.MeasureString(text, font);
        graphics.DrawString(text, font, textBrush, (64 - size.Width) / 2, (64 - size.Height) / 2 - 1);

        var handle = bitmap.GetHicon();
        try
        {
            using var temporaryIcon = Icon.FromHandle(handle);
            return (Icon)temporaryIcon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle rectangle, int radius)
    {
        using var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(rectangle.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = rectangle.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rectangle.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rectangle.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);
}
