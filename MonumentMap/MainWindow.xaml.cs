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
using System.Windows.Threading;

namespace MonumentMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /************************** Databinding objects **************************/
        public WindowConstants WindowConstants { get; set; }
        public CanvasPositions CanvasPositions { get; set; }

        /******** Booleans indicating whether pop-up windows are shown ********/
        public bool isNewMonumentWindowShown = false;


        /***************** Observable collections *****************/
        public ObservableCollection<Monument> observ_monuments;
        //TODO: za tipove spomenika i tagove kolekcija


        private double mainWindowHeight;
        private double mainWindowWidth;

        static BrushConverter brushConverter = new BrushConverter();


        /******************* Timers *******************/
        static DispatcherTimer userNotificationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(4)
        };


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            mainWindowHeight = Height;
            mainWindowWidth = Width;

            worldMap.MouseDoubleClick += new MouseButtonEventHandler(worldMap_MouseDoubleClick);
            worldMap.ViewChangeOnFrame += new EventHandler<MapEventArgs>(worldMap_ViewChangeOnFrame);
            this.SizeChanged += OnWindowSizeChanged;

            onLoad();
        }


        private void onLoad()
        {
            
            WindowConstants = new WindowConstants();
            CanvasPositions = new CanvasPositions();
            observ_monuments = new ObservableCollection<Monument>();

            /* Initializing font sizes */
            WindowConstants.HeaderFontSize = 20;
            WindowConstants.FormFontSize = 16; //do 18 je ok

            WindowConstants.RowSpacing = 18; //setting space between rows in grid

            /* Initializing pop-up window positions */
            centerPopUpWindows();

            //inserting enums to comboboxes
            climateType.ItemsSource = Enum.GetValues(typeof(ClimateType)).Cast<ClimateType>();
            touristStatus.ItemsSource = Enum.GetValues(typeof(TouristStatus)).Cast<TouristStatus>();
        }


        private void centerPopUpWindows()
        {
            CanvasPositions.Top = (mainWindowHeight / 2) - (newMonumTypeGrid.Height / 2);
            CanvasPositions.Left = (mainWindowWidth / 2) - (newMonumTypeGrid.Width / 2);
        }


        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainWindowHeight = e.NewSize.Height;
            mainWindowWidth = e.NewSize.Width;
            centerPopUpWindows();
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

        private void newTagBtn_Click(object sender, RoutedEventArgs e)
        {
            
            
            newTagGrid.Visibility = Visibility.Visible;

            //darkening background
            //mainCanvas.Opacity = 0.90;
            //mainCanvas.Background = Brushes.Black;

        }

        private void newMonumentTypeBtn_Click(object sender, RoutedEventArgs e)
        {
            //darkening background
            //mainCanvas.Opacity = 0.5;
            //mainCanvas.Background = Brushes.Black;

            newMonumTypeGrid.Visibility = Visibility.Visible;
        }

        private void monumentTypeBrowseBtn_Click(object sender, RoutedEventArgs e)
        {

            string filePath = browseFiles("Icon Files (*.png, *.svg, *.eps, *.psd)|*.png;*.svg;*.eps;*.psd");

            if(filePath != null)
            {
                monumentTypeIconPath.Text = filePath; //place the icon path to hidden text box to remember it

                int fileNameIndex = filePath.LastIndexOf("\\");
                monumentTypeIconName.Text = filePath.Substring(fileNameIndex + 1); //extract the icon name and display it
            } 
          
        }

        private void addMonumentTypeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancelMonumentTypeBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO: isprazni input polja

            //returning background to normal
            //mainCanvas.Opacity = 1;
            //mainCanvas.Background = (Brush)brushConverter.ConvertFrom("#001a33");

            newMonumTypeGrid.Visibility = Visibility.Hidden;
        }

        private void addTagToMonumentBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addTagBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void cancelTagBtn_Click(object sender, RoutedEventArgs e)
        {
            //returning background to normal
            //mainCanvas.Opacity = 0;
            //mainCanvas.Background = (Brush)brushConverter.ConvertFrom("#001a33");

            newTagGrid.Visibility = Visibility.Hidden;
        }


        /********************
        * AUXILIARY METHODS *
        * ******************/

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            //TextBox.Text = "#" + ClrPcker_Background.SelectedColor.R.ToString() + ClrPcker_Background.SelectedColor.G.ToString() + ClrPcker_Background.SelectedColor.B.ToString();
        }

        /** Metoda koja sluzi za odabir fajlova. Tip fajlova se specificira kao parametar
         *  string filterCriteria - sluzi za izlistivanje iskljucivo fajlova tog tipa
         *  return - putanja do odabranog fajla ili null ukoliko fajl nije odabran */
        public string browseFiles(string filterCriteria)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension 
            dlg.Filter = filterCriteria;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file path and return it
            if (result == true)
            {
                return dlg.FileName;
                
            }

            return null;
        }

        private void selectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            tagColorCode.Text = ColorPicker_Tag.SelectedColor.ToString();
            
        }

    }
}
