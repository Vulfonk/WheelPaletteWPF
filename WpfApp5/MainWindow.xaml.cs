using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;
using System.Drawing.Imaging;

namespace WpfApp5
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        byte[] pixelData;

        Cursor mouseDownCursor;
        System.Windows.Point pipetteLastPosition;
        bool lkmPressed;

        public MainWindow()
        {
            InitializeComponent();
            var dir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(dir)));
            var cur_path = Path.Combine(dir, "circle.cur");
            mouseDownCursor = new Cursor(cur_path);
        }


        private void Image_MouseUp(object sender, MouseEventArgs e)
        {
            if (!lkmPressed)
                return;

            var point = e.GetPosition(img);
            var r = img.ActualWidth / 2;
            var xm = point.X - r;
            var ym = point.Y - r;

            var isIn = (xm * xm + ym * ym <= r * r);

            if (isIn)
            {
                Canvas.SetLeft(pipet, point.X - pipet.Width / 2);
                Canvas.SetTop(pipet, point.Y - pipet.Height / 2);
            }
            else
            {
                Canvas.SetLeft(pipet, pipetteLastPosition.X - pipet.Width / 2);
                Canvas.SetTop(pipet, pipetteLastPosition.Y - pipet.Height / 2);
            }

            pipet.Visibility = Visibility.Visible;

            img.Cursor = (img.Parent as FrameworkElement).Cursor;
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Canvas.SetLeft(pipet, pipetteLastPosition.X - pipet.Width / 2);
                Canvas.SetTop(pipet, pipetteLastPosition.Y - pipet.Height / 2);
                pipet.Visibility = Visibility.Visible;
                img.Cursor = (img.Parent as FrameworkElement).Cursor;
            }
        }

        private void Image_MouseDown(object sender, MouseEventArgs e)
        {
            if (!lkmPressed)
                return;

            img.Cursor = mouseDownCursor;
            pipet.Visibility = Visibility.Hidden;

            var point = e.GetPosition(img);

            var pixel = GetPixelValue2(point.X, point.Y);

            txt_blockR.Text = pixel.R.ToString();
            txt_blockG.Text = pixel.G.ToString();
            txt_blockB.Text = pixel.B.ToString();

            txt_blockRGB.Background = new SolidColorBrush(pixel);
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(img);
            var r = img.ActualWidth / 2;
            var xm = point.X - r;
            var ym = point.Y - r;

            var isIn = (xm * xm + ym * ym <= r * r);

            bool lkmPressed = e.LeftButton == MouseButtonState.Pressed;

            this.lkmPressed = lkmPressed;

            if (!isIn)
            {
                if (lkmPressed)
                {
                    Canvas.SetLeft(pipet, pipetteLastPosition.X - pipet.Width / 2);
                    Canvas.SetTop(pipet, pipetteLastPosition.Y - pipet.Height / 2);
                    pipet.Visibility = Visibility.Visible;
                }

                img.Cursor = (img.Parent as FrameworkElement).Cursor;
                return;
            }

            if (!lkmPressed) return;

            if (lkmPressed && img.Cursor != mouseDownCursor)
                img.Cursor = mouseDownCursor;


            if (lkmPressed)
                pipet.Visibility = Visibility.Hidden;

            pipetteLastPosition = point;

            var pixel = GetPixelValue2(point.X, point.Y);

            txt_blockR.Text = pixel.R.ToString();
            txt_blockG.Text = pixel.G.ToString();
            txt_blockB.Text = pixel.B.ToString();

            txt_blockRGB.Background = new SolidColorBrush(pixel);
        }

        public System.Windows.Media.Color GetPixelValue2(double x, double y)
        {
            BitmapSource bitmapSource = (BitmapSource)img.Source;

            var ratio = bitmapSource.Width / img.ActualWidth;
            var int_x = (int)(x * ratio);
            var int_y = (int)(y * ratio);

            int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

            int pixelIndex = int_y * stride + int_x * (bitmapSource.Format.BitsPerPixel / 8);

            byte red = pixelData[pixelIndex + 2];
            byte green = pixelData[pixelIndex + 1];
            byte blue = pixelData[pixelIndex];
            byte alpha = pixelData[pixelIndex + 3];

            var pixelColor = System.Windows.Media.Color.FromArgb(alpha, red, green, blue);
            return pixelColor;
        }

        public System.Windows.Media.Color GetPixelValue(double x, double y)
        {
            BitmapSource bitmapSource = (BitmapSource)img.Source;

            var int_x = (int)(x);
            var int_y = (int)(y);

            int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

            int pixelIndex = int_y * stride + int_x * (bitmapSource.Format.BitsPerPixel / 8);

            byte red = pixelData[pixelIndex + 2];
            byte green = pixelData[pixelIndex + 1];
            byte blue = pixelData[pixelIndex];
            byte alpha = pixelData[pixelIndex + 3];

            var pixelColor =  System.Windows.Media.Color.FromArgb(alpha, red, green, blue);
            return pixelColor;
        }

        private void img_Loaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            BitmapSource bitmapSource = (BitmapSource)image.Source;

            int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

            pixelData = new byte[bitmapSource.PixelHeight * stride];

            Canvas.SetLeft(pipet, img.ActualWidth / 2 - pipet.Width / 2);
            Canvas.SetTop(pipet, img.ActualHeight / 2 - pipet.Height / 2);

            bitmapSource.CopyPixels(pixelData, stride, 0);
        }

        private void pipet_MouseEnter(object sender, MouseEventArgs e)
        {
            Image_MouseEnter(sender, e);
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var oldW = e.PreviousSize.Width;
            var oldH = e.PreviousSize.Height;
            var newW = e.NewSize.Width;
            var newH = e.NewSize.Height;

            var ratioW = newW / oldW;
            var ratioH = newH / oldH;
            var x = Canvas.GetLeft(pipet);
            var y = Canvas.GetTop(pipet);

            Canvas.SetLeft(pipet, x * ratioW);
            Canvas.SetTop(pipet, y * ratioH);

        }

        int ColorDiff(System.Windows.Media.Color left, System.Windows.Media.Color right)
        {
            var sub = Math.Abs(left.R - right.R);
            sub += Math.Abs(left.G - right.G);
            sub += Math.Abs(left.B - right.B);

            return sub;
        }

        private void ChangeBrightness(double brightness)
        {

            BitmapSource bitmapSource = (BitmapSource)img.Source;
            var newbitmapSource = new Bitmap((int)bitmapSource.Width, (int)bitmapSource.Height);            

            byte[] color = new byte[4];
            for(int i = 0; i < pixelData.Length; i+=4)
            {
                byte red = pixelData[i + 2];
                byte green = pixelData[i + 1];
                byte blue = pixelData[i];
                byte alpha = pixelData[i + 3];

                byte newRed = (byte)(red * brightness);
                byte newGreen = (byte)(green * brightness);
                byte newBlue = (byte)(blue * brightness);
/*
                pixelData[i + 2] = newRed;
                pixelData[i + 1] = newGreen;
                pixelData[i] = newBlue;
*/
                var pixel = System.Drawing.Color.FromArgb(255, newRed, newGreen, newBlue);

                int x = (i / 4) % (int)bitmapSource.Width;
                int y = (i / 4) / (int)bitmapSource.Width;


                newbitmapSource.SetPixel(x, y, pixel);
            }

            img.Source = Convert(newbitmapSource);


            // return the new bitmap
            //return dest;
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }


        private void txt_blockR_TextChanged(object sender, RoutedEventArgs e)
        {
            var R = byte.Parse(txt_blockR.Text);
            var G = byte.Parse(txt_blockG.Text);
            var B = byte.Parse(txt_blockB.Text);
            double brightness = (double)(R + G + B) / (3 * 255);
            ChangeBrightness(brightness);
            return;
           /* try
            {

                var txtBox = (sender as TextBox);
                var newText = txtBox.Text;
                byte R, G, B;

                if (string.IsNullOrEmpty(txt_blockR.Text))
                    return;

                if (string.IsNullOrEmpty(txt_blockG.Text))
                    return;

                if (string.IsNullOrEmpty(txt_blockB.Text))
                    return;

                if (txtBox == txt_blockR)
                {
                    R = byte.Parse(newText);
                    G = byte.Parse(txt_blockG.Text);
                    B = byte.Parse(txt_blockB.Text);
                }
                else if (txtBox == txt_blockB)
                {
                    R = byte.Parse(txt_blockR.Text);
                    G = byte.Parse(txt_blockG.Text);
                    B = byte.Parse(newText);
                }
                else if (txtBox == txt_blockG)
                {
                    R = byte.Parse(txt_blockR.Text);
                    G = byte.Parse(newText);
                    B = byte.Parse(txt_blockB.Text);
                }
                else
                {
                    throw new Exception();
                }

                Color color = Color.FromArgb(255, R, G, B);

                BitmapSource bmp = (BitmapSource)img.Source;
                var r = bmp.Height / 2;
                var ratio = bmp.Height / img.ActualHeight;
                int minDif = int.MaxValue;
                Point minDifPoint = new Point();

                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        var xm = i - r;
                        var ym = j - r;
                        var isIn = xm * xm + ym * ym <= r * r;
                        if (!isIn)
                            continue;

                        var ijColor = GetPixelValue(i, j);

                        var curDiff = ColorDiff(color, ijColor);

                        if (minDif > curDiff)
                        {
                            minDif = curDiff;
                            minDifPoint = new Point(i, j);
                        }
                    }
                }

                if (minDif != int.MaxValue)
                {
                    Canvas.SetLeft(pipet, minDifPoint.X / ratio);
                    Canvas.SetTop(pipet, minDifPoint.Y / ratio);
                    txt_blockRGB.Background = new SolidColorBrush(color);
                }
            }
            catch (Exception ex)
            {

            }*/

        }
    }
}
