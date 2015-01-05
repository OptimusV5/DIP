/**
 * Created by OptimusV5 on 2014/11/2.
 */

import java.awt.image.BufferedImage;

public class DIP2 {
    //Constructor
    DIP2() {
    }
    //Save Menu Item Listener
    public BufferedImage equalize_hist(BufferedImage inputImage) {
        int w = inputImage.getWidth();
        int h = inputImage.getHeight();
        int grays[] = new int[256];
        int pixes[] = new int[w * h];
        inputImage.getRGB(0, 0, w, h, pixes, 0, w);
        for(int pix : pixes) {
            grays[0x000000ff & pix]++;
        }
        int now = 0;
        for (int i = 0; i < 256; i++) {
            now += grays[i];
            grays[i] = (int)((255 * now/(float)(w * h)));
        }
        BufferedImage outImage = new BufferedImage(w,h,BufferedImage.TYPE_INT_RGB);
        for (int i = 0; i < h; i++) {
            for (int j = 0; j < w; j++) {
                int temp = grays[0x000000ff & pixes[i * w + j]];
                temp = (0xff << 24) | (temp << 16) | (temp << 8) | temp;
                outImage.setRGB(j,i,temp);
            }
        }
        return outImage;
    }
    public byte[][][] view_as_window(BufferedImage image, int x, int y) {
        int w = image.getWidth();
        int h = image.getHeight();
        byte[][][] output = new byte[(w - x + 1) * (h - y + 1)][y][x];
        for (int i = 0; i < (w - x + 1) * (h - y + 1);i++) {
            for (int j = 0; j < y; j++) {
                for (int k = 0; k < x; k++) {
                    output[i][j][k] = (byte)(image.getRGB(i % (w - x + 1) + k, i / (w - x + 1) + j));
                }
            }
        }
        return output;
    }
    public BufferedImage filter2D(BufferedImage inputImage,Filter filter) {
        int weight = (filter.type == 1 ? filter.size * filter.size : 1);
        int[][] newFilter = new int[filter.size][filter.size];
        if (filter.type == 1) {
            for (int i = 0; i < filter.size; i++)
                for (int j = 0; j < filter.size; j++)
                    newFilter[i][j] = 1;
        } else if (filter.type == 2) {
            for (int i = 0; i < filter.size; i++) {
                for (int j = 0; j < filter.size; j++) {
                    if (i == 1 && j == 1)
                        newFilter[i][j] = -8;
                    else
                        newFilter[i][j] = 1;
                }
            }
        } else if (filter.type == 3) {
            newFilter[0][0] = newFilter[0][2] = -1;
            newFilter[2][0] = newFilter[2][2] = 1;
            newFilter[0][1] = -2;
            newFilter[2][1] = 2;
            newFilter[1][0] = newFilter[1][1] = newFilter[1][2] = 0;
        } else {
            newFilter[0][0] = newFilter[2][0] = -1;
            newFilter[0][2] = newFilter[2][2] = 1;
            newFilter[1][0] = -2;
            newFilter[1][2] = 2;
            newFilter[0][1] = newFilter[1][1] = newFilter[2][1] = 0;
        }
        BufferedImage tempImage = new BufferedImage(inputImage.getWidth(null) + filter.size - 1, inputImage.getHeight(null) + filter.size - 1, BufferedImage.TYPE_INT_RGB);
        for (int i = 0; i < tempImage.getHeight(); i++) {
            for (int j = 0; j < tempImage.getWidth(); j++) {
                    int tempW = (j - filter.size / 2 < 0 ? 0 : j - filter.size / 2);
                    tempW = (tempW >= inputImage.getWidth() ? inputImage.getWidth() - 1 : tempW);
                    int tempH = (i - filter.size / 2 < 0 ? 0 : i - filter.size / 2);
                    tempH = (tempH >= inputImage.getHeight() ? inputImage.getHeight() - 1 : tempH);
                    tempImage.setRGB(j, i, inputImage.getRGB(tempW, tempH));
            }
        }
        byte[][][] patches = view_as_window(tempImage,filter.size,filter.size);
        int[] pixes = new int[inputImage.getHeight() * inputImage.getWidth()];
        int max = -10000;
        int min = 10000;
        for (int i = 0; i < inputImage.getHeight() * inputImage.getWidth(); i++) {
            int temp = 0;
            for (int j = 0; j < filter.size; j++) {
                for (int k = 0; k < filter.size; k++) {
                    temp += newFilter[j][k] * (0x000000ff & patches[i][j][k]);
                }
            }
            pixes[i] = temp / weight;
            if (max < pixes[i])
                max = pixes[i];
            if (min > pixes[i])
                min = pixes[i];
//            if (pixes[i] < 0)
//                pixes[i] = 0;
//            if (pixes[i] > 255)
//                pixes[i] = 255;
        }
        if (filter.type != 1) {
            for (int i = 0; i < pixes.length; i++) {
                //pixes[i] += (0x000000ff & inputImage.getRGB(i % inputImage.getWidth(), i / inputImage.getWidth()));
                pixes[i] = (int) ((pixes[i] - min) / (float) (max - min) * 255);
            }
        }
        for (int i = 0; i < pixes.length; i++) {
            pixes[i] |= (pixes[i] << 8) | (pixes[i] << 16) | (0xff << 24);
        }
        BufferedImage outImage = new BufferedImage(inputImage.getWidth(null), inputImage.getHeight(null),BufferedImage.TYPE_INT_RGB);
        outImage.setRGB(0,0,inputImage.getWidth(),inputImage.getHeight(),pixes,0,inputImage.getWidth());
        return outImage;
    }
}
class Filter {
    public int size;
    public int type;
    Filter(int size, int type) {
        this.size = size;
        this.type = type;
    }
}