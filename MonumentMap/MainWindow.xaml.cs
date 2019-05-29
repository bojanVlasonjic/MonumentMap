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

using System.Windows.Controls.Primitives;
using Microsoft.Maps.MapControl.WPF; /* Direktiva za mapu i njene elemente */
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel; //observable collections

namespace MonumentMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //sluzi za databinding na neke konstante, npr velicina fonta, dimenzija prozora... 
        public WindowConstants WindowConstants { get; set; }

        /******** Booleans indicating whether pop-up windows are shown ********/
        public bool isNewMonumentWindowShown = false;


        /***************** Observable collections *****************/
        public ObservableCollection<Monument> observ_monuments;
        //TODO: za tipove spomenika i tagove kolekcija


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;  

            worldMap.MouseDoubleClick += new MouseButtonEventHandler(worldMap_MouseDoubleClick);
            worldMap.ViewChangeOnFrame += new EventHandler<MapEventArgs>(worldMap_ViewChangeOnFrame);

            onLoad();
        }

       
        private void onLoad()
        {
            
            WindowConstants = new WindowConstants();
            observ_monuments = new ObservableCollection<Monument>();

            /* Initializing font sizes */
        WindowConstants.HeaderFontSize = 18;
            WindowConstants.FormFontSize = 16; //do 18 je ok

            WindowConstants.RowSpacing = 18; //setting space between rows in grid

            //inserting enums to comboboxes
            climateType.ItemsSource = Enum.GetValues(typeof(ClimateType)).Cast<ClimateType>();
            touristStatus.ItemsSource = Enum.GetValues(typeof(TouristStatus)).Cast<TouristStatus>();
        }

        //uklanjanje overflow-a na ikonicama toolbar-a
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


                        /*********************
                        * MAP EVENT HANDLERS *
                        * *******************/

        //na dupli klik se ubaci pin na mapu - za sad
        private void worldMap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            e.Handled = true;

            Point mousePosition = e.GetPosition(this);
            Location pinLocation = worldMap.ViewportPointToLocation(mousePosition);

            ControlTemplate template = (ControlTemplate)this.FindResource("MonumentPinTemplate"); //template za promenu izgleda pin-a

            Pushpin pin = new Pushpin();
            pin.Template = template;
            pin.Location = pinLocation;
            pin.Content = "pin";

            worldMap.Children.Add(pin);
        }

        //postavljanje granice za zoom
        private void worldMap_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            double z = worldMap.ZoomLevel;

            //setting min zoom 
            if (z > 8)
            {
                worldMap.ZoomLevel = 8;
            }

            //settin max zoom
            if (z < 2.5)
            {
                worldMap.ZoomLevel = 2.5;
            }
        }


                    /**********************************
                    * ANIMATION BUTTONE EVENT HANDLERS *
                    * *********************************/

        private void closeNewMonumWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            if(isNewMonumentWindowShown)
            {
                DoubleAnimation double_anim = new DoubleAnimation
                {
                    From = 0,
                    To = -newMonumentHolder.Width,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AutoReverse = false
                };

                newMonumentHolder.BeginAnimation(Canvas.LeftProperty, double_anim);
                isNewMonumentWindowShown = false;
            }
        }

        private void newMonumentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isNewMonumentWindowShown)
            {

                DoubleAnimation double_anim = new DoubleAnimation
                {
                    From = -newMonumentHolder.Width,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AutoReverse = false
                };

                newMonumentHolder.BeginAnimation(Canvas.LeftProperty, double_anim);
                isNewMonumentWindowShown = true;
            }
        }


                    /*************************
                    * BUTTON ON_CLICK EVENTS *
                    * ***********************/

        private void changeMonumIconBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO: proveriti dimenzije slike koja se upload-uje
        }

        private void changeMonumPicBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO: proveriti dimenzije ikonice koja se upload-uje
        }

        private void addMonumentBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO: proveriti da li su polja prazna
            //TODO: proveri da li unesena sifra postoji
            //TODO: kreirati novi spomenik i dodati ga u observable collection
        }

        private void addTagBtn_Click(object sender, RoutedEventArgs e)
        {
            //dodavanje neke od postojecih etiketa spomeniku
        }
    }
}
