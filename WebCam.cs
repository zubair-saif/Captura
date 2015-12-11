using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;

namespace ScreenWorks
{
    public class WebCam
    {
        WebcamDirectShow Device;

        public string Name { get { return Device.Name; } }

        public Bitmap TakePicture() { return Device.TakePicture(); }

        WebCam(WebcamDirectShow dev) { Device = dev; }

        public static IEnumerable<WebCam> Enumerate()
        {
            foreach (var dev in WebcamDirectShow.Enumerate())
                yield return new WebCam(dev);
        }
    }

    class WebcamDirectShow : ISampleGrabberCB
    {
        public DsDevice Device { get; private set; }

        public string Name { get { return Device.Name; } }

        int m_callbackState;
        ManualResetEvent m_callbackCompleted;
        WebcamConfiguration m_configuration = WebcamConfiguration.Empty;
        Bitmap m_capturedBitmap;

        public static IEnumerable<WebcamDirectShow> Enumerate()
        {
            foreach (var dev in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
                yield return new WebcamDirectShow(dev);
        }

        WebcamDirectShow(DsDevice Device) { this.Device = Device; }

        public Bitmap TakePicture()
        {
            if (m_callbackCompleted != null)
                return null;

            IFilterGraph2 filterGraph = null;
            IBaseFilter cam = null; // cam
            IPin camCapture = null;
            ISampleGrabber sg = null; // samplegrabber
            IPin sgIn = null;

            try
            {
                // setup filterGraph & connect camera
                filterGraph = (IFilterGraph2)new FilterGraph();
                DsError.ThrowExceptionForHR(filterGraph.AddSourceFilterForMoniker(Device.Mon, null, Device.Name, out cam));

                // setup smarttee and connect so that cam(PinCategory.Capture)->st(PinDirection.Input)
                camCapture = DsFindPin.ByCategory(cam, PinCategory.Capture, 0);	// output
                ConfStreamDimensions((IAMStreamConfig)camCapture);

                // connect Camera output to SampleGrabber input
                sg = (ISampleGrabber)new SampleGrabber();

                // configure
                AMMediaType media = new AMMediaType();
                try
                {
                    media.majorType = MediaType.Video;
                    media.subType = BPP2MediaSubtype(m_configuration.BPP);	// this will ask samplegrabber to do convertions for us
                    media.formatType = FormatType.VideoInfo;
                    DsError.ThrowExceptionForHR(sg.SetMediaType(media));
                }
                finally
                {
                    DsUtils.FreeAMMediaType(media);
                    media = null;
                }

                DsError.ThrowExceptionForHR(sg.SetCallback(this, 1)); // 1 = BufferCB
                DsError.ThrowExceptionForHR(filterGraph.AddFilter((IBaseFilter)sg, "SG"));
                sgIn = DsFindPin.ByDirection((IBaseFilter)sg, PinDirection.Input, 0); // input
                DsError.ThrowExceptionForHR(filterGraph.Connect(camCapture, sgIn));
                GetSizeInfo(sg);

                // wait until timeout - or picture has been taken 
                if (m_callbackCompleted == null)
                {
                    m_callbackCompleted = new ManualResetEvent(false);

                    // start filter
                    DsError.ThrowExceptionForHR(((IMediaControl)filterGraph).Run());
                    m_callbackState = 5;
                    if (!m_callbackCompleted.WaitOne(15000, false))
                        throw new Exception("Timeout while waiting for Picture");

                    return m_capturedBitmap;
                }
                else return null;
            }
            finally
            {
                // release allocated objects
                if (m_callbackCompleted != null)
                {
                    m_callbackCompleted.Close();
                    m_callbackCompleted = null;
                }
                if (sgIn != null)
                {
                    Marshal.ReleaseComObject(sgIn);
                    sgIn = null;
                }
                if (sg != null)
                {
                    Marshal.ReleaseComObject(sg);
                    sg = null;
                }
                if (camCapture != null)
                {
                    Marshal.ReleaseComObject(camCapture);
                    camCapture = null;
                }
                if (cam != null)
                {
                    Marshal.ReleaseComObject(cam);
                    cam = null;
                }
                if (filterGraph != null)
                {
                    try { ((IMediaControl)filterGraph).Stop(); }
                    catch (Exception) { }
                    Marshal.ReleaseComObject(filterGraph);
                    filterGraph = null;
                }
                m_capturedBitmap = null;
                m_callbackCompleted = null;
            }
        }

        #region TakePicture helpers
        void GetSizeInfo(ISampleGrabber sampleGrabber)
        {
            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();
            try
            {
                DsError.ThrowExceptionForHR(sampleGrabber.GetConnectedMediaType(media));
                if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
                    throw new NotSupportedException(); //Unknown Grabber Media Format

                VideoInfoHeader v = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
                m_configuration.Size = new Size(v.BmiHeader.Width, v.BmiHeader.Height);
                m_configuration.BPP = v.BmiHeader.BitCount;
                //m_configuration.MediaSubtype = media.subType;
            }
            finally
            {
                DsUtils.FreeAMMediaType(media);
                media = null;
            }
        }

        void ConfStreamDimensions(IAMStreamConfig streamConfig)
        {
            AMMediaType media = null;
            DsError.ThrowExceptionForHR(streamConfig.GetFormat(out media));

            try
            {
                VideoInfoHeader v = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));

                if (m_configuration.Size.Width > 0)
                    v.BmiHeader.Width = m_configuration.Size.Width;
                if (m_configuration.Size.Height > 0)
                    v.BmiHeader.Height = m_configuration.Size.Height;
                if (m_configuration.BPP > 0)
                    v.BmiHeader.BitCount = m_configuration.BPP;
                if (m_configuration.MediaSubtype != Guid.Empty)
                    media.subType = m_configuration.MediaSubtype;

                //v.AvgTimePerFrame = 10000000 / 30; // 30 fps. FPS might be controlled by the camera, because of lightning exposure may increase and FPS decrease. 

                Marshal.StructureToPtr(v, media.formatPtr, false);
                DsError.ThrowExceptionForHR(streamConfig.SetFormat(media));
            }
            finally
            {
                DsUtils.FreeAMMediaType(media);
                media = null;
            }
        }
        #endregion

