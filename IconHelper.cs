using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace quickapp
{
    public static class IconHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        // Получает иконку из .exe файла
        public static BitmapSource GetIconFromExe(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                using (Icon icon = Icon.ExtractAssociatedIcon(filePath))
                {
                    return ToBitmapSource(icon);
                }
            }
            catch
            {
                return null;
            }
        }

        // Преобразует Icon в BitmapSource
        private static BitmapSource ToBitmapSource(Icon icon)
        {
            using (Bitmap bitmap = icon.ToBitmap())
            {
                return bitmap.ToBitmapSource();
            }
        }

        private static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        // Сохраняет BitmapSource во временный файл PNG
        public static string SaveIconToTemp(BitmapSource icon, string appName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(icon));

            string tempPath = Path.Combine(Path.GetTempPath(), $"{appName}.png");

            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                encoder.Save(stream);
            }

            return tempPath;
        }

        // Загружает BitmapImage из файла
        public static BitmapImage LoadBitmapImage(string path)
        {
            if (!File.Exists(path))
                return null;

            var bitmap = new BitmapImage();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            bitmap.Freeze(); // Важно для работы в UI
            return bitmap;
        }
    }
}