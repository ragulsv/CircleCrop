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
using System.IO;

namespace CircleCrop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Stream processingBitmap = null;
        Border border = new Border();
        public MainWindow()
        {
            InitializeComponent();
            BitmapImage bitmapImage;
            bitmapImage = this.image.Source as BitmapImage;
            if (bitmapImage == null)
            {
                bitmapImage = new BitmapImage(new Uri(this.image.Source.ToString()));
            }
            if (bitmapImage != null && (bitmapImage.UriSource != null || bitmapImage.StreamSource != null))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                MemoryStream stream = new MemoryStream();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);
                processingBitmap = stream;
            }
        }
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            //image.Clip = new EllipseGeometry(new Point(250, 250), 100, 100);
            double width = 0, height = 0;
            if (processingBitmap != null)
            {
                processingBitmap.Seek(0, 0);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = processingBitmap;
                image.EndInit();
                height = image.PixelHeight;
                width = image.PixelWidth;
            }

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = image.Source;
            var pixelWidth = image.ActualWidth;
            var pixelHeight = image.ActualHeight;
            border.Background = new SolidColorBrush(Colors.White);
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = brush;
            border.Child = ellipse;
            width = imageGrid.Width;
            height = imageGrid.Height;
            var min = Math.Min(width, height);
            ellipse.Clip = new EllipseGeometry(new Point(width / 2, height / 2), min / 2, min / 2);
            this.imageGrid.Children.Remove(image);
            this.imageGrid.Children.Add(border);
            //SetCircularStream(border, (int)width, (int)height, croppedRect);
            //this.grid.Children.Remove(border);
        }

        private void SetCircularStream(FrameworkElement element, int pixelWidth, int pixelHeight, Rect croppedRect)
        {
            double width = pixelWidth, height = pixelHeight;
            if (processingBitmap != null)
            {
                processingBitmap.Seek(0, 0);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = processingBitmap;
                image.EndInit();
                height = image.PixelHeight;
                width = image.PixelWidth;
            }

            processingBitmap = GetRenderTargetStream(element, (int)width, (int)height);

            if (processingBitmap != null)
                processingBitmap.Seek(0, 0);

            Int32Rect sourceRect = new Int32Rect((int)croppedRect.X, (int)croppedRect.Y, (int)croppedRect.Width, (int)croppedRect.Height);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = processingBitmap;
            bitmapImage.EndInit();

            CroppedBitmap croppedBitmap = new CroppedBitmap(bitmapImage, sourceRect);
            processingBitmap = ConvertBitmapSourceToStream(croppedBitmap);
            processingBitmap.Seek(0, 0);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = processingBitmap;
            bitmap.EndInit();
            image.Source = bitmap;
        }

        private static Stream ConvertBitmapSourceToStream(BitmapSource bitmapSource)
        {
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            Stream convertedStream = new MemoryStream();
            encoder.Save(convertedStream);
            convertedStream.Seek(0, 0);
            return convertedStream;
        }

        private static Stream GetRenderTargetStream(FrameworkElement element, int actualWidth, int actualHeight)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(actualWidth, actualHeight, 96, 96,
                PixelFormats.Pbgra32);

            var visualBrush = new VisualBrush
            {
                Visual = element,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center,
                Stretch = Stretch.UniformToFill,
            };

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), new Size(actualWidth, actualHeight)));
            }

            renderTargetBitmap.Render(drawingvisual);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            Stream imageStream = new MemoryStream();
            encoder.Save(imageStream);
            imageStream.Seek(0, 0);
            return imageStream;
        }

        private void setImage_Click(object sender, RoutedEventArgs e)
        {
            double width = 0, height = 0;
            width = imageGrid.Width;
            height = imageGrid.Height;
            Rect croppedRect = new Rect(0, 0, 1000, 750);
            SetCircularStream(border, (int)width, (int)height, croppedRect);
            this.imageGrid.Children.Add(image);
            this.imageGrid.Children.Remove(border);
        }
    }
}
