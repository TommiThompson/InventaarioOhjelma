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
using System.Collections.ObjectModel;
using System.IO;
using Path = System.IO.Path;
using Microsoft.Win32;


namespace InventoryManagement

{
    public partial class MainWindow : Window
    {
        ObservableCollection<Nimike> inventory = new ObservableCollection<Nimike>();




        public MainWindow()
        {
            InitializeComponent();
            Inventaario_Lista.ItemsSource = inventory;

        }


        private void BtnLisaa_Nimike(object sender, RoutedEventArgs e)
        {
            string Nimi = itemNameTextBox.Text;
            int Saldo;
            if (int.TryParse(itemQuantityTextBox.Text, out Saldo))
            {
                inventory.Add(new Nimike { Name = Nimi, Quantity = Saldo });
                itemNameTextBox.Clear();
                itemQuantityTextBox.Clear();

            }
            else
            {
                MessageBox.Show("Kirjoita määrä kokonaislukuna.");
            }
        }

        private void Poista_nimike(object sender, RoutedEventArgs e)
        {
            if (Inventaario_Lista.SelectedItem != null)
            {
                inventory.Remove((Nimike)Inventaario_Lista.SelectedItem);
            }
            else
            {
                MessageBox.Show("Valitse listalta poistettava nimike.");
            }
        }


        private void ExportToCSV(string filePath)
        {
            StringBuilder csvContent = new StringBuilder();

            // Lisää otsikot
            csvContent.AppendLine("Nimike;Saldo");

            // Lisää rivit
            foreach (var item in inventory)
            {
                csvContent.AppendLine($"{item.Name};{item.Quantity}");
            }

            try
            {
                File.WriteAllText(filePath, csvContent.ToString());
                MessageBox.Show("Viety Exceliin onnistuneesti.", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exeliin vienti epäonnistui: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string currentDate = DateTime.Now.ToString("ddMMyyyy");

            // Tiedoston nimi "Inventaario" + päiväys.
            string fileName = $"Inventaario_{currentDate}.csv";
            // Tiedostopolku CSV-tiedostolle
            string filePath = Path.Combine("C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\", fileName);
            //string filePath = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\Testi.csv"; 
            ExportToCSV(filePath);
        }
        // Vanhan inventaariolistan hakumetodi

        private void loadCSVButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    List<string[]> data = ReadCSVFile(filePath);

                    PopulateListView(data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file. Original error: " + ex.Message);
                }
            }
        }

        private List<string[]> ReadCSVFile(string filePath)
        {
            List<string[]> data = new List<string[]>();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    //Skipataan otsikkorivi
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
              
                        string[] values = line.Split(';');

                        // Tarkastetaan, onko rivillä vähintään kaksi elementtiä: Nimike ja Saldo
                        if (values.Length >= 2)
                        {
                            data.Add(values);
                        }
                        else
                        {
                            MessageBox.Show($"Invalid data in line: {line}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while reading the file: {ex.Message}");
            }

            return data;
        }


        private void PopulateListView(List<string[]> data)
        {
            inventory.Clear(); // Tyhjennetään ensin näkymä

            foreach (string[] row in data)
            {
                string itemName = row[0];
                string quantityStr = row[1];

                // Try parsing quantity as an integer
                if (int.TryParse(quantityStr, out int itemQuantity))
                {
                    // Add the item to the inventory collection
                    inventory.Add(new Nimike { Name = itemName, Quantity = itemQuantity });
                }
                else
                {
                    MessageBox.Show($"Invalid quantity: {quantityStr}");
                }
            }
        }
    }
}

