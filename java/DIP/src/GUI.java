/**
 * Created by OptimusV5 on 2014/11/5.
 */
import javax.imageio.ImageIO;
import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.awt.image.BufferedImage;
import java.io.File;
import java.util.ArrayList;
import java.util.Random;

public class GUI {
    private JFrame frame;
    private MenuBar menuBar;
    private Menu file;
    private Menu hist;
    private Menu patch;
    private Menu filter;
    private Menu scaling;
    private Menu quantize;
    private MenuItem menuItemSave;
    private MenuItem menuItemOpen;
    private MenuItem menuItemPlot;
    private MenuItem menuItemEqualize;
    private MenuItem menuItemPatch1;
    private MenuItem menuItemPatch2;
    private MenuItem menuItemFilter1;
    private MenuItem menuItemFilter2;
    private MenuItem menuItemFilter3;
    private MenuItem menuItemFilter4;
    private MenuItem menuItemFilter5;
    private MenuItem menuItemFilter6;
    private BufferedImage bufferedImage;
    private BufferedImage image;
    private JPanel jPanel1;
    private JPanel jPanel2;
    private DIP2 dip2;
    private DIP1 dip1;
    private ArrayList<MenuItem> Scalings; //ArrayList of Scaling Scalings
    private ArrayList<MenuItem> quantizes; //ArrayList of Quantize Scalings
    //Constructor
    GUI() {
        dip1 = new DIP1();
        dip2 = new DIP2();
        frame = new JFrame();
        menuBar = new MenuBar();
        file = new Menu("File");
        scaling = new Menu("Scaling");
        quantize = new Menu("Quantize");
        Scalings = new ArrayList<MenuItem>();
        quantizes = new ArrayList<MenuItem>();
        hist = new Menu("Hist");
        patch = new Menu("Patch");
        filter = new Menu("Filter");
        menuItemOpen = new MenuItem("Open");
        menuItemSave = new MenuItem("Save As");
        menuItemPlot = new MenuItem("Plot");
        menuItemEqualize = new MenuItem("Equalize");
        menuItemPatch1 = new MenuItem("96 * 64");
        menuItemPatch2 = new MenuItem("50 * 50");
        menuItemFilter1 = new MenuItem("3 * 3");
        menuItemFilter2 = new MenuItem("7 * 7");
        menuItemFilter3 = new MenuItem("11 * 11");
        menuItemFilter4 = new MenuItem("Laplacian");
        menuItemFilter5 = new MenuItem("Sobel1");
        menuItemFilter6 = new MenuItem("Sobel2");
        Scalings.add(new MenuItem("12 * 8"));
        Scalings.add(new MenuItem("24 * 16"));
        Scalings.add(new MenuItem("48 * 32"));
        Scalings.add(new MenuItem("96 * 64"));
        Scalings.add(new MenuItem("192 * 128"));
        Scalings.add(new MenuItem("300 * 200"));
        Scalings.add(new MenuItem("450 * 300"));
        Scalings.add(new MenuItem("500 * 200"));
        quantizes.add(new MenuItem("128"));
        quantizes.add(new MenuItem("32"));
        quantizes.add(new MenuItem("8"));
        quantizes.add(new MenuItem("4"));
        quantizes.add(new MenuItem("2"));
        jPanel1 = new JPanel();
        jPanel2 = new JPanel();
    }
    //function to form the GUI
    public void run() {
        file.add(menuItemOpen);
        file.add(menuItemSave);
        hist.add(menuItemPlot);
        hist.add(menuItemEqualize);
        patch.add(menuItemPatch1);
        patch.add(menuItemPatch2);
        filter.add(menuItemFilter1);
        filter.add(menuItemFilter2);
        filter.add(menuItemFilter3);
        filter.add(menuItemFilter4);
        filter.add(menuItemFilter5);
        filter.add(menuItemFilter6);
        menuBar.add(file);
        menuBar.add(scaling);
        menuBar.add(quantize);
        menuBar.add(hist);
        menuBar.add(patch);
        menuBar.add(filter);
        frame.setSize(800, 700);
        frame.setLocation(30, 30);
        frame.setLayout(new GridLayout(2, 1));
        frame.setMenuBar(menuBar);
        frame.setResizable(false);
        frame.setVisible(true);
        frame.add(jPanel1);
        frame.add(jPanel2);
        jPanel1.setLayout(new BorderLayout());
        jPanel2.setLayout(new BorderLayout());
        menuItemOpen.addActionListener(openActionListener);
        menuItemSave.addActionListener(saveActionListener);
        menuItemPlot.addActionListener(plotActionListener);
        menuItemEqualize.addActionListener(equalizeActionListener);
        menuItemPatch1.addActionListener(patchActionListener);
        menuItemPatch2.addActionListener(patchActionListener);
        menuItemFilter1.addActionListener(filterActionListener);
        menuItemFilter2.addActionListener(filterActionListener);
        menuItemFilter3.addActionListener(filterActionListener);
        menuItemFilter4.addActionListener(filterActionListener);
        menuItemFilter5.addActionListener(filterActionListener);
        menuItemFilter6.addActionListener(filterActionListener);
        for (final MenuItem menuItem : Scalings) {     //Scaling option
            menuItem.addActionListener(scalingActionListener);
            scaling.add(menuItem);
        }
        for (final MenuItem menuItem : quantizes) {
            menuItem.addActionListener(quantizeActionListener);
            quantize.add(menuItem);
        }
        frame.addWindowListener(new WindowAdapter() {
            @Override
            public void windowClosing(WindowEvent e) {
                System.exit(0);
            }
        });
    }
    public ActionListener scalingActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            try {
                int height = image.getHeight();
                int width = image.getWidth();
                int inputImage[][] = new int[height][width];
                for (int i = 0; i < height; i++) {
                    for (int j = 0; j < width; j++) {
                        inputImage[i][j] = image.getRGB(j,i);    //get origin image matrix
                    }
                }
                Tuple t = new Tuple(e.getActionCommand(),width,height);
                int[][] output_image = dip1.scaling(inputImage, t); //image is a matrix; t is a tuple to store scaling size; width and height is the size of origin image
                bufferedImage = new BufferedImage(t.width,t.height,BufferedImage.TYPE_INT_RGB);
                for (int y = 0; y < t.height; y++) {
                    for (int x = 0; x < t.width; x++) {
                        bufferedImage.setRGB(x, y, output_image[y][x]);
                    }
                }
                jPanel2.removeAll();
                jPanel2.add(new ImagePane(bufferedImage));
                frame.validate();
                frame.repaint();
            } catch (Exception ioE) {
                ioE.printStackTrace();
            }
        }
    };
    public ActionListener quantizeActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            try {
                int height = image.getHeight();
                int width = image.getWidth();
                int inputImage[][] = new int[height][width];
                for (int i = 0; i < height; i++) {
                    for (int j = 0; j < width; j++) {
                        inputImage[i][j] = image.getRGB(j,i);
                    }
                }
                int[][] output_image = dip1.quantize(inputImage, Integer.parseInt(e.getActionCommand()), width, height);
                bufferedImage = new BufferedImage(width,height,BufferedImage.TYPE_INT_RGB);
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        bufferedImage.setRGB(x,y,output_image[y][x]);
                    }
                }
                jPanel2.removeAll();
                jPanel2.add(new ImagePane(bufferedImage));
                frame.validate();
                frame.repaint();
            } catch (Exception ioE) {
                ioE.printStackTrace();
            }
        }
    };
    //Open Menu Item Listener
    public ActionListener openActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            FileDialog fileDialog = new FileDialog(frame,"Open an image",FileDialog.LOAD);
            fileDialog.setVisible(true);
            String fileName = fileDialog.getDirectory() + fileDialog.getFile();
            File imageFile = new File(fileName);
            try {
                image = ImageIO.read(imageFile);
                BufferedImage rgbImage = new BufferedImage(image.getWidth(null),image.getHeight(null),BufferedImage.TYPE_INT_RGB);
                bufferedImage = new BufferedImage(image.getWidth(null),image.getHeight(null),BufferedImage.TYPE_INT_RGB);
                rgbImage.getGraphics().drawImage(image,0,0,image.getWidth(null),image.getHeight(null),null);
                image = rgbImage;
                bufferedImage = rgbImage;
                frame.setTitle(fileName);
                jPanel1.removeAll();
                jPanel1.add(new ImagePane(image));
                frame.validate();
                frame.repaint();
            }catch (Exception ioE) {
                ioE.printStackTrace();
            }
        }
    };
    //Save Menu Item Listener
    public ActionListener saveActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            JFileChooser jFileChooser = new JFileChooser();
            jFileChooser.getFileSelectionMode();
            jFileChooser.showSaveDialog(frame);
            String fileName = jFileChooser.getSelectedFile().getPath() + ".png";
            try {
                ImageIO.write(bufferedImage, "png", new File(fileName));
            } catch (Exception ioe) {
                ioe.printStackTrace();
            }
        }
    };
    //Plot Menu Item Listener
    public ActionListener plotActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            plot_hist();
            jPanel2.removeAll();
            jPanel2.add(new ImagePane(bufferedImage));
            frame.validate();
            frame.repaint();
        }
    };
    //Equalize Menu Item Listener
    public ActionListener equalizeActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            bufferedImage = new BufferedImage(image.getWidth(null),image.getHeight(null),BufferedImage.TYPE_INT_RGB);
            bufferedImage = dip2.equalize_hist(image);
            jPanel2.removeAll();
            jPanel2.add(new ImagePane(bufferedImage));
            frame.validate();
            frame.repaint();
        }
    };
    public ActionListener patchActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            int x = 0;
            int y = 0;
            if (e.getActionCommand().equals("96 * 64")) {
                x = 96;
                y = 64;
            }
            else if (e.getActionCommand().equals("50 * 50")) {
                x = 50;
                y = 50;
            }
            byte[][][] outPatch = dip2.view_as_window(image,x,y);
            String fileName = "";
            Random random = new Random();
            for (int i = 0; i < 8; i ++) {
                int temp = random.nextInt((image.getWidth() - x + 1) * (image.getHeight() - y + 1));
                BufferedImage newImage = new BufferedImage(x,y,BufferedImage.TYPE_INT_RGB);
                for (int j = 0; j < x; j++) {
                    for (int k = 0; k < y; k++) {
                        int rgb  = (0x000000ff & outPatch[temp][k][j]);
                        rgb = (0xff << 24) | (rgb << 16) | (rgb << 8) | rgb;
                        newImage.setRGB(j,k,rgb);
                    }
                }
                if (i == 0) {
                    JFileChooser jFileChooser = new JFileChooser();
                    jFileChooser.getFileSelectionMode();
                    jFileChooser.showSaveDialog(frame);
                    fileName = jFileChooser.getSelectedFile().getPath();
                }
                try {
                    ImageIO.write(newImage, "png", new File(fileName + i + ".png"));
                    System.out.print(fileName);
                } catch (Exception ioe) {
                    ioe.printStackTrace();
                }
            }
        }
    };
    public ActionListener filterActionListener = new ActionListener() {
        @Override
        public void actionPerformed(ActionEvent e) {
            bufferedImage = new BufferedImage(image.getWidth(null),image.getHeight(null),BufferedImage.TYPE_INT_RGB);
            if (e.getActionCommand().equals("3 * 3"))
                bufferedImage = dip2.filter2D(image,new Filter(3,1));
            else if (e.getActionCommand().equals("7 * 7"))
                bufferedImage = dip2.filter2D(image,new Filter(7,1));
            else if (e.getActionCommand().equals("11 * 11"))
                bufferedImage = dip2.filter2D(image,new Filter(11,1));
            else if (e.getActionCommand().equals("Laplacian"))
                bufferedImage = dip2.filter2D(image,new Filter(3,2));
            else if (e.getActionCommand().equals("Sobel1"))
                bufferedImage = dip2.filter2D(image,new Filter(3,3));
            else
                bufferedImage = dip2.filter2D(image,new Filter(3,4));
            jPanel2.removeAll();
            jPanel2.add(new ImagePane(bufferedImage));
            frame.validate();
            frame.repaint();
        }
    };
    public void plot_hist() {
        final int HEIGHT = 300;
        final int WIDTH = 350;
        int w = image.getWidth();
        int h = image.getHeight();
        int pixes[] = new int[w*h];
        int grays[] = new int[256];
        image.getRGB(0, 0, w, h, pixes, 0, w);
        for(int pix : pixes) {
            grays[0x000000ff & pix]++;
        }
        bufferedImage = new BufferedImage(400,350,BufferedImage.TYPE_INT_ARGB);
        Graphics g = bufferedImage.getGraphics();
        Color c = g.getColor();
        g.setColor(Color.red);
        g.drawLine(10, HEIGHT - 10, WIDTH - 30, HEIGHT - 10);
        g.drawLine(WIDTH - 35, HEIGHT - 15, WIDTH - 30, HEIGHT - 10);
        g.drawLine(WIDTH-35, HEIGHT - 5, WIDTH - 30, HEIGHT - 10);
        g.drawString("灰度级", WIDTH - 60, HEIGHT);
        g.drawLine(10,  HEIGHT - 10, 10, 10);
        g.drawLine(5, 15, 10, 10);
        g.drawLine(15, 15, 10, 10);
        g.drawString("像素个数", 15, 15);
        g.setColor(Color.black);
        int max = 0;
        for (int gray : grays) {
            if (max < gray)
                max = gray;
        }
        //int temp = 0;
        for(int i = 0; i < 256; i++) {
            g.drawLine(15 + i, HEIGHT - 10, 15 + i, HEIGHT - 10 - (int)((grays[i] / (float)max) * HEIGHT));
            //temp += (int)((grays[i] * (HEIGHT - 10)) / (float)(w * h));
            //g.drawLine(15 + i,HEIGHT - 10 - temp, 15 + i, HEIGHT - 10 - temp);
            if(i % 30 == 0) {
                g.drawString(i + "", 13 + i, HEIGHT);
            }
        }
        g.setColor(c);
    }
    public static void main(String[] args) {
        GUI gui = new GUI();
        gui.run();
    }
}
class ImagePane extends JPanel {    //class used to load picture
    private BufferedImage image;
    public ImagePane(BufferedImage image) {
        this.image = image;
    }
    @Override
    public void paintComponent(Graphics g) {
        g.drawImage(image,0,0,null);
    }
}
