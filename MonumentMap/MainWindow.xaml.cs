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
using System.Diagnostics;
using System.IO;

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
        public bool isNewMonumentWindowInfoShown = true;


        /***************** Observable collections *****************/
        public ObservableCollection<Monument> observ_monuments;
        //TODO: za tipove spomenika i tagove kolekcija


        private double mainWindowHeight;
        private double mainWindowWidth;

        static BrushConverter brushConverter = new BrushConverter();
        static Thickness FocusThickness = new Thickness(4);
        static Thickness LostFocusThickness = new Thickness(1);


        static string BROWSE_ICONS_FILTER = "Icon Files (*.png, *.svg, *.eps, *.psd)|*.png;*.svg;*.eps;*.psd";
        static string BROWSE_PICS_FILTER = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files(*.*)|*.*";


        //default picture and icon
        private BitmapImage DEFAULT_ICON;
        private BitmapImage DEFAULT_PICTURE;


        static IOSerializer IO_Serializer = new IOSerializer();


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


            observ_monuments = IO_Serializer.deserializeMonuments();

            if(observ_monuments == null)
            {
                NotifyUser("No monuments loaded");
                observ_monuments = new ObservableCollection<Monument>();
            }

            /* Initializing font sizes */
            WindowConstants.HeaderFontSize = 20;
            WindowConstants.FormFontSize = 16; //do 18 je ok

            WindowConstants.RowSpacing = 18; //setting space between rows in grid

            /* Memorizing default monument icon and picture */
            DEFAULT_ICON = new BitmapImage(new Uri(monumentIcon.Source.ToString()));
            DEFAULT_PICTURE = new BitmapImage(new Uri(monumentPicture.Source.ToString()));


            //inserting enums to comboboxes
            monumentType.Items.Add("NO_TYPE");
            //TODO: insert monument types

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

            CanvasPositions.Width = mainWindowWidth;
            CanvasPositions.Height = mainWindowHeight;
            CanvasPositions.ScrollViewerHeights = mainWindowHeight;


            //centerPopUpWindows();
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
          //  NotifyUser(mousePosition.X + " " + mousePosition.Y);
            
            Location pinLocation = worldMap.ViewportPointToLocation(mousePosition);

            ControlTemplate template = (ControlTemplate)this.FindResource("MonumentPinTemplate"); //template za promenu izgleda pin-a

            Pushpin pin = new Pushpin();
            pin.Template = template;
            pin.Location = pinLocation;
            pin.Location.Latitude += 5; //including the toolbar height in lattitude of the pin

            pin.Content = "pin" + worldMap.Children.Count;

            pin.MouseDown += PinClicked;

            worldMap.Children.Add(pin); 
        }


        private void PinClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pushpin p = sender as Pushpin;
            NotifyUser(p.Content.ToString());
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
            closeNewMonumentWindow();
        }

        private void closeNewMonumentWindow()
        {
            if (isNewMonumentWindowShown)
            {
                DoubleAnimation double_anim = new DoubleAnimation
                {
                    From = 0,
                    To = -newMonumentGrid.Width,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AutoReverse = false
                };

                newMonumentGrid.BeginAnimation(Canvas.LeftProperty, double_anim);
                isNewMonumentWindowShown = false;

                clearInputs(newMonumentForm);

                //reseting default icon and picture
                monumentPicture.Source = DEFAULT_PICTURE;
                monumentIcon.Source = DEFAULT_ICON;
            }
        }

        private void newMonumentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isNewMonumentWindowShown)
            {

                DoubleAnimation double_anim = new DoubleAnimation
                {
                    From = -newMonumentGrid.Width,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AutoReverse = false
                };

                newMonumentGrid.BeginAnimation(Canvas.LeftProperty, double_anim);
                isNewMonumentWindowShown = true;
            }
        }


                    /*************************
                    * BUTTON ON_CLICK EVENTS *
                    * ***********************/

        private void changeMonumIconBtn_Click(object sender, RoutedEventArgs e)
        {
            
            string filePath = browseFiles(BROWSE_ICONS_FILTER);

            if(filePath != null)
            {
                monumentIcon.Source = new BitmapImage(new Uri(filePath));
                newMonumentIconPath.Text = filePath;
            }
        }

        private void changeMonumPicBtn_Click(object sender, RoutedEventArgs e)
        {
            
            string filePath = browseFiles(BROWSE_PICS_FILTER);

            if(filePath != null)
            {
                monumentPicture.Source = new BitmapImage(new Uri(filePath));

                newMonumentPicturePath.Text = filePath;
            }
        }

        private void addMonumentBtn_Click(object sender, RoutedEventArgs e)
        {
            
            if(!checkForEmptyFields(newMonumentForm))
            {
                if(monumentIdExists(monumentID.Text))
                {
                    MessageBox.Show("Monument id already exists");
                    return;
                }

                Monument monument = new Monument();

                //extracting textBox values
                monument.ID = monumentID.Text;
                monument.Name = monumentName.Text;
                monument.Description = monumentDescr.Text;
                monument.AnnualIncome = Double.Parse(annualIncome.Text);

                //TODO: postavi tip spomenika

                //parsing comboboxes
                ClimateType climate;
                Enum.TryParse(climateType.SelectedValue.ToString(), out  climate);
                monument.Climate = climate;

                TouristStatus status;
                Enum.TryParse(touristStatus.SelectedValue.ToString(), out status);
                monument.TourStatus = status;

                //extracting values from radio buttons
                monument.IsEcoEndangered = radioButtonChecked(radioEcoPosBtn);
                monument.IsInSettlement = radioButtonChecked(radioSettlementPosBtn);
                monument.ContainsEndangeredSpecies = radioButtonChecked(radioSpeciesPosBtn);

                //parsing date picker
                monument.DateOfDiscovery = discoveryDate.SelectedDate.Value;


                //if the user left the local image as default
                if(newMonumentPicturePath.Text.Equals(""))
                {
                    monument.Picture_path = "pictures/DefaultMonumentImage.jpg";
                } else
                {
                    //get source image extension from path
                    string sourceExtension = getFileExtensionFromPath(newMonumentPicturePath.Text);

                    //get destination path
                    string destFileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + sourceExtension; //to avoid duplicate names
                    string destinationPath = GetDestinationPath(destFileName, "pictures");

                    //copy source image to local folder
                    File.Copy(newMonumentPicturePath.Text, destinationPath);
                    monument.Picture_path = destinationPath;
                }


                //if the user left the default icon
                if(newMonumentIconPath.Text.Equals(""))
                {
                    if(monumentType.SelectedValue.ToString().ToLower().Equals("no_type"))
                    {
                        monument.Icon_path = "icons/monument-icon.png";
                    } else
                    {
                        //TODO: postavi ikonicu odabranog tipa
                    }

                } else
                {
                    //get source image extension from path
                    string sourceExtension = getFileExtensionFromPath(newMonumentIconPath.Text);

                    //get destination path
                    string destFileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + sourceExtension; //to avoid duplicate names
                    string destinationPath = GetDestinationPath(destFileName, "icons");

                    //copy source image to local folder
                    File.Copy(newMonumentIconPath.Text, destinationPath);
                    monument.Icon_path = destinationPath;
                }

                observ_monuments.Add(monument);
                closeNewMonumentWindow();

                if(IO_Serializer.serializeMonuments(observ_monuments))
                {
                    NotifyUser("Monument added");
                } else
                {
                    NotifyUser("Failed to save monuments");
                }
                    
            }
        }

        private void addTagBtn_Click(object sender, RoutedEventArgs e)
        {
            //dodavanje neke od postojecih etiketa spomeniku
            
        }

        private void newTagBtn_Click(object sender, RoutedEventArgs e)
        {

            newTagGridHolder.Visibility = Visibility.Visible;

        }

        private void newMonumentTypeBtn_Click(object sender, RoutedEventArgs e)
        {

            newMonumTypeGridHolder.Visibility = Visibility.Visible;
        }

        private void monumentTypeBrowseBtn_Click(object sender, RoutedEventArgs e)
        {

            string filePath = browseFiles(BROWSE_ICONS_FILTER);

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
            
            clearInputs(newMonumTypeGrid);
            newMonumTypeGridHolder.Visibility = Visibility.Hidden;


        }

        private void addTagToMonumentBtn_Click(object sender, RoutedEventArgs e)
        {

            //TODO: proveri ima li praznih polja


        }

        private void addTagBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void cancelTagBtn_Click(object sender, RoutedEventArgs e)
        {
 
            clearInputs(newTagGrid);
            newTagGridHolder.Visibility = Visibility.Hidden;
        }


                                    /********************
                                    * AUXILIARY METHODS *
                                    * ******************/

        /** Metoda koja sluzi za odabir fajlova. Tip fajlova se specificira kao parametar
         *  string filterCriteria - sluzi za izlistivanje iskljucivo fajlova tog tipa
         *  return - putanja do odabranog fajla ili null ukoliko fajl nije odabran */
        public string browseFiles(string filterCriteria)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false;

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

        private String GetDestinationPath(string filename, string foldername)
        {
            String appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            string directoryPath = String.Format(appStartPath + "\\{0}", foldername);

            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return String.Format(directoryPath + "\\" + filename);
        }

        private void selectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            tagColorCode.Text = ColorPicker_Tag.SelectedColor.ToString();
            
        }

        private bool checkForEmptyFields(Grid inputHolder)
        {
            bool isEmpty = false;

            foreach (UIElement input in inputHolder.Children)
            {
                if (input is TextBox)
                {
                    TextBox textBox = ((TextBox)input);
                    if (textBox.Text.Equals("") && textBox.IsEnabled) {
                        textBox.BorderBrush = Brushes.Red;
                        textBox.BorderThickness = FocusThickness;
                        isEmpty = true;
                    } else
                    {
                        textBox.BorderBrush = Brushes.White;
                        textBox.BorderThickness = LostFocusThickness;
                    }

                } else if(input is Border)
                {
                    Border border = ((Border)input);
                    ComboBox comboBox = ((ComboBox)border.Child);
                    

                    if (comboBox.SelectedItem == null)
                    {
                        
                        isEmpty = true;
                
                        //make border red
                        border.BorderThickness = FocusThickness;
                        border.BorderBrush = Brushes.Red;

                    } else
                    {
                        //make border default
                        border.BorderThickness = LostFocusThickness;
                        border.BorderBrush = Brushes.White;
                    }

                } else if(input is DatePicker)
                {
                    DatePicker date = ((DatePicker)input);

                    if(date.SelectedDate == null)
                    {
                        isEmpty = true;

                        date.BorderBrush = Brushes.Red;
                        date.BorderThickness = FocusThickness;

                    } else
                    {
                        date.BorderThickness = LostFocusThickness;
                        date.BorderBrush = Brushes.White;
                    }
                    
                }
                
            }

            return isEmpty;
        }

        private void clearInputs(Grid inputHolder)
        {
            foreach(UIElement input in inputHolder.Children)
            {
                if(input is TextBox)
                {
                    TextBox textBox = ((TextBox)input);
                    textBox.Text = string.Empty;
                    textBox.BorderThickness = LostFocusThickness;
                    textBox.BorderBrush = Brushes.White;

                } else if(input is Border)
                {
                    Border border = ((Border)input);
                    border.BorderThickness = LostFocusThickness;
                    border.BorderBrush = Brushes.White;

                    ComboBox combobox = ((ComboBox)border.Child);
                    combobox.SelectedItem = null;

                } else if(input is DatePicker)
                {
                    DatePicker date = ((DatePicker)input);
                    date.BorderThickness = LostFocusThickness;
                    date.BorderBrush = Brushes.White;
                }

            }
        }

        private bool monumentIdExists(string id)
        {
            foreach(Monument monument in observ_monuments)
            {
                if(monument.ID.Equals(id))
                {
                    return true;
                }
            }

            return false;
        }


        private string getFileExtensionFromPath(string filePath)
        {
            return filePath.Substring(filePath.LastIndexOf('.'));
        }


        private bool radioButtonChecked(RadioButton positiveBtn)
        {
            //if the positive button is not checked, the negative is
           if(positiveBtn.IsChecked == true)
            {
                return true;
            }

            return false;
        }

                            /******************
                            * ON FOCUS EVENTS *
                            * ****************/

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            txt.Background = Brushes.Yellow;
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            txt.Background = Brushes.White;
        }

        private void comboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;

            ((Border)combo.Parent).BorderThickness = FocusThickness;
            ((Border)combo.Parent).BorderBrush = Brushes.Blue;
        }

        private void comboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;

            ((Border)combo.Parent).BorderThickness = LostFocusThickness;
            ((Border)combo.Parent).BorderBrush = Brushes.White;
        }



        private void Button_ClickCloseDisplayInfo(object sender, RoutedEventArgs e)
        {
            if (isNewMonumentWindowInfoShown)
            {

                DoubleAnimation double_anim = new DoubleAnimation
                {
                    From = 0,
                    To = -DisplayMonumentInfoHolder.Width,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AutoReverse = false
                };

                DisplayMonumentInfoHolder.BeginAnimation(Canvas.RightProperty, double_anim);
                isNewMonumentWindowInfoShown = false;
            }
        }



        /*********************************
         * METHODS FOR USER NOTIFICATION *
         *********************************/

        private void NotifyUser(string message)
        {
            if (!userNotificationTimer.IsEnabled)
            {
                DoubleAnimation da = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.55)),
                    AutoReverse = false
                };

                UserNotificationMessage.Text = message;
                UserNotificationMessage.BeginAnimation(OpacityProperty, da);
                userNotificationTimer.Tick += NotificationMessageTimeout;
                userNotificationTimer.Start();
            }
            else
            {
                userNotificationTimer.Stop();
                UserNotificationMessage.Text = message;
                userNotificationTimer.Tick += NotificationMessageTimeout;
                userNotificationTimer.Start();
            }
        }

        private void NotificationMessageTimeout(object sender, EventArgs e)
        {

            DoubleAnimation da = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.55)),
                AutoReverse = false
            };
            da.Completed += RemoveTextInNotification;
            UserNotificationMessage.BeginAnimation(OpacityProperty, da);

            (sender as DispatcherTimer).Stop();
        }

        private void RemoveTextInNotification(object sender, EventArgs e)
        {
            UserNotificationMessage.Text = "";
        }
    }
}
