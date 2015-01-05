/**
 * Created by OptimusV5 on 2014/11/5.
 */

public class DIP1 {
    DIP1() { }
    public int[][] scaling(int[][] image, Tuple tuple) {
        int Width = tuple.width;
        int Height = tuple.height;
        int[][] finalImage = new int[Height][Width];
        double wW = (double)tuple.Width / Width;
        double hH = (double)tuple.Height / Height;
        for (int i = 0; i < Height; i++) {
            int h = (int)Math.floor(i * hH) < tuple.Height ? (int)Math.floor(i * hH) : tuple.Height - 1;
            for (int j = 0; j < Width; j++) {
                int w = (int)Math.floor(j * wW) < tuple.Width ? (int)Math.floor(j * wW) : tuple.Width - 1;
                if (h != tuple.Height - 1 && h != 0 && w != tuple.Width - 1 && w != 0) {
                    int getB[] = new int[4];
                    getB[0] = image[h - 1][w] & 0x000000ff;
                    getB[1] = image[h][w + 1] & 0x000000ff;
                    getB[2] = image[h + 1][w] & 0x000000ff;
                    getB[3] = image[h][w - 1] & 0x000000ff;
                    int aver = (getB[0] + getB[1] + getB[2] + getB[3]) / 4;
                    finalImage[i][j] = (0xff << 24) | (aver << 16) | (aver << 8) | aver;
                }
                else
                    finalImage[i][j] = image[h][w];
            }
        }
        return finalImage;
    }
    public int[][] quantize(int[][] image, int level, int width, int height) {
        int[][] finalImage = new int[height][width];
        int time = 255 / (level - 1);
        int ans[] = new int[level];
        for (int i = 0;i < level - 1; i++) {
            ans[i] = i * time;
        }
        ans[level - 1] = 255;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                int getIntRed = (0x00ff0000 & image[i][j]) >> 16; //get red since red == green == blue
                double getDoubleRed = getIntRed;
                int quantize;
                if (getDoubleRed / time - getIntRed / time <= 0.5) {
                    quantize = ans[getIntRed / time];
                } else {
                    if (getIntRed / time + 1 < level)
                        quantize = ans[getIntRed / time + 1];
                    else
                        quantize = 255;
                }
                finalImage[i][j] = (0xff << 24) | (quantize << 16) | (quantize << 8) | quantize;
            }
        }
        return finalImage;
    }
}

class Tuple {       //Tuple class to store width and height
    public int width;
    public int height;
    public int Width;
    public int Height;
    public Tuple() {
        width = 0;
        height = 0;
        Width = 0;
        Height = 0;
    }
    public Tuple(String string,int x,int y) {
        String s0[] = string.split(" * ",2);
        String s1[] = s0[1].split(" ");
        width = Integer.parseInt(s0[0]);
        height = Integer.parseInt(s1[1]);
        Width = x;
        Height = y;
    }
}


