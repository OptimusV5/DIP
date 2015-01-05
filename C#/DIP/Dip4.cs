using System;
using System.Collections.Generic;
using System.Linq;

namespace DIP
{
    internal class Dip4
    {
        private static byte[][] Patches(IList<byte> input, int size, int w, int h)
        {
            var tempImage = new byte[(w + size - 1)*(h + size - 1)];
            for (int i = 0; i < h + size - 1; i++)
            {
                for (int j = 0; j < w + size - 1; j++)
                {
                    int tempW = (j - size/2 < 0 ? 0 : j - size/2);
                    tempW = (tempW >= w ? w - 1 : tempW);
                    int tempH = (i - size/2 < 0 ? 0 : i - size/2);
                    tempH = (tempH >= h ? h - 1 : tempH);
                    tempImage[i*(w + size - 1) + j] = input[tempH*w + tempW];
                }
            }
            return Dip2.view_as_window(tempImage, size, size, w + size - 1, h + size - 1);
        }

        private static double[] GausNoiseGenerator(int w, int h, double mean, double sv)
        {
            var random = new Random();
            var output = new double[w*h];
            for (int i = 0; i < w*h; i++)
            {
                double u1 = random.NextDouble();
                double u2 = random.NextDouble();
                output[i] = Math.Sqrt(-2*Math.Log(u2))*Math.Sin(2*Math.PI*u1)*sv + mean;
            }
            return output;
        }

        public static byte[] GausNoiseImage(byte[] input, int w, int h, double mean, double sv)
        {
            double[] noises = GausNoiseGenerator(w, h, mean, sv);
            for (int i = 0; i < w*h; i++)
            {
                noises[i] += input[i];
                if (noises[i] >= 255)
                    noises[i] = 255;
                else if (noises[i] <= 0)
                    noises[i] = 0;
            }
            var output = new byte[w*h];
            for (int i = 0; i < w*h; i++)
                output[i] = Convert.ToByte(noises[i]);
            return output;
        }

        public static byte[] salt_and_pepperImage(byte[] input, int w, int h, double pa, double pb)
        {
            var output = new byte[w*h];
            var random = new Random();
            for (int i = 0; i < w*h; i++)
            {
                double p = random.NextDouble();
                if (p <= pa)
                {
                    output[i] = 255;
                }
                else if (p > pa && p <= pa + pb)
                {
                    output[i] = 0;
                }
                else
                {
                    output[i] = input[i];
                }
            }
            return output;
        }

        public static byte[] Mean_Filter(byte[] input, Filter filter, int w, int h, double q)
        {
            var temp = new double[w*h];
            byte[][] patches = Patches(input, filter.Size, w, h);
            for (int i = 0; i < w*h; i++)
            {
                switch (filter.Type)
                {
                    case 1:
                        temp[i] = 1;
                        for (int j = 0; j < filter.Size*filter.Size; j++)
                        {
                            temp[i] *= patches[i][j];
                        }
                        temp[i] = Math.Pow(temp[i], 1/(double) (filter.Size*filter.Size));
                        break;
                    case 2:
                        temp[i] = 0;
                        for (int j = 0; j < filter.Size*filter.Size; j++)
                        {
                            temp[i] += 1/(double) patches[i][j];
                        }
                        temp[i] = filter.Size*filter.Size/temp[i];
                        break;
                    case 3:
                        double tem = 0;
                        temp[i] = 0;
                        for (int j = 0; j < filter.Size*filter.Size; j++)
                        {
                            temp[i] += Math.Pow(patches[i][j], 1 + q);
                            tem += Math.Pow(patches[i][j], q);
                        }
                        temp[i] /= tem;
                        if (double.IsNaN(temp[i]))
                            temp[i] = 0;
                        break;
                }
            }
            double max = temp.Max();
            double min = temp.Min();
            var output = new byte[w*h];
            if (max <= 255 && min >= 0)
            {
                for (int i = 0; i < w*h; i++)
                    output[i] = Convert.ToByte(temp[i]);
                return output;
            }
            if (Math.Abs(max) < 0.00000001 && Math.Abs(min) < 0.00000001)
            {
                for (int i = 0; i < w*h; i++)
                {
                    output[i] = 0;
                }
            }
            else
            {
                for (int i = 0; i < w*h; i++)
                {
                    output[i] = Convert.ToByte((temp[i] - min)/(max - min)*255);
                }
            }
            return output;
        }

        public static byte[] Sta_Filter(byte[] input, int size, int type, int w, int h)
        {
            var output = new byte[w*h];
            byte[][] patches = Patches(input, size, w, h);
            for (int i = 0; i < w*h; i++)
            {
                Array.Sort(patches[i]);
                output[i] = patches[i][type];
            }
            return output;
        }

        public static int[] Color_His1(int[] input,int w,int h)
        {
            var rImage = new byte[w*h];
            var gImage = new byte[w*h];
            var bImage = new byte[w*h];
            for (var i = 0; i < w*h; i++)
            {
                bImage[i] = Convert.ToByte(0x000000ff & input[i]);
                gImage[i] = Convert.ToByte((0x0000ff00 & input[i]) >> 8);
                rImage[i] = Convert.ToByte((0x00ff0000 & input[i]) >> 16);
            }
            bImage = Dip2.equalize_hist(bImage, w, h);
            gImage = Dip2.equalize_hist(gImage, w, h);
            rImage = Dip2.equalize_hist(rImage, w, h);
            var output = new int[w*h];
            for (var i = 0; i < w*h; i++)
            {
                output[i] = (Convert.ToInt32(rImage[i]) << 16) | (Convert.ToInt32(gImage[i]) << 8) | Convert.ToInt32(bImage[i]);
            }
            return output;
        }

        public static int[] Color_His2(int[] input, int w, int h)
        {
            var rImage = new byte[w * h];
            var gImage = new byte[w * h];
            var bImage = new byte[w * h];
            var rgrays = new int[256];
            var ggrays = new int[256];
            var bgrays = new int[256];
            var hisImage = new double[w*h];
            for (var i = 0; i < w * h; i++)
            {
                bImage[i] = Convert.ToByte(0x000000ff & input[i]);
                bgrays[bImage[i]]++;
                gImage[i] = Convert.ToByte((0x0000ff00 & input[i]) >> 8);
                ggrays[gImage[i]]++;
                rImage[i] = Convert.ToByte((0x00ff0000 & input[i]) >> 16);
                rgrays[rImage[i]]++;
            }
            for (var i = 0; i < 256; i++)
            {
                hisImage[i] = (double)(rgrays[i] + ggrays[i] + bgrays[i])/3;
            }
            var now = 0.0;
            for (var i = 0; i < 256; i++)
            {
                now += hisImage[i];
                hisImage[i] = 255*now/(w*h);
            }
            for (var i = 0; i < w*h; i++)
            {
                rImage[i] = Convert.ToByte(hisImage[rImage[i]]);
                gImage[i] = Convert.ToByte(hisImage[gImage[i]]);
                bImage[i] = Convert.ToByte(hisImage[bImage[i]]);
            }
            var output = new int[w * h];
            for (var i = 0; i < w * h; i++)
            {
                output[i] = (Convert.ToInt32(rImage[i]) << 16) | (Convert.ToInt32(gImage[i]) << 8) | Convert.ToInt32(bImage[i]);
            }
            return output;
        }
    }
}