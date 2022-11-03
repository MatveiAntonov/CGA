using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphicsLibrary.Models;
using GraphicsLibrary.ObjParser;
using GraphicsLibrary.Space;
using GraphicsLibrary.Camera;

namespace PresentationApp
{
    public partial class MainWindow : Window
    {
        #region Constants

        private const int PixelHeight = Config.Height;
        private const int PixelWidth = Config.Width;
        private const int DpiHeight = Config.DpiHeight;
        private const int DpiWidth = Config.DpiWidth;
        private const int RgbBytesPerPixel = Config.RgbBytesPerPixel;
        private const float ScaleSpeed = Config.ScaleSpeed;
        private const float TranslationSpeed = Config.TranslationSpeed;
        private const float AngleSpeed = Config.AngleSpeed;
        private const int LineColor = 0;

        #endregion
        #region External
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);
        #endregion
        private readonly Microsoft.Win32.OpenFileDialog _fileDialog = new();
        private readonly WriteableBitmap _bitmap =
            new(PixelWidth, PixelHeight, DpiWidth, DpiHeight, PixelFormats.Bgr32, null);

        private readonly byte[] _backImage = new byte[PixelHeight * PixelWidth * RgbBytesPerPixel];
        private readonly byte[] _image = new byte[PixelHeight * PixelWidth * RgbBytesPerPixel];
        private ObjModel _model;

        CameraModel cam;


        public MainWindow()
        {
            InitializeComponent();

            for (var i = 0; i < _backImage.Length; i++)
            {
                _backImage[i] = 128;
                _image[i] = 128;
            }

            //90T 0F

            InitializeBitmap();
            Image.Source = _bitmap;
            cam = new();
        }

        private void InitializeBitmap()
        {
            unsafe
            {
                fixed (byte* b = _backImage)
                {
                    CopyMemory(_bitmap.BackBuffer, (IntPtr)b, (uint)_backImage.Length);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _bitmap.Lock();
                        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
                        _bitmap.Unlock();
                    });
                }
            }
        }
        
        private void SelectFile(object sender, RoutedEventArgs e)
        {
            if (_fileDialog != null && (bool) _fileDialog.ShowDialog())
            {
                var fileName = _fileDialog.FileName;
                TxtBoxPathToFile.Text = fileName;
            }
        }

        private void Render(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(TxtBoxPathToFile.Text))
            {
                MessageBox.Show("File doesn't exists.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            var objReader = new ParserObj(TxtBoxPathToFile.Text);
            _model = objReader.GetObjModel();
      

            _model.Height = PixelHeight;
            _model.Width = PixelWidth;

            _model.ProjectionSpace = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), 1, 0.1f, 1);

            // пространство наблюдателя (пространство камеры)
            _model.ViewSpace = Matrix4x4.CreateLookAt(cam.camPos, cam.camTarget, cam.camUp);


            // пространство окна просмотра
            _model.ViewportSpace = new ViewportSpace(PixelWidth, PixelHeight, 0, 0);

            ShowModel();
        }


        private void ShowModel()
        {
            _model.TransformVertices();
            var points = _model.FindPoints();
            var ptr = _bitmap.BackBuffer;
            unsafe
            {
                fixed (byte* clear = _backImage)
                {
                    CopyMemory(ptr, (IntPtr)clear, (uint)_backImage.Length);
                }

                Parallel.ForEach(points, point =>
                {
                    var column = (int)point.X;
                    var row = (int)point.Y;

                    var localPtr = ptr;
                    localPtr += row * PixelWidth * RgbBytesPerPixel;
                    localPtr += column * RgbBytesPerPixel;
                    *((int*)localPtr) = LineColor;
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _bitmap.Lock();
                    _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
                    _bitmap.Unlock();
                });
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                _model.TranslationY += TranslationSpeed;
            }

            if (e.Key == Key.Down)
            {
                _model.TranslationY -= TranslationSpeed;
            }

            if (e.Key == Key.Left)
            {
                _model.TranslationX -= TranslationSpeed;
            }

            if (e.Key == Key.Right)
            {
                _model.TranslationX += TranslationSpeed;
            }

            if (e.Key == Key.W)
            {
                _model.AngleX -= AngleSpeed;
            }
            
            if (e.Key == Key.S)
            {
                _model.AngleX += AngleSpeed;
            }
            
            if (e.Key == Key.A)
            {
                _model.AngleY -= AngleSpeed;
            }
            
            if (e.Key == Key.D)
            {
                _model.AngleY += AngleSpeed;
            }
            
            if (e.Key == Key.Q)
            {
                _model.AngleZ += AngleSpeed;
            }
            
            if (e.Key == Key.E)
            {
                _model.AngleZ -= AngleSpeed;
            }

            if (e.Key == Key.F) {
                //if (cam.fi - Config.fiSpeed > (float)-Math.PI / 2)
                if (cam.fi - Config.fiSpeed > (float)-Math.PI)
                    cam.fi -= Config.fiSpeed;
                cam.CountParams();
                _model.ViewSpace = Matrix4x4.CreateLookAt(cam.camPos, cam.camTarget, cam.camUp);
            }

            if (e.Key == Key.H) {
                if (cam.fi + Config.fiSpeed < (float)Math.PI)
                    cam.fi += Config.fiSpeed;
                cam.CountParams();
                _model.ViewSpace = Matrix4x4.CreateLookAt(cam.camPos, cam.camTarget, cam.camUp);
            }

            if (e.Key == Key.T) {
                if (cam.tet - Config.tetSpeed > (float)0)
                    cam.tet -= Config.tetSpeed;
                cam.CountParams();
                _model.ViewSpace = Matrix4x4.CreateLookAt(cam.camPos, cam.camTarget, cam.camUp);
            }

            if (e.Key == Key.G) {
                if (cam.tet + Config.tetSpeed < (float)Math.PI) {
                    cam.tet += Config.tetSpeed;
                }
                cam.CountParams();
                _model.ViewSpace = Matrix4x4.CreateLookAt(cam.camPos, cam.camTarget, cam.camUp);
            }

            ShowModel();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _model.Scale += ScaleSpeed;

            }
            else if (e.Delta < 0)
            {
                _model.Scale -= ScaleSpeed;
            }

        ShowModel();
        }
    }
}