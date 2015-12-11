using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenWorks
{
    public class WebCamProvider : IImageProvider
    {
        WebCam WCam;

        public WebCamProvider(WebCam WCam)
        {
            this.WCam = WCam;

            var BMP = Capture();

            Height = BMP.Height;
            Width = BMP.Width;
            PixelFormat = BMP.PixelFormat;

            int BytesPerPixel;

            switch (PixelFormat)
            {
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    BytesPerPixel = 4;
                    break;
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                    BytesPerPixel = 2;
                    break;
                default:
                    BytesPerPixel = 3;
                    break;
            }

            BufferSize = Height * Width * BytesPerPixel;

            Rectangle = new Rectangle(Point.Empty, BMP.Size);
        }

        public Bitmap Capture() { return WCam.TakePicture(); }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public Rectangle Rectangle { get; private set; }

        public int BufferSize { get; private set; }

        public void Dispose() { }

        public PixelFormat PixelFormat { get; private set; }
    }
}