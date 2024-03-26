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


        private void Lisaa_Nimike(object sender, RoutedEventArgs e)
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
                MessageBox.Show($"Error exporting inventory to CSV: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Tiedostopolku CSV-tiedostolle
            string filePath = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\Testi.csv"; // Modify the path as needed
            ExportToCSV(filePath);
        }
    }
}

