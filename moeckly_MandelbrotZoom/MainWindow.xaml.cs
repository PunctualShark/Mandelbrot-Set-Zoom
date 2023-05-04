using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace moeckly_MandelbrotZoom
{
    /// <summary>
    /// Isaac Moeckly
    /// CS 480 - Assignment 4
    /// Generate Mandelbrot Set and ability to click-and-drag zoom in WPF-C#
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Windows.Point start;
        public double sX, sY, SX, SY;
        public MainWindow()
        {
            InitializeComponent();
            //Initialize sX, sY, SX and SY for scaling the zoom factor
            sX = -2.0; sY = -2.0; SX = 2.0; SY = 2.0;
            Mandelbrot();
        }

        private void Mandelbrot()
        {
            // initialize bitmap
            int width = Convert.ToInt32(img1.Width);
            int height = Convert.ToInt32(img1.Height);
            Bitmap drawingSurface = new Bitmap(width, height);

            // iterate through every pixel in image size's domain
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int color = 0;
                    int ivalue = 0;

                    // set c1 and c2 according to pixel location in the image
                    double c1 = (SX - sX) * ((Convert.ToDouble(x) - 0.0) / (Convert.ToDouble(width) - 0.0)) + sX;
                    double c2 = -(SY - sY) * ((Convert.ToDouble(y) - Convert.ToDouble(height)) / (0.0 - Convert.ToDouble(height))) - sY;

                    // initialize x0 and y0, and xold and yold for next iteration calculations
                    double xit = 0.0;
                    double yit = 0.0;
                    double xold, yold;

                    // iterate 100 times to check for divergence
                    for (int i = 0; i < 100; i++)
                    {
                        xold = xit;
                        yold = yit;

                        // if x^2 + y^2 > 30, specific julia set diverges
                        if (((xold * xold) + (yold * yold)) > 30)
                        {
                            ivalue = i;
                            break;
                        }

                        // set next iteration
                        xit = xold * xold - yold * yold + c1;
                        yit = 2 * xold * yold + c2;
                    }

                    double dcolor = Convert.ToDouble(ivalue) * 2;
                    dcolor = Convert.ToDouble(color) + dcolor;
                    color = Convert.ToInt32(dcolor);

                    if(ivalue == 0) { color = 255; }
                    Color pixelcolor = Color.FromArgb(255, color, color, color);

                    drawingSurface.SetPixel(x, y, pixelcolor);
                }
            }

            // Save bitmap to memoryStream and set imageSource to memoryStream
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();

            MemoryStream stream = new MemoryStream();
            drawingSurface.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;

            bitmap.EndInit();

            // Set window's image source to generated bitmap png
            img1.Source = bitmap;

            return;
        }

        private void Zoom(System.Windows.Point start, System.Windows.Point end)
        {
            // take x and y coords from the points where the user mouse clicked
            double startX = start.X;
            double startY = start.Y;
            double endX = end.X;
            double endY = end.Y;
            int width = Convert.ToInt32(img1.Width);
            int height = Convert.ToInt32(img1.Height);

            double buffer;

            // if start coordinate X value is larger than end coord X, swap
            // these 2 if statements make normalizing the zoom factor easy
            int status = startX.CompareTo(endX);
            if (status > 0)
            {
                buffer = startX;
                startX = endX;
                endX = buffer;
            }

            // if end coordinate Y value is larger than start coord Y, swap
            status = startY.CompareTo(endY);
            if (status < 0)
            {
                buffer = startY;
                startY = endY;
                endY = buffer;
            }

            // determine distance between X and Y values
            // this is for correctly scaling the zoom into a square; otherwise
            // Mandelbrot() would heavily skew the image if one distance was much
            // larger than the other
            double xdistance = endX - startX;
            double ydistance = startY - endY;

            status = xdistance.CompareTo(ydistance);
            if (status > 0)
            {
                startY = endY + xdistance;
            }
            else
            {
                endX = startX + ydistance;
            }

            // convert found pixel values into user coordinates
            double dstartX = (SX - sX) * (startX - sX) / (Convert.ToDouble(width) - 0.0) + sX;
            double dendX = (SX - sX) * (endX - sX) / (Convert.ToDouble(width) - 0.0) + sX;
            double dstartY = -(SY - sY) * (startY - Convert.ToDouble(height)) / (0.0 - Convert.ToDouble(height)) - sY;
            double dendY = -(SY - sY) * (endY - Convert.ToDouble(height)) / (0.0 - Convert.ToDouble(height)) - sY;

            // set new X and Y scale values accordingly
            sX = dstartX; SX = dendX; sY = dstartY; SY = dendY;

            // re-render Mandelbrot with new scale factors
            Mandelbrot();
        }

        // function to tell when mouse button has been clicked
        private void HandleButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point end;

            // if mouse button is pressed down, capture the coord point that was clicked on
            if (e.ButtonState == MouseButtonState.Pressed) { start = Mouse.GetPosition(img1); }
            // if mouse button is released, capture the coord point that was released on and run Zoom function,
            // as well as clear img2 source (img2 is for drawing zoom box)
            else if (e.ButtonState == MouseButtonState.Released)
            {
                end = Mouse.GetPosition(img1);
                Zoom(start, end);
                img2.Source = null;
            }
        }

        // function for resetting Mandelbrot if button is clicked
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sX = -2;
            sY = -2;
            SX = 2;
            SY = 2;
            Mandelbrot();
        }

        // function for resetting Mandelbrot if right mouse button is clicked
        private void HandleReset(object sender, MouseButtonEventArgs e)
        {
            sX = -2;
            sY = -2;
            SX = 2;
            SY = 2;
            Mandelbrot();
        }

        // function to track mouse while hovering AND the left button is pressed down;
        // needed to dynamically draw zoom box as mouse moves around
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                // capture coord point that was clicked on and pass it through to drawRectangle()
                System.Windows.Point here = e.GetPosition(img2);
                drawRectangle(here.X, here.Y);
            }
        }

        // function for drawing the rectangle to show user where they will be zooming in on
        private void drawRectangle(double endX, double endY)
        {
            double Xstart, Xend, Ystart, Yend;

            // initialize values to work with without messing up parent values
            Xstart = start.X;
            Xend = endX;
            Ystart = start.Y;
            Yend = endY;

            // compare xstart and xend values; useful for normalizing math
            // needed to traverse bitmap image and plot each pixel to draw rectangle
            double buffer;
            int status = Xstart.CompareTo(Xend);
            if (status > 0)
            {
                buffer = Xend;
                Xend = Xstart;
                Xstart = buffer;
            }

            // do the same for ystart and yend
            status = Ystart.CompareTo(Yend);
            if (status > 0)
            {
                buffer = Yend;
                Yend = Ystart;
                Ystart = buffer;
            }

            // initialize bitmap for img2 (where rectangle is drawn)
            int width = Convert.ToInt32(img2.Width);
            int height = Convert.ToInt32(img2.Height);
            Bitmap drawingSurface = new Bitmap(width, height);

            // iterate through pixels according to x values found earlier;
            // skip a few pixels to make rectangle appear dotted
            // two for loops for drawing top and bottom of rectangle
            for (int x = Convert.ToInt32(Xstart); x < Xend; x++)
            {
                for (int y = Convert.ToInt32(Ystart); y < Ystart+1; y++) { drawingSurface.SetPixel(x, y, Color.Red); }
                for(int y = Convert.ToInt32(Yend); y < Yend+1; y++) { drawingSurface.SetPixel(x, y, Color.Red); }
            }

            // iterate through pixels according to y values found earlier;
            // skip a few pixels to make rectangle appear dotted
            // two for loops for drawing right and left of rectangle
            for (int y = Convert.ToInt32(Ystart); y < Yend; y++)
            {
                for (int x = Convert.ToInt32(Xstart); x < Xstart + 1; x++) { drawingSurface.SetPixel(x, y, Color.Red); }
                for (int x = Convert.ToInt32(Xend); x < Xend + 1; x++) { drawingSurface.SetPixel(x, y, Color.Red); }
            }

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();

            MemoryStream stream = new MemoryStream();
            drawingSurface.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;

            bitmap.EndInit();

            // Set window's image source to generated bitmap png
            img2.Source = bitmap;

            return;
        }

        private void Quit(object sender, RoutedEventArgs e) { Close(); }
    }
}
