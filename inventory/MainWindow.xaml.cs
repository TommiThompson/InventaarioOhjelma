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
//Windows-työpöytä sovellus, joka mahdollistaa jatkuvan inventaarion, liitettynä Sharepoint-tietokantaan.

{
    public partial class MainWindow : Window
    {
        //ObservableCollection<Nimike> on .NET-kehyksen tarjoama kokoelmaluokka, joka toteuttaa INotifyCollectionChanged-rajapinnan.
        ObservableCollection<Nimike> inventaario = new ObservableCollection<Nimike>();


        //<Nimike>-niminen inventaario on kokoelma, joka sisältää Nimike-luokan esiintymiä.
        //Kaikki tähän kokoelmaan tehdyt muutokset näkyvät automaattisesti käyttöliittymässä
        //ObservableCollectionin antamien ilmoitusten vuoksi.

        public MainWindow()
        {
            //Alustetaan lomake. Esimerkiksi painikkeiden, tapahtumakäsittelijöiden
            //määrittämiseen käyttöliittymässä.
            InitializeComponent();
            Inventaario_Lista.ItemsSource = inventaario;

        }


        private void BtnLisaa_Nimike(object sender, RoutedEventArgs e)
        {
            string Koodi = KoodiTextBox.Text;
            string Nimi = NimikeTextBox.Text;
            int Saldo;
            string Yksikko = YksikkoTextBox.Text;
            if (string.IsNullOrWhiteSpace(Nimi))
            {
                MessageBox.Show("Nimike- ja Saldo kentät ovat pakollisia.");

                // Muuta tekstilaatikon väriä, jos arvoja ei ole syötetty
                NimikeTextBox.Background = Brushes.Orange;
                NimikeSaldoTextBox.Background = Brushes.Orange;
                return;
            }
            if (int.TryParse(NimikeSaldoTextBox.Text, out Saldo))
            {              
                    inventaario.Add(new Nimike { Code = Koodi, Name = Nimi, Quantity = Saldo, Units = Yksikko });
                    NimikeTextBox.Clear();
                    NimikeSaldoTextBox.Clear();
                    KoodiTextBox.Clear();
                    YksikkoTextBox.Clear();
                    NimikeSaldoTextBox.Background = Brushes.White;
                    NimikeTextBox.Background = Brushes.White;

            }
            else
            {
                MessageBox.Show("Kirjoita saldon määrä kokonaislukuna.");
                NimikeSaldoTextBox.Background = Brushes.Orange;
            }
        }

        private void Poista_nimikeButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (Inventaario_Lista.SelectedItem != null)
            {              
                inventaario.Remove((Nimike)Inventaario_Lista.SelectedItem);
            }
            else
            {
                MessageBox.Show("Valitse ensin listalta poistettava nimike.");
            }
        }


        private void Luo_CSV(string Tiedosto_Polku)
        {
            // Luo StreamReaderin instanssin tiedostoon kirjoittamista varten.
            StringBuilder CSVSisalto = new StringBuilder();

            // Lisää otsikot
            CSVSisalto.AppendLine("Koodi;Nimike;Saldo;Yksikkö");

            // Lisää rivit
            foreach (var item in inventaario)
            {
                CSVSisalto.AppendLine($"{item.Code};{item.Name};{item.Quantity};{item.Units}");
            }

            try
            {
                File.WriteAllText(Tiedosto_Polku, CSVSisalto.ToString());
                MessageBox.Show("Viety Exceliin onnistuneesti.", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exeliin vienti epäonnistui: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void VieExceliinButton_Click(object sender, RoutedEventArgs e)
        {
            string currentDate = DateTime.Now.ToString("ddMMyyyy");

            // Tiedoston nimi "Inventaario" + päiväys.
            string Tiedosto_Nimi = $"Inventaario_{currentDate}.csv";
            // Tiedostopolku CSV-tiedostolle
            string Tiedosto_Polku = Path.Combine("C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\", Tiedosto_Nimi);
            //string filePath = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\Testi.csv"; 
            Luo_CSV(Tiedosto_Polku);
        }


        // Vanhan inventaariolistan hakufunktio
        private void HaeListaButton_Click(object sender, RoutedEventArgs e)
        {
            //Avaa valintaikkunan,josta valitaan Excel-tiedosto (csv).
            OpenFileDialog AvaaExcelValinta = new OpenFileDialog();
            AvaaExcelValinta.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            AvaaExcelValinta.FilterIndex = 1;
            AvaaExcelValinta.RestoreDirectory = true;
            NimikeSaldoTextBox.Background = Brushes.White;
            NimikeTextBox.Background = Brushes.White;

            if (AvaaExcelValinta.ShowDialog() == true)
            {
                try
                {
                    string Tiedosto_Polku = AvaaExcelValinta.FileName;
                    List<string[]> data = LueCSVtiedosto(Tiedosto_Polku);

                    Taydenna_ListView_Nakyma(data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file. Original error: " + ex.Message);
                }
            }
        }

        private List<string[]> LueCSVtiedosto(string Tiedosto_Polku)
        {
                List<string[]> data = new List<string[]>();

            try
            {
                // Luo StreamReaderin instanssin tiedostosta lukemista varten.
                // Use-lause sulkee myös StreamReaderin.
                using (var reader = new StreamReader(Tiedosto_Polku, Encoding.UTF8))
                {
                    //Skipataan otsikkorivi
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string rivi = reader.ReadLine();
              
                        string[] arvot = rivi.Split(';');

                        // Tarkastetaan, onko rivillä vähintään kaksi elementtiä: Nimike ja Saldo
                        // Huom! Jatkossa mahdollisesti lisättävä elementtejä esim. hinta
                        // Lisättiin kentät 'koodi' ja 'yksikkö'
                        if (arvot.Length >= 4)
                        {
                            data.Add(arvot);
                        }
                        else
                        {
                            MessageBox.Show($"Invalid data in line: {rivi}");
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


        private void Taydenna_ListView_Nakyma(List<string[]> data)
        {
            inventaario.Clear(); // Tyhjennetään ensin näkymä

            foreach (string[] row in data)
            {
                string Nimike_Koodi = row[0];
                string Nimike_Nimi = row[1];
                string Nimike_Saldo = row[2];
                string Nimike_Yksikko = row[3];

                // Tarkistetaan saldo kokonaislukuna
                if (int.TryParse(Nimike_Saldo, out int itemQuantity))
                {
                    // Lisätään nimike inventaariolistaan
                    inventaario.Add(new Nimike { Code = Nimike_Koodi, Name = Nimike_Nimi, Quantity = itemQuantity, Units = Nimike_Yksikko });
                }
                else
                {
                    MessageBox.Show($"Invalid quantity: {Nimike_Saldo}");
                }
            }
        }
        public int Uusi_Saldo;
        private void Paivita_Saldo(Nimike Valittu_Nimike, int Uusi_Saldo)
        {
            // Etsitään kyseinen indeksi ObservableCollectionista
            int indeksi = inventaario.IndexOf(Valittu_Nimike);

            // Tarkistetaan, löytyykö nimike
            if (indeksi != -1)
            {
                // Päivitä valitun nimikkeen saldo
                inventaario[indeksi].Quantity = Uusi_Saldo;

                // Siirretään muuttunut nimike ItemsListiin
                Inventaario_Lista.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Valitsemaasi nimikettä ei löydy listalta.");
            }
        }
        private void Uusi_SaldoButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (Inventaario_Lista.SelectedItem != null)
                
            {
                if (int.TryParse(NimikeSaldoTextBox.Text, out int Uusi_Saldo))
                {
                    Paivita_Saldo((Nimike)Inventaario_Lista.SelectedItem, Uusi_Saldo);
                    NimikeSaldoTextBox.Clear(); // Tyhjennetään TextBox päivityksen jälkeen.
                    NimikeSaldoTextBox.Background = Brushes.Orange;
                }
                else
                {
                    MessageBox.Show("Syötä ensin uusi saldo 'SALDO' ruutuun.");
                    NimikeSaldoTextBox.Background = Brushes.Orange;
                }
            }
            else
            {
                MessageBox.Show("Valitse ensin nimike, jonka saldoa haluat muuttaa.");
            }
        }
        private void ClearListViewButton_Click(object sender, RoutedEventArgs e)
        {
            // Kutsutaan clear-metodia tyhjentämään kenttiä. Lisäksi palautetaan kentän väri.
            inventaario.Clear();
            NimikeTextBox.Clear();
            NimikeSaldoTextBox.Clear();
            KoodiTextBox.Clear();
            YksikkoTextBox.Clear();
            NimikeSaldoTextBox.Background = Brushes.White;
            NimikeTextBox.Background = Brushes.White;
        }
        private void Inventaario_Lista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Inventaario_Lista.SelectedItem != null)
            {
                Nimike selectedNimike = (Nimike)Inventaario_Lista.SelectedItem;

                // Näytetään SALDO-ruudussa nykyinen listassa oleva saldo.
                NimikeSaldoTextBox.Text = selectedNimike.Quantity.ToString();
                Poista_Nimike.Background = Brushes.Orange;// Muutetaan buttonien ja tekstilaatikon väri valittaessa rivi.
                Muuta_Saldo.Background = Brushes.Orange;
                NimikeSaldoTextBox.Background = Brushes.Orange;
            }
            else
            {
                Poista_Nimike.Background = Brushes.LightGray; // Muutetaan takaisin oletusväriksi, jossei riviä valita.
                Muuta_Saldo.Background = Brushes.LightGray;
                NimikeSaldoTextBox.Background = Brushes.White;
            }
        }

    }
       
    
}


