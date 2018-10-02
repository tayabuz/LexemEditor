using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Notifications;

namespace LexemEditor
{
    public sealed partial class MainPage : Page
    {
        public LexemesFilesHandler CurrentSession = new LexemesFilesHandler();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OpenFolderAndGetLexemes_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            await StartSaveLexemes();
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                await Task.Run(() => StartNewSessionForGetLexemes(folder.Path));
                ProgressRingLoadingLexemes.IsActive = true;
                ProgressRingLoadingLexemes.Visibility = Visibility.Visible;
                await Task.Delay(500);
                InitializeListView();
                ProgressRingLoadingLexemes.IsActive = false;
                ProgressRingLoadingLexemes.Visibility = Visibility.Collapsed;
            }
        }

        private void StartNewSessionForGetLexemes(string path)
        {
            CurrentSession = new LexemesFilesHandler(path);
        }
        private async void SaveLexemes_Click(object sender, RoutedEventArgs e)
        {
           await StartSaveLexemes();
        }

        private async Task StartSaveLexemes()
        {
            if (!CurrentSession.IsStorageOfLexemesNullOrEmpty())
            {
                SaveLexemes.IsEnabled = false;
                AddLexemesFromFolder.IsEnabled = false;
                await Task.Run(() => CurrentSession.SaveLexemesToFiles());
                SaveLexemes.IsEnabled = true;
                AddLexemesFromFolder.IsEnabled = true;
                CreateAndShowSaveNotification();
            }
        }

        private void CreateAndShowSaveNotification()
        {
            var xmlToastTemplate = "<toast launch=\"app-defined-string\">" +
                                   "<visual>" +
                                   "<binding template =\"ToastGeneric\">" +
                                   "<text>Изменения сохранены</text>" +
                                   "</binding>" +
                                   "</visual>" +
                                   "</toast>";
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);
            var toastNotification = new ToastNotification(xmlDocument);
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        private void InitializeListView()
        {
            ListViewLexemesViewer.Items.Clear();
            int index = 1;
            Grid headerGrid = new Grid();
            ColumnDefinition EmptyColumn = new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) };
            headerGrid.ColumnDefinitions.Add(EmptyColumn);
            foreach (var language in CurrentSession.Languages)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) };
                headerGrid.ColumnDefinitions.Add(columnDefinition);
                TextBlock textBlock = new TextBlock { Text = language, TextAlignment = TextAlignment.Center };
                headerGrid.Children.Add(textBlock);
                Grid.SetColumn(textBlock, CurrentSession.Languages.IndexOf(language) + 1);
            }
            ListViewLexemesViewer.Items.Add(headerGrid);
            foreach (var lexeme in CurrentSession.Lexemes)
            {
                Grid grid = new Grid() { HorizontalAlignment = HorizontalAlignment.Stretch };
                ColumnDefinition col = new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) };
                grid.ColumnDefinitions.Add(col);
                foreach (var language in CurrentSession.Languages)
                {
                    ColumnDefinition column = new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) };
                    grid.ColumnDefinitions.Add(column);
                }
                TextBlock textBlock = new TextBlock {Text = lexeme.Key};
                grid.Children.Add(textBlock);
                Grid.SetColumn(textBlock, 0);
                foreach (var item in lexeme.Value)
                {
                    Binding binding = new Binding
                    {
                        Source = item,
                        Path = new PropertyPath("Value"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    TextBox textBox = new TextBox { TextWrapping = TextWrapping.Wrap };
                    BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);
                    grid.Children.Add(textBox);
                    Grid.SetColumn(textBox, CurrentSession.Languages.IndexOf(item.Language) + 1);
                }
                index++;
                ListViewLexemesViewer.Items.Add(grid);
            }
        }
    }
}
