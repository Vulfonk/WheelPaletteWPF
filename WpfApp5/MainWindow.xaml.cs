using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.CodeDom;
using System.Windows.Ink;
using System.Windows.Interop;
using Image = System.Windows.Controls.Image;

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


        public MainWindow()
        {
            InitializeComponent();
            mouseDownCursor = new Cursor("C:\\Users\\admin\\source\\repos\\WpfApp5\\WpfApp5\\circle.cur");
        }


        private void Image_MouseUp(object sender, MouseEventArgs e)
        {
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
            }
        }

        private void Image_MouseDown(object sender, MouseEventArgs e)
        {

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

        bool IsColorEquals(Color left, Color right)
        {
            var sub = left.R - right.R;
            if (Math.Abs(sub) > 5)
                return false;

            sub = left.G - right.G;
            if (Math.Abs(sub) > 5)
                return false;

            sub = left.B - right.B;
            if (Math.Abs(sub) > 5)
                return false;
            return true;
        }

        private void txt_blockR_TextChanged(object sender, RoutedEventArgs e)
        {
            /*var txtBox = (sender as TextBox);
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
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                { 
                    var isIn = i * i + j * j <= r * r;
                    if (!isIn)
                        continue;

                    var ijColor = GetPixelValue(i, j);
                    if (IsColorEquals(ijColor ,color))
                    {
                        Canvas.SetLeft(pipet, i / ratio);
                        Canvas.SetTop(pipet, j / ratio);
                        txt_blockRGB.Background = new SolidColorBrush(color);

                    }

                }
            }*/
        }
    }
}
