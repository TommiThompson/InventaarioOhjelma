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
using System.Threading.Tasks;




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
                Muuta_Saldo.Background = Brushes.LightGray;
                
                VieExceliinButton_Click(sender, e);

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
                // Kysytään käyttäjältä vahvistus rivin poistoon.
                MessageBoxResult result = MessageBox.Show("Haluatko varmasti poistaa nimikkeen?,", "Poista nimike", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    // Jos
                    inventaario.Remove((Nimike)Inventaario_Lista.SelectedItem);
                    VieExceliinButton_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // Suljetaan ikkuna
                    return;
                }
            }
            else
            {
                MessageBox.Show("Valitse ensin listalta poistettava nimike.");
            }
        }



        private void Luo_CSV(string Tiedosto_Polku, Encoding encoding)
        {
            // Luo StreamReaderin instanssin tiedostoon kirjoittamista varten.
            StringBuilder CSVSisalto = new StringBuilder();

            // Lisää otsikot
            CSVSisalto.AppendLine("Koodi;Nimike;Saldo;Yksikkö;Hälytysraja");

            // Lisää rivit
            foreach (var item in inventaario)
            {
                CSVSisalto.AppendLine($"{item.Code};{item.Name};{item.Quantity};{item.Units};{item.Alarm}");
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
            if (inventaario.Count == 0)
            {
                MessageBox.Show("Tyhjää listaa ei pysty tallentamaan.", "Lista tyhjä!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentDate = DateTime.Now.ToString("ddMMyyyy");

            // Tiedoston nimi "Inventaario" + päiväys.
            string Tiedosto_Nimi = $"SM_Inventaario_{currentDate}.csv";
            
            // Tiedostopolku CSV-tiedostolle
            string Tiedosto_Polku = Path.Combine("C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\SM\\", Tiedosto_Nimi);
            
            //string filePath = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\Testi.csv"; 
            Luo_CSV(Tiedosto_Polku, Encoding.UTF8);
            Lisaa_nimike.Background = Brushes.LightGray;
            NimikeTextBox.Background = Brushes.White;

        }


        // Vanhan inventaariolistan hakufunktio
        private void HaeListaButton_Click(object sender, RoutedEventArgs? e)
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
                    List<string[]> data = LueCSVtiedosto(Tiedosto_Polku, Encoding.UTF8);

                    Taydenna_ListView_Nakyma(data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file. Original error: " + ex.Message);
                }
            }
        }
        

        private List<string[]> LueCSVtiedosto(string Tiedosto_Polku, Encoding encoding)
        {
            List<string[]> data = new List<string[]>();

            try
            {
                // Luo StreamReaderin instanssin tiedostosta lukemista varten.
                // Use-lause sulkee myös StreamReaderin.
                using (var reader = new StreamReader(Tiedosto_Polku, encoding))
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
                        if (arvot.Length >= 5)
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
                MessageBox.Show($"Virhe luettaessa tiedostoa: {ex.Message}");
            }

            return data;
        }

        private void Taydenna_ListView_Nakyma(List<string[]> data)
        {
            inventaario.Clear(); // Tyhjennetään ensin näkymä

            foreach (string[] row in data)
            {
                if (row.Length >= 5)
                {
                    string Nimike_Koodi = row[0];
                    string Nimike_Nimi = row[1];
                    string Nimike_Saldo = row[2];
                    string Nimike_Yksikko = row[3];
                    string Nimike_Halytysraja = row[4];

                    // Tarkistetaan saldo kokonaislukuna
                    if (int.TryParse(Nimike_Saldo, out int itemQuantity))
                    {
                        SolidColorBrush backgroundColor = Brushes.White;

                        // Vertaillaan csv-tiedoston saraketta 3, sarakkeeseen 4.
                        if (int.TryParse(Nimike_Halytysraja, out int alarm))
                        {
                            if (itemQuantity < alarm)
                            {
                                // Jos saldo on pienempi kuin asetettu hälytysraja, muuttuu rivi punaiseksi.
                                backgroundColor = Brushes.Red;
                            }
         
                        }
                        else
                        {
                            MessageBox.Show($"Invalid alarm value: {Nimike_Halytysraja}");
                        }

                        // Lisätään nimike inventaariolistaan
                        Nimike nimike = new Nimike { Code = Nimike_Koodi, Name = Nimike_Nimi, Quantity = itemQuantity, Units = Nimike_Yksikko, Alarm = Nimike_Halytysraja };
                        nimike.BackgroundColor = backgroundColor;
                        inventaario.Add(nimike);
                    }
                    else
                    {
                        MessageBox.Show($"Invalid quantity: {Nimike_Saldo}");
                    }
                }
                else
                {
                    MessageBox.Show($"Invalid data format");
                }
            }

            // Asetetaan tyyli ListView-itemille.
            Style itemStyle = new Style(typeof(ListViewItem));
            itemStyle.Setters.Add(new Setter(ListViewItem.BackgroundProperty, new Binding("BackgroundColor")));
            Inventaario_Lista.ItemContainerStyle = itemStyle;
        }

        public int Uusi_Saldo;
        //public int Vanha_Saldo;


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
                NimikeSaldoTextBox.Background = Brushes.White;
                Muuta_Saldo.Background = Brushes.LightGray;

                // Vaihdetaan rivin väriä, jos saldo muutettaessa on sama tai suurempi, kuin hälytysraja.
                foreach (var item in Inventaario_Lista.Items)
                {
                    Nimike nimike = item as Nimike;
                    if (nimike != null)
                    {
                        if (nimike.Quantity < int.Parse(nimike.Alarm))
                        {
                            nimike.BackgroundColor = Brushes.Red;
                        }
                        else
                        {
                            nimike.BackgroundColor = Brushes.White;
                        }
                    }
                }
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
                if (int.TryParse(NimikeSaldoTextBox.Text, out int Vahennys))
                {
                    // Get the selected item
                    Nimike selectedNimike = (Nimike)Inventaario_Lista.SelectedItem;

                    // Ensure saldo is greater than or equal to the reduction
                    if (selectedNimike.Quantity >= Vahennys)
                    {
                        // Vähennetäänb syötetty saldo nykyisestä saldosta.
                        int Uusi_Saldo = selectedNimike.Quantity - Vahennys;

                        // Päivitä saldo
                        Paivita_Saldo(selectedNimike, Uusi_Saldo);
                        NimikeSaldoTextBox.Clear();
                        NimikeTextBox.Clear();
                        NimikeSaldoTextBox.Background = Brushes.White;
                        LisaaSaldoButton.Background = Brushes.LightGray;

                        // Trigger export to Excel
                        VieExceliinButton_Click(sender, e);
                    }
                    else
                    {
                        MessageBox.Show("Syötä luku, joka on pienempi tai yhtä suuri kuin nykyinen saldo.");
                        NimikeSaldoTextBox.Background = Brushes.Orange;
                    }
                }
                else
                {
                    MessageBox.Show("Syötä ensin vähennettävä saldo 'SALDO' ruutuun.");
                    NimikeSaldoTextBox.Background = Brushes.Orange;
                }
            }
            else
            {
                MessageBox.Show("Valitse ensin nimike (rivi), jonka saldoa haluat muuttaa.");
            }
        }
        private void LisaaSaldoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Inventaario_Lista.SelectedItem != null)
            {
                if (int.TryParse(NimikeSaldoTextBox.Text, out int Lisays))
                {
                    // Hae valittu nimike.
                    Nimike selectedNimike = (Nimike)Inventaario_Lista.SelectedItem;

                    // Lisää syötetty määrä saldoon.
                    int Uusi_Saldo = selectedNimike.Quantity + Lisays;

                    // Päivitä saldo
                    Paivita_Saldo(selectedNimike, Uusi_Saldo);
                    NimikeSaldoTextBox.Clear();
                    NimikeTextBox.Clear();
                    NimikeSaldoTextBox.Background = Brushes.White;
                    LisaaSaldoButton.Background = Brushes.LightGray;
                    VieExceliinButton_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Syötä ensin lisättävä saldo 'SALDO' ruutuun.");
                    NimikeSaldoTextBox.Background = Brushes.Orange;
                }
            }
            else
            {
                MessageBox.Show("Valitse ensin nimike (rivi), jonka saldoa haluat muuttaa.");
            }
        }
        private void Tyhjenna_NakymaButton_Click(object sender, RoutedEventArgs e)
        {
            // Kutsutaan clear-metodia tyhjentämään kenttiä. Lisäksi palautetaan kentän väri.
            inventaario.Clear();
            NimikeTextBox.Clear();
            NimikeSaldoTextBox.Clear();
            KoodiTextBox.Clear();
            YksikkoTextBox.Clear();
            NimikeSaldoTextBox.Background = Brushes.White;
            NimikeTextBox.Background = Brushes.White;
            Lisaa_nimike.Background = Brushes.LightGray;
            SaldoTextBox.Clear();
            LisaaSaldoButton.Background = Brushes.LightGray;
            Inv_Alueet.SelectedItem = null;
        }
        private void Inventaario_Lista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Nimike nimike in inventaario)
            {
                nimike.BackgroundColor = Brushes.White; // Palauta oletus taustaväri kaikille nimikkeille.
            }

            if (Inventaario_Lista.SelectedItem != null)
            {
                Nimike selectedNimike = (Nimike)Inventaario_Lista.SelectedItem;

                SaldoTextBox.Text = selectedNimike.Quantity.ToString();
                Poista_Nimike.Background = Brushes.Orange;               
                NimikeSaldoTextBox.Background = Brushes.Orange;
                NimikeTextBox.Background = Brushes.White;
                Lisaa_nimike.Background = Brushes.LightGray;
                selectedNimike.BackgroundColor = Brushes.LightGreen;
            }
            else
            {
                // Palauta oletusvärit, jos yhtään riviä ei valita.
                Poista_Nimike.Background = Brushes.LightGray;
                Muuta_Saldo.Background = Brushes.LightGray;
                NimikeSaldoTextBox.Background = Brushes.White;
                SaldoTextBox.Clear();
                NimikeTextBox.Clear();
                KoodiTextBox.Clear();
                YksikkoTextBox.Clear();
            }
        }

        private void NimikeSaldoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Poista_Nimike.Background = Brushes.LightGray; // Vaihdetaan väri oletusväriksi, kun saldoruutu valitaan.
            NimikeSaldoTextBox.Background = Brushes.LightGreen;
            Muuta_Saldo.Background = Brushes.Red;
            LisaaSaldoButton.Background = Brushes.Green;
        }

        private void ShowInfobutton_Click(object sender, RoutedEventArgs e) // Annettaan käyttäjälle infoa, kuinka ohjelmaa käytetään.
        {
            MessageBox.Show("Valitse ensin hiirellä klikkaamalla listalta rivi. Tämän jälkeen voit joko muuttaa saldoa syöttämällä " +
                "uuden saldon 'SALDO' ruutuun ja klikkaamalla MUUTA SALDOA '+' tai '-' -painikkeita riippuen siitä haluatko lisätä tai vähentää saldoa." + 
                "Vaihtoehtoisesti voit poistaa nimikkeen 'POISTA NIMIKE'-painikkeella." +
                "Voit myös lisätä nimikkeen syöttämällä tiedot ainakin kenttiin 'NIMIKE', 'SALDO' ja painamalla 'LISÄÄ NIMIKE'-painiketta");
        }


        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            // Tarkistetaan onko listalta valittu itemi ja onko se "SM"
            if (comboBox.SelectedItem != null && ((ComboBoxItem)comboBox.SelectedItem).Content.ToString() == "SM")
            {
                // Kutsutaan metodia, kun Comboboxista valitaan 'SM'.
                HaeListaButton_Click(sender, null);
            }

        }

    }

}
