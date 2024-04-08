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
//Tommi Villanen Ohjelmoinnin näyttö 12.04.2024

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
                MessageBox.Show("Valitse ensin listalta poistettava nimike.");
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
                        // Huom! Jatkossa mahdollisesti lisättävä elementtejä esim. hinta
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

                // Tarkistetaan saldo kokonaislukuna
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
        private void UpdateQuantity(Nimike selectedItem, int newQuantity)
        {
            // Find the index of the selected item in the ObservableCollection
            int index = inventory.IndexOf(selectedItem);

            // Tarkistetaan, löytyykö nimike
            if (index != -1)
            {
                // Päivitä valitun nimikkeen saldo
                inventory[index].Quantity = newQuantity;

                // Siirretään muuttunut nimike ItemsListiin
                Inventaario_Lista.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Valitsemaasi nimikettä ei löydy listalta.");
            }
        }
        private void UpdateQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (Inventaario_Lista.SelectedItem != null)
            {               
                if (int.TryParse(newQuantityTextBox.Text, out int newQuantity))
                {
                    UpdateQuantity((Nimike)Inventaario_Lista.SelectedItem, newQuantity);
                    newQuantityTextBox.Clear(); // Tyhjennetään TextBox päivityksen jälkeen.
                }
                else
                {
                    MessageBox.Show("Syötä ensin uusi saldo 'UUSI SALDO' ruutuun.");
                }
            }
            else
            {
                MessageBox.Show("Valitse ensin nimike, jonka saldoa haluat muuttaa.");
            }
        }
    }

}

