using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace WpfApp5
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        byte[] pixelData;

        Cursor mouseDownCursor;
        Point pippetLastPosition;
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
                Canvas.SetLeft(pipet, pippetLastPosition.X - pipet.Width / 2);
                Canvas.SetTop(pipet, pippetLastPosition.Y - pipet.Height / 2);
            }

            pipet.Visibility = Visibility.Visible;

            img.Cursor = (img.Parent as FrameworkElement).Cursor;
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Canvas.SetLeft(pipet, pippetLastPosition.X - pipet.Width / 2);
                Canvas.SetTop(pipet, pippetLastPosition.Y - pipet.Height / 2);
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
                    Canvas.SetLeft(pipet, pippetLastPosition.X - pipet.Width / 2);
                    Canvas.SetTop(pipet, pippetLastPosition.Y - pipet.Height / 2);
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

            pippetLastPosition = point;

            var pixel = GetPixelValue2(point.X, point.Y);

            txt_blockR.Text = pixel.R.ToString();
            txt_blockG.Text = pixel.G.ToString();
            txt_blockB.Text = pixel.B.ToString();

            txt_blockRGB.Background = new SolidColorBrush(pixel);
        }

        public Color GetPixelValue2(double x, double y)
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

            Color pixelColor = Color.FromArgb(alpha, red, green, blue);
            return pixelColor;
        }

        public Color GetPixelValue(double x, double y)
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

            Color pixelColor = Color.FromArgb(alpha, red, green, blue);
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

        int ColorDiff(Color left, Color right)
        {
            var sub = Math.Abs(left.R - right.R);
            sub += Math.Abs(left.G - right.G);
            sub += Math.Abs(left.B - right.B);

            return sub;
        }

        private void txt_blockR_TextChanged(object sender, RoutedEventArgs e)
        {

            return;
            try
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

            }

        }
    }
}
