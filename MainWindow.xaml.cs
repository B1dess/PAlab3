using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lr3_data;

public partial class MainWindow : Window
{
    private BTree bTree;
    private const string fileName = "records.txt";
    
    public MainWindow()
    {
        InitializeComponent();
        bTree = new BTree(25);
        
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
        RefreshRecords();
    }
    
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        bTree.LoadFromFile("records.txt");
        RecordsDataGrid.ItemsSource = bTree.GetRecords();
        RefreshRecords();
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        bTree.SaveToFile("records.txt");
    }
    
    private void AddRecordButton_Click(object sender, RoutedEventArgs e)
    {
        int key;
        if (string.IsNullOrEmpty(KeyTextBox.Text))
        {
            var random = new Random();
            while(true)
            {
                key = random.Next();
                if (bTree.Search(key).Item1 == null)
                {
                    break;
                }
            }
        } 
        else
        {
            if (!int.TryParse(KeyTextBox.Text, out key))
            {
                MessageBox.Show("Ключ повинен бути цілим числом");
                return;
            }
            if (bTree.Search(key).Item1 != null)
            {
                MessageBox.Show("Ключ повинен бути унікальним");
                return;
            } 
        }
        
        string value = ValueTextBox.Text;
        bTree.Insert(key, value);
        RefreshRecords();
    }

    private void EditRecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (RecordsDataGrid.SelectedItem == null)
        {
            MessageBox.Show("Виберіть поле для редагування");
            return;
        }
        var data = (KeyValuePair<int, string>)RecordsDataGrid.SelectedItem;
        var newValue = EditValue.Text;
        bTree.Delete(data.Key);
        bTree.Insert(data.Key, newValue);
        RefreshRecords();
    }
    
    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        int n;
        if (!int.TryParse(GenerateAmount.Text, out n))
        {
            MessageBox.Show("Кількість повинна бути цілим числом");
            return;
        }

        var random = new Random();

        for (int i = 0; i < n; i++)
        {
            int key = random.Next();
        
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int len = random.Next(1, 31);
            var valueChars = new char[len];

            for (int j = 0; j < len; j++)
            {
                valueChars[j] = chars[random.Next(chars.Length)];
            }

            var value = new String(valueChars);
        
            bTree.Insert(key, value);
            RefreshRecords();
        }
    }
    
    private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
    { 
        if (RecordsDataGrid.SelectedItem == null)
        {
            MessageBox.Show("Виберіть поле для видалення");
            return;
        }
        var data = (KeyValuePair<int, string>)RecordsDataGrid.SelectedItem;
        bTree.Delete(data.Key);
        RefreshRecords();
    }
    
    private void SearchRecordButton_Click(object sender, RoutedEventArgs e)
    {
        int key;
        if (!int.TryParse(SearchKeyTextBox.Text, out key))
        {
            MessageBox.Show("Ключ повинен бути цілим числом");
            return;
        }

        var result = bTree.Search(key);
        var value = result.Item1;
        int comparisons = result.Item2;
        
        if (value == null)
        {
            MessageBox.Show("За даним ключем даних не знайдено\n" +
                            $"Кількість порівнянь: {comparisons}");
            return;
        } 
        MessageBox.Show($"Знайдені дані: {value}\n" +
                        $"Кількість порівнянь: {comparisons}");
        
        var record = new KeyValuePair<int, string>(key, value);
        RecordsDataGrid.SelectedItem = record;
        RecordsDataGrid.ScrollIntoView(RecordsDataGrid.SelectedItem);
        RecordsDataGrid.Focus();
    }
    
    private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
    {
    }
    
    private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
    {
    }

    private void RefreshRecords()
    {
        var records = bTree.GetRecords();
        RecordsNum.Text = $"Кількість записів: {records.Count}";
        RecordsDataGrid.ItemsSource = records;
    }
    
}