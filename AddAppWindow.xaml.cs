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

            if (File.Exists(path))
            {
                string name = Path.GetFileNameWithoutExtension(path);

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
            else
            {
                MessageBox.Show("Файл не найден.");
            }
            
        }

        private void SaveAppToJson(AppItem app)
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