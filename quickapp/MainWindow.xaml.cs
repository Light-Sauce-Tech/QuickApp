using System;
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
using System.Diagnostics;

namespace quickapp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Create();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterList.Visibility = FilterList.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        }

        //приложухи
        private void Create()
        {
            //анидеск
            string path = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe";

            if (File.Exists(path))
            {
                Button button = new Button
                {
                    Content = "AnyDesk",
                    Width = 50,
                    Height = 50,
                    Style = (Style)this.FindResource("ModernButtonStyle"),
                };

                
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(@"C:\Users\0\Desktop\icons8-anydesk-48.png")),
                    Width = 30,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                button.Content = image;

                button.Click += (s, ev) =>
                {
                    try
                    {
                        Process.Start(path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка запуска: {ex.Message}");
                    }
                };
                yes.Children.Add(button);
            }
        }
    }
}