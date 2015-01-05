using System;


namespace DIP
{
    internal class Dip1
    {
        public static byte[] Scaling(byte[] image, Tuple<int, int> tuple, int width, int height)
        {
// ReSharper disable once InconsistentNaming
            var Width = tuple.Item1;
// ReSharper disable once InconsistentNaming
            var Height = tuple.Item2;
            var finalImage = new byte[Height, Width];
            var wW = (double) width/Width;
            var hH = (double) height/Height;
            for (var i = 0; i < Height; i++)
            {
                var h = (int) Math.Floor(i*hH) < height ? (int) Math.Floor(i*hH) : height - 1;
                for (var j = 0; j < Width; j++)
                {
                    var w = (int) Math.Floor(j*wW) < width ? (int) Math.Floor(j*wW) : width - 1;
                    if (h != height - 1 && h != 0 && w != width - 1 && w != 0)
                    {
                        var getB = new byte[4];
                        getB[0] = image[(h - 1)*width + w];
                        getB[1] = image[h*width + w + 1];
                        getB[2] = image[(h + 1)*width + w];
                        getB[3] = image[h*width + w - 1];
                        var aver = (byte) ((getB[0] + getB[1] + getB[2] + getB[3])/4);
                        finalImage[i, j] = aver;
                    }
                    else
                        finalImage[i, j] = image[h*width + w];
                }
            }
            var final = new byte[Width*Height];
            for (var i = 0; i < tuple.Item2; i++)
            {
                for (var j = 0; j < tuple.Item1; j++)
                {
                    final[i*tuple.Item1 + j] = finalImage[i, j];
                }
            }
            return final;
        }

        public static byte[] Quantize(byte[] image, int level, int width, int height)
        {
            var finalImage = new byte[height*width];
            var time = 255/(level - 1);
            var ans = new byte[level];
            for (var i = 0; i < level - 1; i++)
            {
                ans[i] = (byte) (i*time);
            }
            ans[level - 1] = 255;
            for (var i = 0; i < height*width; i++)
            {
                double getDoubleGray = image[i];
// ReSharper disable once PossibleLossOfFraction
                if (getDoubleGray/time - image[i]/time <= 0.5)
                {
                    finalImage[i] = ans[image[i]/time];
                }
                else
                {
                    if (image[i]/time + 1 < level)
                        finalImage[i] = ans[image[i]/time + 1];
                    else
                        finalImage[i] = 255;
                }
            }
            return finalImage;
        }
    }
}