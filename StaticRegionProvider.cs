using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using Point = System.Drawing.Point;
using ScreenWorks;

namespace Captura
{
    class StaticRegionProvider : IImageProvider
    {
        RegionSelector RegSel;
        Func<bool> IncludeCursor;
        MouseKeyHook MouseKeyHookFacade = null;

        public StaticRegionProvider(RegionSelector RegSel, Func<bool> IncludeCursor, bool CaptureMouseClicks, bool CaptureKeyStrokes)
        {
            this.RegSel = RegSel;

            Height = (int)RegSel.Height;
            Width = (int)RegSel.Width;

            RegSel.ResizeMode = ResizeMode.NoResize;

            this.IncludeCursor = IncludeCursor;

            if (CaptureMouseClicks || CaptureKeyStrokes)
                MouseKeyHookFacade = new MouseKeyHook(CaptureMouseClicks, CaptureKeyStrokes);
        }

        Rectangle Rectangle
        {
            get
            {
                var Location = (Point)RegSel.Dispatcher.Invoke(new Func<Point>(() => new Point((int)RegSel.Left, (int)RegSel.Top)));
                return new Rectangle(Location.X, Location.Y, Width, Height);
            }
        }

        public Bitmap Capture()
        {                        
            var BMP = ScreenShot.Capture(Rectangle, IncludeCursor());
            if (MouseKeyHookFacade != null) MouseKeyHookFacade.Draw(BMP, Rectangle.Location);
            return BMP;
        }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public int BufferSize { get { return Height * Width * 4; } }

        public PixelFormat PixelFormat { get { return PixelFormat.Format32bppRgb; } }

        public void Dispose()
        {
            RegSel.ResizeMode = ResizeMode.CanResize;
            if (MouseKeyHookFacade != null) MouseKeyHookFacade.Dispose();
        }
    }
}
