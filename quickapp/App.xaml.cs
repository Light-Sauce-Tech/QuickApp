using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;

namespace quickapp
{
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        // Вызывается при запуске приложения
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                notifyIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                CreateContextMenu();
                MainWindow = new MainWindow();
                MainWindow.Hide();
                MainWindow.Closing += MainWindow_Closing;
                RegisterHotKey();
            }
            catch (Exception ex)
            {
                // Показываем ошибку, чтобы понять, что пошло не так
                MessageBox.Show($"Ошибка при запуске:\n{ex.Message}\n\n{ex.StackTrace}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        // Вызывается при завершении приложения
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose(); // Освобождаем ресурсы
            base.OnExit(e);
        }

        private void CreateContextMenu()
        {
            var openItem = new System.Windows.Controls.MenuItem
            {
                Header = "Открыть"
            };
            openItem.Click += (s, e) => ShowMainWindow();

            var exitItem = new System.Windows.Controls.MenuItem
            {
                Header = "Выход"
            };
            exitItem.Click += (s, e) =>
            {
                notifyIcon.Dispose();
                Shutdown(); // Завершаем приложение
            };

            notifyIcon.ContextMenu = new System.Windows.Controls.ContextMenu();
            notifyIcon.ContextMenu.Items.Add(openItem);
            notifyIcon.ContextMenu.Items.Add(exitItem);
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                MainWindow.Activate();
                MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            MainWindow.Hide();
        }


        private const int HOTKEY_ID = 1;
        private const uint MOD_CTRL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_SPACE = 0x20;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(MainWindow);
            bool registered = RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL | MOD_SHIFT, VK_SPACE);

            if (!registered)
            {
                MessageBox.Show("Не удалось зарегистрировать горячую клавишу.\nВозможно, она уже используется.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
            base.OnSessionEnding(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            ComponentDispatcher.ThreadFilterMessage += OnThreadMessage;
            base.OnDeactivated(e);
        }

        private void OnThreadMessage(ref MSG msg, ref bool handled)
        {
            if (!handled && msg.message == 0x0312 && msg.wParam.ToInt32() == HOTKEY_ID)
            {
                ShowMainWindow();
                handled = true;
            }
        }

        // Обработчик двойного клика по иконке в трее
        private void OnTrayIconDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
    }
}