        #region ISampleGrabberCB Callbacks
        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if (m_callbackState > 0)
            {
                m_callbackState--;

                if (m_callbackState <= 0)
                {
                    using (Bitmap b = new Bitmap(m_configuration.Size.Width, m_configuration.Size.Height, BufferLen / m_configuration.Size.Height, m_configuration.PixelFormat, pBuffer))
                    {
                        m_capturedBitmap = new Bitmap(b);	// copy
                        m_capturedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    }

                    m_callbackCompleted.Set();
                }
            }

            return 0;
        }
        #endregion

        static Guid BPP2MediaSubtype(short bpp)
        {
            switch (bpp)
            {
                case 16:
                    return MediaSubType.RGB565;
                case 24:
                    return MediaSubType.RGB24;
                case 32:
                    return MediaSubType.RGB32;
            }
            return MediaSubType.RGB24;
        }
    }

    struct WebcamConfiguration
    {
        public static readonly WebcamConfiguration Empty = new WebcamConfiguration(Size.Empty, 0, Guid.Empty);

        public WebcamConfiguration(Size size, short bpp, Guid mediaSubtype)
        {
            this = new WebcamConfiguration()
            {
                Size = size,
                BPP = bpp,
                MediaSubtype = mediaSubtype
            };
        }

        public PixelFormat PixelFormat
        {
            get
            {
                switch (BPP)
                {
                    case 32:
                        return PixelFormat.Format32bppRgb;
                    case 16:
                        return PixelFormat.Format16bppRgb565;
                    default:
                        return PixelFormat.Format24bppRgb;
                }
            }
        }

        public Size Size { get; set; }

        public short BPP { get; set; }

        public Guid MediaSubtype { get; set; }

        public int BufferSize { get { return Size.Width * Size.Height * BPP; } }
    }
}
