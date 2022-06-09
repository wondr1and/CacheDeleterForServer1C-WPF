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


namespace WPF_CacheDeleter1C
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string excPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\CacheDeleter\user-exception.txt";
        
        // Класс для датагрида
        public class UserGrid
        {
            public DateTime uDC { get; set; }
            public string uName { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        
        //Обновление листбокса с исключениями
        public void paintUsrExc(string[] UsrArr)
        {
            try
            {
                IskListBox.Items.Clear();
                if (UsrArr[0] == "Brand new file") { }
                else
                {
                    for (int i = 0; i < UsrArr.Length; i++) IskListBox.Items.Add(UsrArr[i]);
                    File.WriteAllLines(excPath, UsrArr);
                }
            }
            catch
            {
                File.Delete(excPath);
                File.WriteAllText(excPath, "Brand new file");
            }
        }

        //Обновление листбокса с пользователями
        private void btnCacheRefresh_Click(object sender, RoutedEventArgs e)
        {
            UserListBox.Items.Clear();
            string[] usrDirs = Directory.GetDirectories(@"C:\Users");
            for (int i = 0; i < usrDirs.Length; i++)
            {
                if ((usrDirs[i] == @"C:\Users\Default User") || (usrDirs[i] == @"C:\Users\Public") ||
                    (usrDirs[i] == @"C:\Users\Default") || (usrDirs[i] == @"C:\Users\All Users") ||
                    (usrDirs[i] == @"C:\Users\Все пользователи")) { }
                else UserListBox.Items.Add(usrDirs[i]);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //Проверяю есть ли папка программы в Роуминге, если нет то создаю
            if (Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\CacheDeleter\")) { }
            else Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\CacheDeleter\");

            IskListBox.Items.Clear();
            if (File.Exists(excPath))
            {              
                string[] UEList = File.ReadAllLines(excPath);
                paintUsrExc(UEList);
            }
            else File.WriteAllText(excPath, "Brand new file");
        }

        //Добавление пользователя в исключения
        private void btnCacheAdd_Click(object sender, RoutedEventArgs e)
        {
            if (UserListBox.SelectedItem != null)
            {
                string[] UEList = File.ReadAllLines(excPath);
                if (UEList[0] == "Brand new file") UEList[0] = UserListBox.SelectedItem.ToString();
                else
                {
                    bool isAdd = true;
                    for (int k = 0; k < UEList.Length; k++)  // Проверяем нет ли выбраного пользователя в исключениях
                        if (UEList[k] == UserListBox.SelectedItem.ToString()) isAdd = false;
                    if (isAdd)
                    {
                        Array.Resize(ref UEList, UEList.Length + 1);  // Добавляем пустой элемент в массив, чтобы не словить аут оф рендж
                        UEList[UEList.Length - 1] = UserListBox.SelectedItem.ToString();
                    }
                }

                paintUsrExc(UEList);
            }
        }

        //Удаление пользователя из исключений
        private void btnCacheDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IskListBox.SelectedItem != null)
            {
                string[] UEList = File.ReadAllLines(excPath);
                int DelIndex = -1;
                for (int i = 0; i < UEList.Length; i++)
                    if (UEList[i] == IskListBox.SelectedItem.ToString()) DelIndex = i;

                if (DelIndex > -1)
                {
                    for (int j = DelIndex; j < UEList.Length - 1; j++)
                    {
                        UEList[j] = UEList[j + 1];
                    }
                    Array.Resize(ref UEList, UEList.Length - 1);
                }
                File.WriteAllLines(excPath, UEList);

                if (IskListBox.Items.Count == 0) File.WriteAllText(excPath, "Brand new file");
                UEList = File.ReadAllLines(excPath);
                paintUsrExc(UEList);
            }
        }

        //Начало очистки кеша 1с
        private void btnCacheStart_Click(object sender, RoutedEventArgs e)
        {
            string[] usrDirs = Directory.GetDirectories(@"C:\Users");
            string[] UEList = File.ReadAllLines(excPath);
            int UselessCounter = 0;

            for (int i = 0; i < UEList.Length; i++)                       //  Сортировка исключений
                for (int j = 0; j < usrDirs.Length; j++)                    
                    if (usrDirs[j] == UEList[i])                            
                        for (int k = j; k < usrDirs.Length - 1; k++)        
                            usrDirs[k] = usrDirs[k + 1];                    
            Array.Resize(ref usrDirs, usrDirs.Length - UEList.Length);    // Удаляем повторяющиеся элементы массива
            CachePB.Visibility = Visibility.Visible;
            CachePB.Value = 0;

            try
            {
                for (int i = 0; i < usrDirs.Length; i++)
                {
                    // LOCAL
                    if (Directory.Exists(usrDirs[i] + @"\AppData\Local\1C\1cv8"))
                    {
                        string[] CacheDir = Directory.GetDirectories(usrDirs[i] + @"\AppData\Local\1C\1cv8");
                        try
                        {
                            UselessCounter++;
                            for (int j = 0; j < CacheDir.Length; j++)
                                Directory.Delete(CacheDir[j], true);
                        } catch { /* Если возникло это исключение, то пользователь сейчас в 1С? */ }
                    }

                    //ROAMING
                    if (Directory.Exists(usrDirs[i] + @"\AppData\Roaming\1C\1cv8"))
                    {
                        string[] CacheDir = Directory.GetDirectories(usrDirs[i] + @"\AppData\Roaming\1C\1cv8");
                        try
                        {
                            UselessCounter++;
                            for (int j = 0; j < CacheDir.Length; j++)
                            {
                                if (CacheDir[j] == usrDirs[i] + @"\AppData\Roaming\1C\1cv8\ExtCompT") { /* Пропускаем папку с драйвером для сканера */ }
                                else Directory.Delete(CacheDir[j], true);
                            }
                        } catch { /* Если возникло это исключение, то пользователь сейчас в 1С? */ }
                    }
                    CachePB.Value += (100 / (usrDirs.Length - 5));
                }            
            }
            catch { MessageBox.Show("Ошибка! Возможно, мне не хватает прав."); }
            CachePB.Visibility = Visibility.Hidden;
            MessageBox.Show("Очистка завершена: " + (UselessCounter/2) + " пользователей очищено.");
        }

        //Удаление слова "Поиск..."
        private void FinderBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FinderBox.Text = "";
        }

        
        //Строка для поиска
        private void FinderBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (UserListBox.Items.Count > 0)
            {
                string[] UsrFnd = Directory.GetDirectories(@"C:\Users");
                UserListBox.Items.Clear();

                if (FinderBox.Text.Length > 0)
                {
                    foreach (string finder in UsrFnd)
                    {
                        bool bFinder = finder.Contains(FinderBox.Text);
                        if (bFinder)
                        {
                            int index = finder.IndexOf(FinderBox.Text);
                            string rezult = finder.Substring(0);
                            UserListBox.Items.Add(rezult);
                        }
                    }
                }
                else
                {
                    btnCacheRefresh_Click(null, null);
                }
                              
            }
        }

        //Обновление датагрида
        private void btnFileRefresh_Click(object sender, RoutedEventArgs e)
        {
            UserTable.Items.Clear ();
            string[] usrDirs = Directory.GetDirectories(@"C:\Users");
            for (int i = 0; i < usrDirs.Length; i++)
            {
                if ((usrDirs[i] == @"C:\Users\Default User") || (usrDirs[i] == @"C:\Users\Public") ||
                    (usrDirs[i] == @"C:\Users\Default") || (usrDirs[i] == @"C:\Users\All Users") ||
                    (usrDirs[i] == @"C:\Users\Все пользователи")) { }
                else
                {
                    UserGrid tempUser = new UserGrid();
                    tempUser.uName = usrDirs[i];
                    tempUser.uDC = Directory.GetLastWriteTime(usrDirs[i]);
                    UserTable.Items.Add(tempUser);
                }
            }
        }

        //Полное удаление пользователя
        public void FullDelete(string UserPath) {
            try
            {
                Directory.Delete(UserPath, true);
                File.Delete(UserPath);
                btnFileRefresh_Click(null, null);
            } catch { /*_*/ }
        }
        
        //Удаление кеша 1с у выбранного
        public void CacheDelete(string UserPath) {
            try
            {
                // LOCAL
                if (Directory.Exists(UserPath + @"\AppData\Local\1C\1cv8"))
                {
                    string[] CacheDir = Directory.GetDirectories(UserPath + @"\AppData\Local\1C\1cv8");
                    try
                    {
                        for (int j = 0; j < CacheDir.Length; j++)
                            Directory.Delete(CacheDir[j], true);
                    }
                    catch { /* Если возникло это исключение, то пользователь сейчас в 1С? */ }
                }

                //ROAMING
                if (Directory.Exists(UserPath + @"\AppData\Roaming\1C\1cv8"))
                {
                    string[] CacheDir = Directory.GetDirectories(UserPath + @"\AppData\Roaming\1C\1cv8");
                    try
                    {
                        for (int j = 0; j < CacheDir.Length; j++)
                        {
                            if (CacheDir[j] == UserPath + @"\AppData\Roaming\1C\1cv8\ExtCompT") { /* Пропускаем папку с драйвером для сканера */ }
                            else Directory.Delete(CacheDir[j], true);
                        }
                    }
                    catch { /* Если возникло это исключение, то пользователь сейчас в 1С? */ }
                }
            } catch { /*_*/ }
        }
        
        //Удаление кеша браузера
        public void BrowserDelete(string UserPath) {
            try
            {
                // IE
                if (Directory.Exists(UserPath + @"\AppData\Local\Microsoft\Windows\INetCache\"))
                {
                    Directory.Delete(UserPath + @"\AppData\Local\Microsoft\Windows\INetCache\", true);
                    File.Delete(UserPath + @"\AppData\Local\Microsoft\Windows\INetCache\");
                }
                if (Directory.Exists(UserPath + @"\AppData\Local\Microsoft\Windows\Temporary Internet Files\"))
                {
                    Directory.Delete(UserPath + @"\AppData\Local\Microsoft\Windows\Temporary Internet Files\", true);
                    File.Delete(UserPath + @"\AppData\Local\Microsoft\Windows\Temporary Internet Files\");
                }

                // CHROME
                if (Directory.Exists(UserPath + @"\AppData\Local\Google\Chrome\User Data\Default\Cache"))
                {
                    Directory.Delete(UserPath + @"\AppData\Local\Google\Chrome\User Data\Default\Cache", true);
                    File.Delete(UserPath + @"\AppData\Local\Google\Chrome\User Data\Default\Cache");
                }
                if (Directory.Exists(UserPath + @"\AppData\Local\Google\Chrome\User Data\Profile 1\Cache"))
                {
                    Directory.Delete(UserPath + @"\AppData\Local\Google\Chrome\User Data\Profile 1\Cache", true);
                    File.Delete(UserPath + @"\AppData\Local\Google\Chrome\User Data\Profile 1\Cache");
                }

                // YANDEX
                if (Directory.Exists(UserPath + @"\AppData\Local\Yandex\YandexBrowser\User Data\Default\Cache"))
                {
                    Directory.Delete(UserPath + @"\AppData\Local\Yandex\YandexBrowser\User Data\Default\Cache", true);
                    File.Delete(UserPath + @"\AppData\Local\Yandex\YandexBrowser\User Data\Default\Cache");
                }

                //OPERA
                if (Directory.Exists(UserPath + @"\AppData\Local\Opera Software\Opera Stable\Cache"))
                {
                    Directory.Delete(UserPath + @"\AppData\Local\Opera Software\Opera Stable\Cache", true);
                    File.Delete(UserPath + @"\AppData\Local\Opera Software\Opera Stable\Cache");
                }
            } catch { MessageBox.Show("У меня нет для этого прав."); }
        }
        
        // Удаление выбранного каталога
        public void DocumentsDelete(string UserPath) {
            try
            {
                if (Directory.Exists(UserPath))
                {
                    Directory.Delete(UserPath, true);
                    File.Delete(UserPath);
                }
            } catch { /*_*/ }
        }

        private void btnFileStart_Click(object sender, RoutedEventArgs e)
        {
            if (UserTable.SelectedItem != null)
            {
                UserGrid tempuser = new UserGrid();
                tempuser = UserTable.SelectedItem as UserGrid;

                if (CheckAllDelete.IsChecked == true)
                {
                    MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить ВСЕ внутри каталога пользователя?", "Удаление пользователя", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            MessageBoxResult result1 = MessageBox.Show("Точно-точно?", "Удаление пользователя", MessageBoxButton.YesNo);
                            switch (result1)
                            {
                                case MessageBoxResult.Yes:
                                    FullDelete(tempuser.uName);
                                    break;
                                case MessageBoxResult.No:
                                    break;
                            }
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                }
                else
                {
                    if (CheckCache.IsChecked == true) CacheDelete(tempuser.uName);
                    if (CheckBrowser.IsChecked == true) BrowserDelete(tempuser.uName);
                    if (CheckDesktop.IsChecked == true) DocumentsDelete(tempuser.uName + @"\Desktop");
                    if (CheckDownloads.IsChecked == true) DocumentsDelete(tempuser.uName + @"\Downloads");
                    if (CheckDocuments.IsChecked == true)
                    {
                        DocumentsDelete(tempuser.uName + @"\Documents");
                        DocumentsDelete(tempuser.uName + @"\Pictures");
                        DocumentsDelete(tempuser.uName + @"\Videos");
                        DocumentsDelete(tempuser.uName + @"\Music");
                    }
                }

                MessageBox.Show("Удаление завершено.");
            }
            else MessageBox.Show("Вы не выбрали пользователя.");
            
        }

        private void FinderBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FinderBox.Text.Length == 0) btnCacheRefresh_Click(null, null); //я дебил
        }
    }
}
