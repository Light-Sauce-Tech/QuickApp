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
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;


namespace quickapp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadApps();
            LoadFrequentApps();
            EnsureAutostart();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Предотвращаем закрытие — только скрываем
            e.Cancel = true;
            this.Hide();
        }

        private const string JsonFilePath = "apps.json";

        private const int MinRows = 3; 
        private int CurrentColumns = 5;

        private void EnsureAutostart()
        {
            string appName = "QuickAppLauncher"; // Имя в реестре
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key != null)
                {
                    var existingValue = key.GetValue(appName) as string;
                    if (existingValue != appPath)
                    {
                        key.SetValue(appName, appPath);
                    }
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGridColumns();
        }
        public void LoadApps()
        {
            StackP1.Children.Clear();

            if (!File.Exists(JsonFilePath))
                return;

            string json = File.ReadAllText(JsonFilePath);
            List<AppItem> apps = JsonSerializer.Deserialize<List<AppItem>>(json);

            foreach (var app in apps)
            {
                if (!app.IsValid()) continue;

                var container = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button button = new Button
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5),
                    Style = (Style)this.FindResource("ModernButtonStyle"),
                    Tag = app
                };

                if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
                {
                    button.Content = new Image
                    {
                        Source = new BitmapImage(new Uri(app.IconPath)),
                        Width = 30,
                        Height = 30
                    };
                }

                button.Click += (s, e) =>
                {
                    try
                    {
                        app.LaunchCount++;
                        UpdateAppInJson(app);
                        Process.Start(app.AppPath);
                        LoadFrequentApps();
                        UpdateGridColumns();
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

            UpdateGridColumns();
        }
        private void SaveAppList(List<AppItem> apps)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(apps, options);
            File.WriteAllText(JsonFilePath, json);
        }

        private void UpdateAppInJson(AppItem updatedApp)
        {
            var apps = AddAppWindow.GetAppList();
            var existing = apps.FirstOrDefault(a => a.AppPath == updatedApp.AppPath);
            if (existing != null)
            {
                existing.LaunchCount = updatedApp.LaunchCount;
                SaveAppList(apps);
            }
        }

        private void LoadFrequentApps()
        {
            var apps = AddAppWindow.GetAppList()
                .Where(app => app.IsValid())
                .OrderByDescending(app => app.LaunchCount)
                .Take(3)
                .ToList();

            Button[] buttons = { oftexe1, oftexe2, oftexe3 };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < apps.Count)
                {
                    var app = apps[i];

                    if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
                    {
                        buttons[i].Content = new Image
                        {
                            Source = new BitmapImage(new Uri(app.IconPath)),
                            Width = 30,
                            Height = 30
                        };
                    }
                    else
                    {
                        buttons[i].Content = null;
                    }

                    ToolTipService.SetToolTip(buttons[i], app.AppName);

                    buttons[i].Tag = app;
                    buttons[i].Click -= FrequentApp_Click;
                    buttons[i].Click += FrequentApp_Click;
                }
                else
                {
                    buttons[i].Content = null;
                    buttons[i].Tag = null;
                    buttons[i].Click -= FrequentApp_Click;
                    ToolTipService.SetToolTip(buttons[i], null);
                }
            }
        }
        private void FrequentApp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AppItem app)
            {
                try
                {
                    app.LaunchCount++;
                    UpdateAppInJson(app);
                    Process.Start(app.AppPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка запуска: {ex.Message}");
                }
            }
        }

        private void UpdateGridColumns()
        {
            const double MinColumnWidth = 100;
            double availableWidth = StackP1.ActualWidth;

            if (availableWidth <= 0) return;

            int columns = Math.Max(1, (int)(availableWidth / MinColumnWidth));
            CurrentColumns = columns;

            StackP1.Columns = columns;
            if (StackP1.Children.Count > 0)
            {
                var items = (from sp in StackP1.Children.OfType<StackPanel>()
                             let btn = sp.Children[0] as Button
                             let app = btn?.Tag as AppItem
                             where app != null
                             select app).ToList();

                ShowAppsInGrid(items);
            }
        }

        private void AppsWrapPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as FrameworkElement;
            if (clickedElement == null) return;

            FrameworkElement current = clickedElement;
            while (current != null && !(current is Button))
            {
                current = VisualTreeHelper.GetParent(current) as FrameworkElement;
            }

            if (current is Button button && button.Tag is AppItem appToRemove)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить приложение:\n\"{appToRemove.AppName}\"?",
                    "Подтвердите удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var apps = new List<AppItem>();
                    if (File.Exists(JsonFilePath))
                    {
                        var json = File.ReadAllText(JsonFilePath);
                        apps = JsonSerializer.Deserialize<List<AppItem>>(json);
                    }

                    apps.RemoveAll(a => a.AppPath == appToRemove.AppPath);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(JsonFilePath, JsonSerializer.Serialize(apps, options));

                    LoadApps();

                    DelApp.Content = "Удалить приложение";
                    DelApp.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 248, 255));

                    StackP1.PreviewMouseLeftButtonDown -= AppsWrapPanel_PreviewMouseLeftButtonDown;
                }

                e.Handled = true;
            }
        }

        private void DeleteApp_Click(object sender, RoutedEventArgs e)
        {
            if (DelApp.Content.ToString() == "Удалить приложение")
            {
                DelApp.Content = "Отмена";
                DelApp.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 80, 80));

                StackP1.PreviewMouseLeftButtonDown += AppsWrapPanel_PreviewMouseLeftButtonDown;
            }
            else
            {
                DelApp.Content = "Удалить приложение";
                DelApp.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 248, 255));
                StackP1.PreviewMouseLeftButtonDown -= AppsWrapPanel_PreviewMouseLeftButtonDown;
            }
        }

        private void AddApp_Click(object sender, RoutedEventArgs e)
        {
            AddAppWindow addAppWindow = new AddAppWindow();
            bool? result = addAppWindow.ShowDialog();

            if (result == true)
            {
                StackP1.Children.Clear();
                LoadApps();
            }
        }

        private void FilterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterList.SelectedItem is ListBoxItem selectedItem)
            {
                string sortBy = selectedItem.Content.ToString();

                var allApps = AddAppWindow.GetAppList()
                    .Where(app => app.IsValid())
                    .ToList();

                IEnumerable<AppItem> sortedApps = sortBy switch
                {
                    "По имени" => allApps.OrderBy(app => app.AppName),

                    "По дате" => allApps.OrderBy(app =>
                    {
                        try
                        {
                            return File.GetLastWriteTime(app.AppPath);
                        }
                        catch
                        {
                            return DateTime.MinValue;
                        }
                    }),

                    "По частоте запуска" => allApps.OrderByDescending(app => app.LaunchCount),

                    _ => allApps
                };

                ShowAppsInGrid(sortedApps.ToList());

                FilterList.Visibility = Visibility.Collapsed;
                FilterList.SelectedItem = null;
            }
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";

            LoadApps();

            FilterList.Visibility = Visibility.Collapsed;
            FilterList.SelectedItem = null;
        }
        private void ShowAppsInGrid(List<AppItem> apps)
        {

            StackP1.Children.Clear();

            foreach (var app in apps)
            {
                var container = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button button = new Button
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5),
                    Style = (Style)this.FindResource("ModernButtonStyle"),
                    Tag = app
                };

                if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
                {
                    button.Content = new Image
                    {
                        Source = new BitmapImage(new Uri(app.IconPath)),
                        Width = 30,
                        Height = 30
                    };
                }

                button.Click += (s, e) =>
                {
                    try
                    {
                        app.LaunchCount++;
                        UpdateAppInJson(app);
                        Process.Start(app.AppPath);
                        LoadFrequentApps();
                        UpdateGridColumns();
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

            int totalCells = CurrentColumns * MinRows;
            int currentCount = StackP1.Children.Count;

            for (int i = currentCount; i < totalCells; i++)
            {
                StackP1.Children.Add(new Border());
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchTextBox.Text?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                LoadApps();
                return;
            }

            var allApps = AddAppWindow.GetAppList()
                .Where(app => app.IsValid())
                .ToList();

            var filteredApps = allApps
                .Where(app => app.AppName.ToLower().Contains(query))
                .ToList();

            ShowAppsInGrid(filteredApps);
        }

        private void FilterButton_Click_1(object sender, RoutedEventArgs e)
        {
            FilterList.Visibility = FilterList.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        }
    }
}