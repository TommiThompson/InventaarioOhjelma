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
//Tommi Villanen Ohjelmistokehityksen näyttö 15.5.2024
//Windows-työpöytä sovellus, joka mahdollistaa jatkuvan inventaarion, liitettynä jatkossa Sharepoint-tietokantaan.

{
    public partial class MainWindow : Window
    {
        public string currentDate = DateTime.Now.ToString("ddMMyyyy");
        public string Tiedosto_Nimi { get; set; }
        public string Tiedosto_Polku { get; set; }
        private readonly string oletusPolkuSM = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\SM\\";
        private readonly string oletusPolkuPI = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\PI\\";
        private readonly string oletusPolkuKISALLINTIE = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\KISALLINTIE\\";
        private readonly string oletusPolkuMUUTLISTAT = "C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\MUUT LISTAT\\";

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
                MessageBox.Show("Nimike-, Saldo-, ja yksikkökentät ovat pakollisia.");


                // Muuta tekstilaatikon väriä, jos arvoja ei ole syötetty
                NimikeTextBox.Background = Brushes.Orange;
                NimikeSaldoTextBox.Background = Brushes.Orange;
                YksikkoTextBox.Background = Brushes.Orange;
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
                LisaaSaldoButton.Background = Brushes.LightGray;
                Muuta_Saldo.BorderBrush = Brushes.Orange;
                LisaaSaldoButton.BorderBrush = Brushes.Orange;

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

            if (string.IsNullOrEmpty(Tiedosto_Polku))
            {
                MessageBox.Show("Tiedostopolku on tyhjä.");
                return;
            }

            Luo_CSV(Tiedosto_Polku, Encoding.UTF8);
            Lisaa_nimike.Background = Brushes.LightGray;
            NimikeTextBox.Background = Brushes.White;
            YksikkoTextBox.Background = Brushes.White;
            Muuta_Saldo.BorderBrush = Brushes.Orange;
            LisaaSaldoButton.BorderBrush = Brushes.Orange;
        }
        private void Tallenna_Uusi_Lista(object sender, RoutedEventArgs e)
        {
            // Avataan valinta ikkuna, johon käyttäjä voi luoda uuden tiedoston.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = oletusPolkuMUUTLISTAT;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = Path.Combine("C:\\Users\\Tommi Villanen\\source\\repos\\inventory\\MUUT LISTAT\\", saveFileDialog.FileName);

                // Luodaan uusi CSV-tiedosto valittuun polkuun.
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        // Lisää otsikkorivi.
                        sw.WriteLine("Koodi;Nimike;Saldo;Yksikkö;Hälytysraja");
                    }

                    MessageBox.Show("Luotu uusi Excel-tiedosto.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Virheviesti.
                    MessageBox.Show("Virhe luodessa uutta tiedostoa.: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        // Lisättiin kentät 'koodi' ja 'yksikkö' ja myös hälytysraja.
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
                            Nimike_Halytysraja = "0";
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
                    // Valitaan rivi.
                    Nimike selectedNimike = (Nimike)Inventaario_Lista.SelectedItem;

                    // Varmistetaan, että saldo on isompi tai vähintään sama, kuin vähennys.
                    if (selectedNimike.Quantity >= Vahennys)
                    {
                        // Vähennetään syötetty saldo nykyisestä saldosta.
                        int Uusi_Saldo = selectedNimike.Quantity - Vahennys;

                        // Päivitä saldo
                        Paivita_Saldo(selectedNimike, Uusi_Saldo);
                        NimikeSaldoTextBox.Clear();
                        NimikeTextBox.Clear();
                        NimikeSaldoTextBox.Background = Brushes.White;
                        LisaaSaldoButton.Background = Brushes.LightGray;

                        // Kutsutaan Exceliin tallennus funktiota.
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
            YksikkoTextBox.Background = Brushes.White;
            SaldoTextBox.Clear();
            LisaaSaldoButton.Background = Brushes.LightGray;
            Muuta_Saldo.BorderBrush = Brushes.Orange;
            LisaaSaldoButton.BorderBrush = Brushes.Orange;
            comboBox1.SelectedItem = null;
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
                YksikkoTextBox.Background = Brushes.White;

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
            Muuta_Saldo.BorderBrush = Brushes.White;
            LisaaSaldoButton.Background = Brushes.Green;
            LisaaSaldoButton.BorderBrush = Brushes.White;
            NimikeTextBox.Background = Brushes.White;
            YksikkoTextBox.Background = Brushes.White;
        }
        private void NimikeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Poista_Nimike.Background = Brushes.LightGray; // Vaihdetaan väri oletusväriksi, kun saldoruutu valitaan.
            NimikeSaldoTextBox.Background = Brushes.White;
            Muuta_Saldo.Background = Brushes.LightGray;
            Muuta_Saldo.BorderBrush = Brushes.Orange;
            LisaaSaldoButton.Background = Brushes.LightGray;
            LisaaSaldoButton.BorderBrush = Brushes.Orange;
            NimikeTextBox.Background = Brushes.White;
            YksikkoTextBox.Background = Brushes.White;
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

            if (comboBox.SelectedItem != null)
            {
                string selectedValue = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();

                switch (selectedValue)
                {
                    case "PI":
                        Tyhjenna_NakymaButton_Click(sender, e);
                        Tiedosto_Nimi = "PI_Inventaario.csv";
                        Tiedosto_Polku = Path.Combine(oletusPolkuPI, Tiedosto_Nimi);
                        break;
                    case "SM":
                        Tyhjenna_NakymaButton_Click(sender, e);
                        Tiedosto_Nimi = "SM_Inventaario.csv";
                        Tiedosto_Polku = Path.Combine(oletusPolkuSM, Tiedosto_Nimi);
                        break;
                    case "KISÄLLINTIE":
                        Tyhjenna_NakymaButton_Click(sender, e);
                        Tiedosto_Nimi = "KISALLINTIE_Inventaario.csv";
                        Tiedosto_Polku = Path.Combine(oletusPolkuKISALLINTIE, Tiedosto_Nimi);
                        break;
                    case "MUUT LISTAT":
                        Tyhjenna_NakymaButton_Click(sender, e);
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                        openFileDialog.InitialDirectory = oletusPolkuMUUTLISTAT; // Oletus tallennuspaikka.

                        if (openFileDialog.ShowDialog() == true)
                        {
                            Tiedosto_Nimi = Path.GetFileName(openFileDialog.FileName);
                            Tiedosto_Polku = openFileDialog.FileName;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        Tyhjenna_NakymaButton_Click(sender, e);
                        break;
                }

                LoadDataFromSelectedFilePath(Tiedosto_Polku); // Parametrinä valittu (CASE) tiedostopolku ja tiedosto.
            }
        }

        private void LoadDataFromSelectedFilePath(string filePath)
        {
            // Tarkistetaan löytyykö tiedostopolku.
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    List<string[]> data = LueCSVtiedosto(filePath, Encoding.UTF8);
                    Taydenna_ListView_Nakyma(data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Tiedoston lukeminen epäonnistui: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Tiedostopolkua ei löydy tai se on tyhjä.");
            }
        }
    }
}



