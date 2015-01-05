using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DIP
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly Hashtable _filter;
        private readonly Hashtable _quanSet;
        private readonly Hashtable _scalSet;
        private BitmapSource _image;
        private BitmapSource _colorImage;
        private byte[] _inPixels;
        private int[] _inColorPixels;
        private BitmapSource _outImage;

        public MainWindow()
        {
            InitializeComponent();
            _scalSet = new Hashtable();
            _quanSet = new Hashtable();
            _filter = new Hashtable();

            _scalSet.Add(Scal1, new Tuple<int, int>(12, 8));
            _scalSet.Add(Scal2, new Tuple<int, int>(24, 16));
            _scalSet.Add(Scal3, new Tuple<int, int>(48, 32));
            _scalSet.Add(Scal4, new Tuple<int, int>(96, 64));
            _scalSet.Add(Scal5, new Tuple<int, int>(192, 128));
            _scalSet.Add(Scal6, new Tuple<int, int>(300, 200));
            _scalSet.Add(Scal7, new Tuple<int, int>(450, 300));
            _scalSet.Add(Scal8, new Tuple<int, int>(500, 200));

            _quanSet.Add(Quan1, 128);
            _quanSet.Add(Quan2, 32);
            _quanSet.Add(Quan3, 8);
            _quanSet.Add(Quan4, 4);
            _quanSet.Add(Quan5, 2);

            _filter.Add(FilterItem1, new Filter(3, 1));
            _filter.Add(FilterItem2, new Filter(7, 1));
            _filter.Add(FilterItem3, new Filter(11, 1));
            _filter.Add(FilterItem4, new Filter(3, 2));
            _filter.Add(FilterItem5, new Filter(3, 3));
            _filter.Add(FilterItem6, new Filter(3, 4));
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Title = "Open an image", InitialDirectory = "E:\\"};
            if (openFileDialog.ShowDialog() != true) return;
            _image = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute));
            _colorImage = _image;
            _inPixels = new byte[_image.PixelHeight*_image.PixelWidth];
            _inColorPixels = new int[_image.PixelHeight * _image.PixelWidth];
            if (OriginImage != null) OriginImage.Source = _image;
            (_image = new FormatConvertedBitmap(_image, PixelFormats.Gray8, null, 0)).CopyPixels(_inPixels,
                _image.PixelWidth, 0);
            _colorImage = new FormatConvertedBitmap(_colorImage, PixelFormats.Bgr32, null, 0);
            _colorImage.CopyPixels(_inColorPixels,_colorImage.PixelWidth * 4,0);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (OutputImage.Source == null) return;
            var bs = (BitmapSource) OutputImage.Source;
            var saveFileDialog = new SaveFileDialog {DefaultExt = ".png"};
            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bs));
            if (saveFileDialog.ShowDialog() != true) return;
            var file = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite);
            png.Save(file);
            file.Close();
        }

        private void Scaling_Click(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            var tuple = (Tuple<int, int>) _scalSet[sender];
            byte[] outPixels = Dip1.Scaling(_inPixels, tuple, _image.PixelWidth, _image.PixelHeight);
            _outImage = BitmapSource.Create(tuple.Item1, tuple.Item2, _image.DpiX, _image.DpiY, _image.Format, null,
                outPixels, tuple.Item1);
            OutputImage.Source = _outImage;
        }

        private void Quantize_Click(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            byte[] outPixels = Dip1.Quantize(_inPixels, (int) _quanSet[sender], _image.PixelWidth, _image.PixelHeight);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void Plot_OnClick(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            plot_hist();
        }

        private void Equatize_OnClick(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            byte[] outPixels = Dip2.equalize_hist(_inPixels, _image.PixelWidth, _image.PixelHeight);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void PatchItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            if (sender.Equals(PatchItem1))
            {
                byte[][] outPixels = Dip2.view_as_window(_inPixels, 96, 64, _image.PixelWidth, _image.PixelHeight);
//                var outPixels = new byte[96 * 64];
//                var ptr = Dip2.view(_inPixels, 96, 64, 350, 220).ToInt32();
//                for (var i = 0; i < 96*64; i++)
//                {
//                    outPixels[i] = Convert.ToByte(Marshal.ReadInt32(new IntPtr(ptr + i*4)));
//                }
                _outImage = BitmapSource.Create(96, 64, _image.DpiX, _image.DpiY, _image.Format, null, outPixels[9], 96);
                OutputImage.Source = _outImage;
            }
            else
            {
                byte[][] outPixels = Dip2.view_as_window(_inPixels, 50, 50, _image.PixelWidth, _image.PixelHeight);
                _outImage = BitmapSource.Create(50, 50, _image.DpiX, _image.DpiY, _image.Format, null, outPixels[9], 50);
                OutputImage.Source = _outImage;
            }
        }

        private void Filter_OnClick(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            byte[] outPixels = Dip2.Filter2D(_inPixels, (Filter) _filter[sender], _image.PixelWidth, _image.PixelHeight);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void plot_hist()
        {
            var grays = new int[256];
            int max = 0;
            foreach (byte pix in _inPixels)
            {
                grays[pix]++;
                if (grays[pix] > max)
                    max = grays[pix];
            }
            for (int i = 0; i < 256; i++)
            {
                grays[i] = (int) (grays[i]*300/(float) max);
            }
            var outPixels = new byte[258*300];
            for (int i = 2; i < 258; i++)
            {
                for (int j = 299; j >= 0; j--)
                {
                    if (grays[i - 2] > 299 - j)
                    {
                        outPixels[i + j*258] = 0;
                    }
                    else
                    {
                        outPixels[i + j*258] = 255;
                    }
                }
            }
            _outImage = BitmapSource.Create(258, 300, _image.DpiX, _image.DpiY, _image.Format, null, outPixels, 258);
            OutputImage.Source = _outImage;
        }

        private void Dft_OnClick(object sender, RoutedEventArgs e)
        {
            var inPut = new Complex[_image.PixelWidth*_image.PixelHeight];
            for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
            {
                inPut[i] = _inPixels[i];
            }
            Complex[,] outPu = Dip3.Dft2D(inPut, 1, _image.PixelWidth, _image.PixelHeight);
            var outP = new Complex[_image.PixelWidth*_image.PixelHeight];
            var magnitude = new double[_image.PixelWidth*_image.PixelHeight];
            for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
            {
                outP[i] = outPu[i/_image.PixelWidth, i%_image.PixelWidth];
                magnitude[i] = Math.Log(outP[i].Magnitude + 1);
                //magnitude[i] = outP[i].Magnitude;
            }
            if (((MenuItem) sender).Header.Equals("DFT"))
            {
                if (_image == null) return;
                var outPixels = new byte[_image.PixelWidth*_image.PixelHeight];
                double max = magnitude.Max();
                double min = magnitude.Min();
                for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
                {
                    outPixels[i] = Convert.ToByte((magnitude[i] - min)/(max - min)*255);
                }
                _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                    _image.Format, null, outPixels, _image.PixelWidth);
                OutputImage.Source = _outImage;
            }
            else
            {
                Complex[,] outpu = Dip3.Dft2D(outP, 2, _image.PixelWidth, _image.PixelHeight);
                var outPixels = new byte[_image.PixelWidth*_image.PixelHeight];
                var outp = new double[_image.PixelWidth*_image.PixelHeight];
                for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
                {
                    outp[i] = outpu[i/_image.PixelWidth, i%_image.PixelWidth].Real*
                              Math.Pow(-1, i/_image.PixelWidth + i%_image.PixelWidth);
                }
                double max = outp.Max();
                double min = outp.Min();
                for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
                {
                    outPixels[i] = Convert.ToByte((outp[i] - min)/(max - min)*255);
                }
                _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                    _image.Format, null, outPixels, _image.PixelWidth);
                OutputImage.Source = _outImage;
            }
        }

        private void Fft_OnClick(object sender, RoutedEventArgs e)
        {
            int paddingW = Convert.ToInt32(Math.Floor(Math.Log(_image.PixelWidth, 2)));
            paddingW = Math.Abs(Math.Pow(2, paddingW) - _image.PixelWidth) < 0.000001
                ? 0
                : Convert.ToInt32(Math.Pow(2, paddingW + 1) - _image.PixelWidth);
            int paddingH = Convert.ToInt32(Math.Floor(Math.Log(_image.PixelHeight, 2)));
            paddingH = Math.Abs(Math.Pow(2, paddingH) - _image.PixelHeight) < 0.000001
                ? 0
                : Convert.ToInt32(Math.Pow(2, paddingH + 1) - _image.PixelHeight);
            var inPut = new Complex[_image.PixelWidth*_image.PixelHeight];
            for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
            {
                inPut[i] = _inPixels[i];
            }
            Complex[] outP = Dip3.Fft2D(inPut, 1, _image.PixelWidth, _image.PixelHeight);
            var magnitude = new double[(_image.PixelWidth + paddingW)*(_image.PixelHeight + paddingH)];
            for (int i = 0; i < (_image.PixelWidth + paddingW)*(_image.PixelHeight + paddingH); i++)
            {
                magnitude[i] = Math.Log(1 + outP[i].Magnitude);
                //magnitude[i] = outP[i].Magnitude;
            }
            if (sender.Equals(FftItem))
            {
                if (_image == null) return;
                var outPixels = new byte[(_image.PixelWidth + paddingW)*(_image.PixelHeight + paddingH)];
                double max = magnitude.Max();
                double min = magnitude.Min();
                for (int i = 0; i < (_image.PixelWidth + paddingW)*(_image.PixelHeight + paddingH); i++)
                {
                    outPixels[i] = Convert.ToByte((magnitude[i] - min)/(max - min)*255);
                }
                byte[] outPi = Dip1.Scaling(outPixels,
                    new Tuple<int, int>(_image.PixelWidth, _image.PixelHeight), _image.PixelWidth + paddingW,
                    _image.PixelHeight + paddingH);
                _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                    _image.Format, null, outPi, _image.PixelWidth);
                OutputImage.Source = _outImage;
            }
            else
            {
                Complex[] outpu = Dip3.Fft2D(outP, 2, _image.PixelWidth + paddingW, _image.PixelHeight + paddingH);
                var outPixels = new byte[_image.PixelWidth*_image.PixelHeight];
                var outp = new double[_image.PixelWidth*_image.PixelHeight];
                for (int i = 0; i < (_image.PixelWidth + paddingW)*(_image.PixelHeight + paddingH); i++)
                {
                    if (i/(_image.PixelWidth + paddingW) < _image.PixelHeight &&
                        i%(_image.PixelWidth + paddingW) < _image.PixelWidth)
                        outp[i/(_image.PixelWidth + paddingW)*_image.PixelWidth + i%(_image.PixelWidth + paddingW)] =
                            outpu[i].Real*
                            Math.Pow(-1, i/(_image.PixelWidth + paddingW) + i%(_image.PixelWidth + paddingW));
                }
                double max = outp.Max();
                double min = outp.Min();
                for (int i = 0; i < _image.PixelWidth*_image.PixelHeight; i++)
                {
                    outPixels[i] = Convert.ToByte((outp[i] - min)/(max - min)*255);
                }
                _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                    _image.Format, null, outPixels, _image.PixelWidth);
                OutputImage.Source = _outImage;
            }
        }

        private void Freq_Filter_OnClick(object sender, RoutedEventArgs e)
        {
            if (_image == null) return;
            if (sender.Equals(FreqItem1))
            {
                byte[] outPixels = Dip3.Filter2d_Frq(_inPixels, new Filter(11, 1), _image.PixelWidth, _image.PixelHeight);
                _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                    _image.Format, null, outPixels, _image.PixelWidth);
                OutputImage.Source = _outImage;
            }
            else
            {
                byte[] outPixels = Dip3.Filter2d_Frq(_inPixels, new Filter(3, 2), _image.PixelWidth, _image.PixelHeight);
                _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                    _image.Format, null, outPixels, _image.PixelWidth);
                OutputImage.Source = _outImage;
            }
        }

        private void Ari_OnClick(object sender, RoutedEventArgs e)
        {
            var size = ((MenuItem) sender).Header.ToString()[0] - '0';
            var outPixels = Dip2.Filter2D(_inPixels, new Filter(size, 1), _image.PixelWidth, _image.PixelHeight);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void Har_OnClick(object sender, RoutedEventArgs e)
        {
            var size = ((MenuItem) sender).Header.ToString()[0] - '0';
            var outPixels = Dip4.Mean_Filter(_inPixels, new Filter(size, 2), _image.PixelWidth, _image.PixelHeight, 0);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void Con_OnClick(object sender, RoutedEventArgs e)
        {
            var size = ((MenuItem) sender).Header.ToString()[0] - '0';
            var outPixels = Dip4.Mean_Filter(_inPixels, new Filter(size, 3), _image.PixelWidth, _image.PixelHeight, -1.5);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void T3_Nos_OnClick(object sender, RoutedEventArgs e)
        {
            var outPixels = Dip4.GausNoiseImage(_inPixels, _image.PixelWidth, _image.PixelHeight, 0, 40);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void T3_Den_OnCLick(object sender, RoutedEventArgs e)
        {
            var inPixels = Dip4.GausNoiseImage(_inPixels, _image.PixelWidth, _image.PixelHeight, 0, 40);
            var outPixels = new byte[_image.PixelHeight*_image.PixelWidth];
            if (((MenuItem) sender).Header.Equals("Arithmetic"))
            {
                outPixels = Dip2.Filter2D(inPixels, new Filter(3, 1), _image.PixelWidth, _image.PixelHeight);
            }
            else if (((MenuItem) sender).Header.Equals("Geometric"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 1), _image.PixelWidth, _image.PixelHeight, 0);
            }
            else if (((MenuItem) sender).Header.Equals("Harmonic"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 2), _image.PixelWidth, _image.PixelHeight, 0);
            }
            else if (((MenuItem) sender).Header.Equals("Contraharmontic"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 3), _image.PixelWidth, _image.PixelHeight, -1.5);
            }
            else if (((MenuItem) sender).Header.Equals("Median"))
            {
                outPixels = Dip4.Sta_Filter(inPixels, 3, 4, _image.PixelWidth, _image.PixelHeight);
            }
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void T4_Nos_OnClick(object sender, RoutedEventArgs e)
        {
            var outPixels = Dip4.salt_and_pepperImage(_inPixels, _image.PixelWidth, _image.PixelHeight, 0.2, 0);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void T4_Den_OnCLick(object sender, RoutedEventArgs e)
        {
            var inPixels = Dip4.salt_and_pepperImage(_inPixels, _image.PixelWidth, _image.PixelHeight, 0.2, 0);
            var outPixels = new byte[_image.PixelHeight*_image.PixelWidth];
            if (((MenuItem) sender).Header.Equals("Q < 0"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 3), _image.PixelWidth, _image.PixelHeight, -1.5);
            }
            else if (((MenuItem) sender).Header.Equals("Q > 0"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 3), _image.PixelWidth, _image.PixelHeight, 2);
            }
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void T5_Nos_OnClick(object sender, RoutedEventArgs e)
        {
            var outPixels = Dip4.salt_and_pepperImage(_inPixels, _image.PixelWidth, _image.PixelHeight, 0.2, 0.2);
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void T5_Den_OnCLick(object sender, RoutedEventArgs e)
        {
            var inPixels = Dip4.salt_and_pepperImage(_inPixels, _image.PixelWidth, _image.PixelHeight, 0.2, 0.2);
            var outPixels = new byte[_image.PixelHeight*_image.PixelWidth];
            if (((MenuItem) sender).Header.Equals("Arithmetic"))
            {
                outPixels = Dip2.Filter2D(inPixels, new Filter(3, 1), _image.PixelWidth, _image.PixelHeight);
            }
            else if (((MenuItem) sender).Header.Equals("Harmonic"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 2), _image.PixelWidth, _image.PixelHeight, 0);
            }
            else if (((MenuItem) sender).Header.Equals("Contraharmontic"))
            {
                outPixels = Dip4.Mean_Filter(inPixels, new Filter(3, 3), _image.PixelWidth, _image.PixelHeight, -1.5);
            }
            else if (((MenuItem) sender).Header.Equals("Min"))
            {
                outPixels = Dip4.Sta_Filter(inPixels, 3, 0, _image.PixelWidth, _image.PixelHeight);
            }
            else if (((MenuItem) sender).Header.Equals("Max"))
            {
                outPixels = Dip4.Sta_Filter(inPixels, 3, 8, _image.PixelWidth, _image.PixelHeight);
            }
            else if (((MenuItem) sender).Header.Equals("Median"))
            {
                outPixels = Dip4.Sta_Filter(inPixels, 3, 4, _image.PixelWidth, _image.PixelHeight);
            }
            _outImage = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                _image.Format, null, outPixels, _image.PixelWidth);
            OutputImage.Source = _outImage;
        }

        private void His_OnClick(object sender, RoutedEventArgs e)
        {
            var outPiexls = ((MenuItem) sender).Header.Equals("Target1") ? Dip4.Color_His1(_inColorPixels, _colorImage.PixelWidth, _colorImage.PixelHeight) : Dip4.Color_His2(_inColorPixels, _colorImage.PixelWidth, _colorImage.PixelHeight);
            _outImage = BitmapSource.Create(_colorImage.PixelWidth, _colorImage.PixelHeight, _colorImage.DpiX, _colorImage.DpiY,
               _colorImage.Format, null, outPiexls, _colorImage.PixelWidth * 4);
            OutputImage.Source = _outImage;
        }
    }
}