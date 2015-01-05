using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace DIP
{
    internal class Dip3
    {
        public static Complex[][] Parameters;

        public static Complex[,] Dft2D(Complex[] image, int flag, int w, int h)
        {
            var input = new Complex[h, w];
            var paraW = new Complex[w, w];
            var paraH = new Complex[h, h];
            var parasW = new Complex[w];
            var parasH = new Complex[h];
            int para = flag == 1 ? -2 : 2;
            int mn = flag == 1 ? 1 : w*h;
            for (int i = 0; i < w; i++)
            {
                parasW[i] = new Complex(Math.Cos(para*Math.PI*i/w), Math.Sin(para*Math.PI*i/w));
            }
            for (int i = 0; i < h; i++)
            {
                parasH[i] = new Complex(Math.Cos(para*Math.PI*i/h), Math.Sin(para*Math.PI*i/h));
            }
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (flag == 1)
                        input[i, j] = image[i*w + j]*Math.Pow(-1, i + j);
                    else
                        input[i, j] = image[i*w + j];
                }
            }

            for (int j = 0; j < w; j++)
            {
                for (int k = 0; k < w; k++)
                {
                    paraW[j, k] = parasW[(j*k)%w];
                }
            }

            for (int j = 0; j < h; j++)
            {
                for (int k = 0; k < h; k++)
                {
                    paraH[k, j] = parasH[(j*k)%h];
                }
            }
            Matrix<Complex> wMatrix = DenseMatrix.OfArray(paraW);
            Matrix<Complex> nMatrix = DenseMatrix.OfArray(paraH);
            Matrix<Complex> matrix = DenseMatrix.OfArray(input);
            return nMatrix.Multiply(matrix).Multiply(wMatrix).Divide(mn).ToArray();
        }

        public static Complex[] Fft2D(Complex[] image, int flag, int w, int h)
        {
            int paddingW = Convert.ToInt32(Math.Floor(Math.Log(w, 2)));
            paddingW = Math.Abs(Math.Pow(2, paddingW) - w) < 0.000001
                ? 0
                : Convert.ToInt32(Math.Pow(2, paddingW + 1) - w);
            int paddingH = Convert.ToInt32(Math.Floor(Math.Log(h, 2)));
            paddingH = Math.Abs(Math.Pow(2, paddingH) - h) < 0.000001
                ? 0
                : Convert.ToInt32(Math.Pow(2, paddingH + 1) - h);
            var input = new Complex[h + paddingH][];
            var temp = new Complex[w + paddingW][];
            var output = new Complex[(w + paddingW)*(h + paddingH)];
            int mn = flag == 1 ? 1 : (w + paddingW)*(h + paddingH);
            int lengths = Convert.ToInt32(Math.Log(w + paddingW > h + paddingH ? w + paddingW : h + paddingH, 2));
            Parameters = new Complex[(w + paddingW > h + paddingH ? w + paddingW : h + paddingH) + 1][];
            for (int i = 0; i < lengths; i++)
            {
                int length = Convert.ToInt32(Math.Pow(2, i));
                Parameters[length*2] = new Complex[length];
                double fac = (flag == 1 ? -2 : 2)*Math.PI/(length*2);
                for (int j = 0; j < length; j++)
                {
                    Parameters[length*2][j] = new Complex(Math.Cos(fac*j), Math.Sin(fac*j));
                }
            }
            for (int i = 0; i < h + paddingH; i++)
            {
                input[i] = new Complex[w + paddingW];
                for (int j = 0; j < w + paddingW; j++)
                {
                    if (j < w && i < h)
                        input[i][j] = (flag == 2 ? 1 : Math.Pow(-1, i + j))*image[i*w + j];
                    else
                    {
                        input[i][j] = 0;
                    }
                }
            }
            for (int i = 0; i < w + paddingW; i++)
            {
                temp[i] = new Complex[h + paddingH];
            }
            for (int i = 0; i < h + paddingH; i++)
            {
                Complex[] t = Fft(input[i]);
                for (int j = 0; j < w + paddingW; j++)
                {
                    temp[j][i] = t[j];
                }
            }
            for (int i = 0; i < w + paddingW; i++)
            {
                Complex[] t = Fft(temp[i]);
                for (int j = 0; j < h + paddingH; j++)
                {
                    output[i + j*(w + paddingW)] = t[j]/mn;
                }
            }
            return output;
        }

        private static Complex[] Fft(IList<Complex> input)
        {
            if (input.Count == 1)
                return new[] {input[0]};
            int length = input.Count;
            int half = length/2;
            var output = new Complex[length];
            var evens = new Complex[half];
            for (int i = 0; i < half; i++)
            {
                evens[i] = input[2*i];
            }
            Complex[] evenResult = Fft(evens);
            var odds = new Complex[half];
            for (int i = 0; i < half; i++)
            {
                odds[i] = input[2*i + 1];
            }
            Complex[] oddResult = Fft(odds);
            for (int i = 0; i < half; i++)
            {
                Complex oddPart = oddResult[i]*Parameters[length][i];
                output[i] = evenResult[i] + oddPart;
                output[i + half] = evenResult[i] - oddPart;
            }
            return output;
        }

        public static byte[] Filter2d_Frq(byte[] image, Filter filter, int w, int h)
        {
            int paddingW = Convert.ToInt32(Math.Floor(Math.Log(w + filter.Size - 1, 2)));
            paddingW = Math.Abs(Math.Pow(2, paddingW) - (w + filter.Size - 1)) < 0.000001
                ? 0
                : Convert.ToInt32(Math.Pow(2, paddingW + 1) - (w + filter.Size - 1));
            int paddingH = Convert.ToInt32(Math.Floor(Math.Log(h + filter.Size - 1, 2)));
            paddingH = Math.Abs(Math.Pow(2, paddingH) - (h + filter.Size - 1)) < 0.000001
                ? 0
                : Convert.ToInt32(Math.Pow(2, paddingH + 1) - (h + filter.Size - 1));
            var input = new Complex[(w + filter.Size - 1)*(h + filter.Size - 1)];
            var ava = new Complex[(w + filter.Size - 1)*(h + filter.Size - 1)];
            var outImage = new byte[w*h];
            var newFilter = new double[filter.Size*filter.Size];
            switch (filter.Type)
            {
                case 1:
                    for (int i = 0; i < filter.Size*filter.Size; i++)
                        newFilter[i] = 1/(double) 121;
                    break;
                case 2:
                    for (int i = 0; i < filter.Size*filter.Size; i++)
                    {
                        if (i == 4)
                            newFilter[i] = -8;
                        else
                            newFilter[i] = 1;
                    }
                    break;
            }
            for (int i = 0; i < h + filter.Size - 1; i++)
            {
                for (int j = 0; j < w + filter.Size - 1; j++)
                {
                    int tempW = (j - filter.Size/2 < 0 ? 0 : j - filter.Size/2);
                    tempW = (tempW >= w ? w - 1 : tempW);
                    int tempH = (i - filter.Size/2 < 0 ? 0 : i - filter.Size/2);
                    tempH = (tempH >= h ? h - 1 : tempH);
                    input[i*(w + filter.Size - 1) + j] = image[tempH*w + tempW];
//                    if (i < h && j < w)
//                        input[i*(w + filter.Size - 1) + j] = image[i*w + j];
//                    else
//                    {
//                        input[i*(w + filter.Size - 1) + j] = 0;
//                    }
                }
            }
            for (int i = 0; i < h + filter.Size - 1; i++)
            {
                for (int j = 0; j < w + filter.Size - 1; j++)
                {
                    if (i < filter.Size && j < filter.Size)
                    {
                        ava[i*(w + filter.Size - 1) + j] = newFilter[(i - 0)*filter.Size + j - 0];
                    }
                    else
                    {
                        ava[i*(w + filter.Size - 1) + j] = 0;
                    }
                }
            }
            Complex[] fft = Fft2D(input, 1, (w + filter.Size - 1), (h + filter.Size - 1));
            Complex[] average = Fft2D(ava, 1, (w + filter.Size - 1), (h + filter.Size - 1));
            var fftVector = new DenseVector(fft);
            var avaVector = new DenseVector(average);
            Complex[] result = Fft2D(fftVector.PointwiseMultiply(avaVector).ToArray(), 2,
                (w + filter.Size - 1 + paddingW),
                (h + filter.Size - 1 + paddingH));
            var outp = new double[w*h];
            for (int i = filter.Size - 1; i < h + filter.Size - 1; i++)
            {
                for (int j = filter.Size - 1; j < w + filter.Size - 1; j++)
                {
                    outp[(i - filter.Size + 1)*w + j - filter.Size + 1] =
                        result[i*(w + filter.Size - 1 + paddingW) + j].Real*Math.Pow(-1, i + j);
                }
            }
//            var outp = new double[w * h];
//            var dft = Dft2D(input, 1, (w + filter.Size - 1), (h + filter.Size - 1));
//            var average = Dft2D(ava, 1, (w + filter.Size - 1), (h + filter.Size - 1));
//            Matrix<Complex> dftMatrix = DenseMatrix.OfArray(dft);
//            Matrix<Complex> avaMatrix = DenseMatrix.OfArray(average);
//            var result = Dft2D(dftMatrix.PointwiseMultiply(avaMatrix).ToRowWiseArray(), 2, (w + filter.Size - 1), (h + filter.Size - 1));
//            for (var i = 0; i < h ; i++)
//            {
//                for (var j = 0; j < w ; j++)
//                {
//                    outp[i * w + j] = result[i, j].Real* Math.Pow(-1, i + j);
//                }
//            }
            double max = outp.Max();
            double min = outp.Min();
            for (int i = 0; i < w*h; i++)
            {
                outImage[i] = Convert.ToByte((outp[i] - min)/(max - min)*255);
            }
            return outImage;
        }
    }
}