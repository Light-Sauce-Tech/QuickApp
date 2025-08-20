using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace quickapp
{
    public partial class AddAppWindow : Window
    {
        private const string JsonFilePath = "apps.json";

        public AppItem NewApp { get; private set; }

        public AddAppWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Исполняемые файлы (*.exe)|*.exe";
            if (dlg.ShowDialog() == true)
            {
                PathTextBox.Text = dlg.FileName;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string path = PathTextBox.Text;

            if (!File.Exists(path))
            {
                MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверяем, уже ли добавлено это приложение
            var existingApps = GetAppList();
            if (existingApps.Any(app => app.AppPath == path))
            {
                MessageBox.Show("Это приложение уже добавлено.", "Дубликат", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string name = Path.GetFileNameWithoutExtension(path);

            try
            {
                var icon = IconHelper.GetIconFromExe(path);
                string iconPath = IconHelper.SaveIconToTemp(icon, name);

                var appItem = new AppItem
                {
                    AppName = name,
                    AppPath = path,
                    IconPath = iconPath
                };

                SaveAppToJson(appItem);

                NewApp = appItem;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось извлечь иконку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static List<AppItem> GetAppList()
        {
            if (!File.Exists("apps.json"))
                return new List<AppItem>();

            string json = File.ReadAllText("apps.json");
            return JsonSerializer.Deserialize<List<AppItem>>(json) ?? new List<AppItem>();
        }

        public static void SaveAppToJson(AppItem app)
        {
            List<AppItem> apps = new List<AppItem>();

            if (File.Exists(JsonFilePath))
            {
                string json = File.ReadAllText(JsonFilePath);
                apps = JsonSerializer.Deserialize<List<AppItem>>(json);
            }

            apps.Add(app);

            string updatedJson = JsonSerializer.Serialize(apps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonFilePath, updatedJson);
        }
    }
}