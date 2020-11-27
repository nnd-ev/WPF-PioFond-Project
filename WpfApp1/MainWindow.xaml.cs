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

namespace WpfApp1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        FondDataContext FondDC = new FondDataContext();
 
        public MainWindow() {

            InitializeComponent();
 
        }

        private void BtnPretrazi_Click(object sender, RoutedEventArgs e) {
            tbUplacenoPoGodini.Text = tbUkupno.Text = "";

            if (!String.IsNullOrEmpty(tbSifraOsiguranik.Text)) {
                puniGrid();
                cmbGodine.ItemsSource = null;
                puniCombo();
            } else {
                MessageBox.Show("Morate uneti sifru osiguranika.");
                tbSifraOsiguranik.Focus();
            }
        }

        private bool proveriVrednost() {
 
            int SifraOsiguranika = int.Parse(tbSifraOsiguranik.Text);
            var pom = (from u in FondDC.Uplates
                       where u.SifOsiguranika.Equals(SifraOsiguranika)
                       select u).Count();

            if(pom > 0) {
                return true;
            } else {
                return false;
            }

        }

        private void puniGrid() {
 
            if (proveriVrednost()) {
                int SifraOsiguranika = int.Parse(tbSifraOsiguranik.Text);

            var pom = from f in FondDC.Firmas   
                      join u in FondDC.Uplates
                      on f.PIB equals u.PIB
                      //where u.SifOsiguranika.Equals(int.Parse(tbSifraOsiguranik.Text))                      
                      where u.SifOsiguranika.Equals(SifraOsiguranika)
                      select new { f.PIB, f.Naziv, f.Grad, u.Godina, u.Mesec, u.Uplaceno };

            glavniGrid.ItemsSource = pom;

            if(pom != null) {
                tbUkupno.Text = pom.Sum(x => x.Uplaceno).ToString();
                
            }
            } else {
                MessageBox.Show("Morate uneti ispravnu sifru osiguranika");
                tbSifraOsiguranik.Focus();
            } 
        }

        private void puniCombo() {
            int SifraOsiguranika = int.Parse(tbSifraOsiguranik.Text);

            var pom = (from u in FondDC.Uplates
                       //where u.SifOsiguranika.Equals(int.Parse(tbSifraOsiguranik.Text))
                       where u.SifOsiguranika.Equals(SifraOsiguranika)
                       select u.Godina).Distinct();

            if(pom != null) {
                cmbGodine.ItemsSource = pom;
            }
        } 

        private void UnosNUplate_Click(object sender, RoutedEventArgs e) {
            GbUnosNoveUplate.IsEnabled = true;
            napuniForme();
        }

        private void napuniForme() {
            var firme = from f in FondDC.Firmas
                        select f;
            cmbFirme.ItemsSource = firme;
        }

        //DELETE
        private void Brisanje_Click(object sender, RoutedEventArgs e) {

            int pib = int.Parse(((TextBlock)glavniGrid.SelectedCells[0].Column.GetCellContent(glavniGrid.SelectedCells[0].Item)).Text);
            int godina = int.Parse(((TextBlock)glavniGrid.SelectedCells[1].Column.GetCellContent(glavniGrid.SelectedCells[0].Item)).Text);
            int mesec = int.Parse(((TextBlock)glavniGrid.SelectedCells[2].Column.GetCellContent(glavniGrid.SelectedCells[0].Item)).Text);


            int sifraOsig = int.Parse(tbSifraOsiguranik.Text);

            Uplate brisiUplatu = (from u in FondDC.Uplates
                                  where u.PIB == pib && u.Godina == godina && u.Mesec == mesec && u.SifOsiguranika == sifraOsig
                                  select u).SingleOrDefault();

            MessageBoxResult deleteRezult = MessageBox.Show("Da li ste sigurni da zelite da obrisete uplatu? ", "Brisanje", MessageBoxButton.YesNo);

            if(deleteRezult == MessageBoxResult.Yes) {
                FondDC.Uplates.DeleteOnSubmit(brisiUplatu);
                puniGrid();

                try {
                    FondDC.SubmitChanges();
                    MessageBox.Show("Uspesno brisanje");
                    tbUplacenoUnos.Text = tbUplacenoPoGodini.Text = "";

                    puniGrid();
                    cmbGodine.ItemsSource = null;
                    puniCombo();
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CmbGodine_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            tbUplacenoPoGodini.Text = "";

            if(cmbGodine.SelectedIndex != -1) {
                var pom = (from u in FondDC.Uplates
                           where u.SifOsiguranika.Equals(int.Parse(tbSifraOsiguranik.Text))
                           && u.Godina.Equals(int.Parse(cmbGodine.SelectedValue.ToString()))
                           select u.Uplaceno).Sum();

                tbUplacenoPoGodini.Text = pom.ToString();
            }
        }

        private void CmbFirme_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Firma izabranaFirma = (Firma)cmbFirme.SelectedItem;
            tbAdresa.Text = izabranaFirma.Adresa;
            tbGrad.Text = izabranaFirma.Grad;
        }


        //INSERT DATA
        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e) {
            if(!String.IsNullOrEmpty(tbGodinaUnos.Text) && !String.IsNullOrEmpty(tbUplacenoUnos.Text) && !String.IsNullOrEmpty(cmbFirme.SelectedItem.ToString()) && !String.IsNullOrEmpty(tbSifraOsiguranik.Text) && !String.IsNullOrEmpty(tbMesec.Text)){

                
                Uplate novaUplata = new Uplate {
                    PIB = ((Firma)cmbFirme.SelectedItem).PIB,
                    Godina = int.Parse(tbGodinaUnos.Text),
                    Mesec = int.Parse(tbMesec.Text),
                    Uplaceno = int.Parse(tbUplacenoUnos.Text),
                    SifOsiguranika = int.Parse(tbSifraOsiguranik.Text)
                };

                FondDC.Uplates.InsertOnSubmit(novaUplata); 

                try {

                    FondDC.SubmitChanges();
                    MessageBox.Show("Uspesan unos");
                    tbGodinaUnos.Text = tbUplacenoUnos.Text = tbMesec.Text=  "";
                    sliderMesec.Value = 0;

                    cmbGodine.ItemsSource = null;
                    puniGrid();
                    puniCombo();


                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            } else {
                MessageBox.Show("Sva polja su obavezna");
            }
        }

        private void Promena_Click(object sender, RoutedEventArgs e) {
            Promena promena = new Promena();
            promena.ShowDialog();
            napuniForme();
        }

       
        
    }
}
