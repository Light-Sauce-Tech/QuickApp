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
using System.Drawing;
using Image = System.Windows.Controls.Image;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace quickapp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadApps();
        }

        private const string JsonFilePath = "apps.json";
 
        public void LoadApps()
        {
            if (!File.Exists(JsonFilePath))
                return;

            string json = File.ReadAllText(JsonFilePath);
            List<AppItem> apps = JsonSerializer.Deserialize<List<AppItem>>(json);

            foreach (var app in apps)
            {
                if (File.Exists(app.AppPath))
                {
                    var container = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    Button button = new Button
                    {
                        Width = 50,
                        Height = 50,
                        Margin = new Thickness(10),
                        Style = (Style)this.FindResource("ModernButtonStyle")
                    };

                    if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
                    {
                        button.Content = new Image
                        {
                            Source = new BitmapImage(new Uri(app.IconPath, UriKind.Absolute)),
                            Width = 30,
                            Height = 30,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    else
                    {
                        button.Content = null;
                    }

                    button.Click += (s, e) =>
                    {
                        try
                        {
                            Process.Start(app.AppPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка запуска: {ex.Message}");
                        }
                    };

                    TextBlock textBlock = new TextBlock
                    {
                        Text = app.AppName,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 12
                    };

                    container.Children.Add(button);
                    container.Children.Add(textBlock);
                    StackP1.Children.Add(container);
                }
            }
        }





        private void AddApp_Click(object sender, RoutedEventArgs e)
        {
            AddAppWindow addAppWindow = new AddAppWindow();
            addAppWindow.ShowDialog();
        }

        private void FilterButton_Click_1(object sender, RoutedEventArgs e)
        {
            FilterList.Visibility = FilterList.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            StackP1.Children.Clear();
            LoadApps();
        }
    }
}