namespace DIP
{
    internal class Dip2
    {
//        [DllImport("E:\\VSProject\\DIP\\DIP\\Fast.dll", CallingConvention = CallingConvention.Cdecl)]
//        public static extern IntPtr view(byte[] image, int x, int y, int w, int h);
        public static byte[] equalize_hist(byte[] inputImage, int w, int h)
        {
            var grays = new int[256];
            var outImage = new byte[w*h];
            foreach (var pix in inputImage)
            {
                grays[pix]++;
            }
            var now = 0;
            for (var i = 0; i < 256; i++)
            {
                now += grays[i];
                grays[i] = (byte) ((255*now/(float) (w*h)));
            }
            for (var i = 0; i < h*w; i++)
            {
                outImage[i] = (byte) grays[inputImage[i]];
            }
            return outImage;
        }

        public static byte[][] view_as_window(byte[] image, int x, int y, int w, int h)
        {   
            var output = new byte[(w - x + 1)*(h - y + 1)][];
            for (var i = 0; i < (w - x + 1)*(h - y + 1); i++)
            {
                output[i] = new byte[y*x];
                for (var j = 0; j < y; j++)
                {
                    for (var k = 0; k < x; k++)
                    {
                        output[i][j*x + k] = image[(i/(w - x + 1) + j)*w + i%(w - x + 1) + k];
                    }
                }
            }
//            var im = new int[h*w];
//            for(var i = 0; i < h * w ;i++)
//            {
//                im[i] = image[i];
//            }
//            var output = new byte[h * w];
//            var ptr = view(image, x, y, w, h).ToInt32();
//            for (var i = 0; i < w * h; i++)
//            {
//                 int temp = Marshal.ReadInt32(new IntPtr(ptr + i * 4));
//                output[i] = Convert.ToByte(temp);
//                //Console.WriteLine(output[i]);
//            }
            return output;
        }

        public static byte[] Filter2D(byte[] inputImage, Filter filter, int w, int h)
        {
            var weight = (filter.Type == 1 ? filter.Size*filter.Size : 1);
            var newFilter = new int[filter.Size * filter.Size];
            switch (filter.Type)
            {
                case 1:
                    for (var i = 0; i < filter.Size * filter.Size; i++)
                        newFilter[i] = 1;
                    break;
                case 2:
                    for (var i = 0; i < filter.Size * filter.Size; i++)
                    {
                        if (i == 4)
                            newFilter[i] = -8;
                        else
                            newFilter[i] = 1;
                    }
                    break;
                case 3:
                    newFilter[0] = newFilter[2] = 1;
                    newFilter[6] = newFilter[8] = -1;
                    newFilter[1] = 2;
                    newFilter[7] = -2;
                    newFilter[3] = newFilter[4] = newFilter[5] = 0;
                    break;
                default:
                    newFilter[0] = newFilter[6] = 1;
                    newFilter[2] = newFilter[8] = -1;
                    newFilter[3] = 2;
                    newFilter[5] = -2;
                    newFilter[1] = newFilter[4] = newFilter[7] = 0;
                    break;
            }
            var tempImage = new byte[(w + filter.Size - 1) * (h + filter.Size - 1)]; 
            for (var i = 0; i < h + filter.Size - 1; i++)
            {
                for (var j = 0; j < w + filter.Size - 1; j++)
                {
                    var tempW = (j - filter.Size/2 < 0 ? 0 : j - filter.Size/2);
                    tempW = (tempW >= w ? w - 1 : tempW);
                    var tempH = (i - filter.Size/2 < 0 ? 0 : i - filter.Size/2);
                    tempH = (tempH >= h ? h - 1 : tempH);
                    tempImage[i * (w + filter.Size - 1) + j] = inputImage[tempH * w + tempW];
                }
            }
            var patches = view_as_window(tempImage, filter.Size, filter.Size, w + filter.Size - 1, h + filter.Size - 1);
            var pixes = new int[w * h];
            var max = -10000;
            var min = 10000;
            for (var i = 0; i < h*w; i++)
            {
                var temp = 0;
                for (var j = 0; j < filter.Size * filter.Size; j++)
                {
                    temp += newFilter[j] * patches[i][j];
                }
                pixes[i] = temp/weight;
                if (max < pixes[i])
                    max = pixes[i];
                if (min > pixes[i])
                    min = pixes[i];
//            if (pixes[i] < 0)
//                pixes[i] = 0;
//            if (pixes[i] > 255)
//                pixes[i] = 255;
            }
            var outImage = new byte[w * h];
            for (var i = 0; i < w*h; i++)
            {
                //pixes[i] += inputImage[i];
                if (filter.Type != 1)
                    pixes[i] = (int) ((pixes[i] - min)/(float) (max - min)*255);
                outImage[i] = (byte)pixes[i];
            }
            return outImage;
        }
    }
    class Filter
    {
        public readonly int Size;
        public readonly int Type;
        public Filter(int size, int type)
        {
            Size = size;
            Type = type;
        }
    }
}