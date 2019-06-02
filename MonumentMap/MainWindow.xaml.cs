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

        public Grid selectedMonument = null;

        /************************** Databinding objects **************************/
        public WindowConstants WindowConstants { get; set; }
        public CanvasPositions CanvasPositions { get; set; }

        /******** Booleans indicating whether pop-up windows are shown ********/
        public bool isNewMonumentWindowShown = false;
        public bool isNewMonumentWindowInfoShown = false;


        /***************** Observable collections *****************/
        public ObservableCollection<Monument> observ_monuments { get; set; }
        public ObservableCollection<MonumentType> observ_monum_types { get; set; }
        public ObservableCollection<MonumentTag> observ_monum_tags { get; set; }


        private double mainWindowHeight;
        private double mainWindowWidth;

        static BrushConverter brushConverter = new BrushConverter();

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

            worldMap.ViewChangeOnFrame += new EventHandler<MapEventArgs>(worldMap_ViewChangeOnFrame);
            this.SizeChanged += OnWindowSizeChanged;
            this.MouseMove += Window_OnMouseMove;
            this.MouseUp += Window_OnMouseUp;

            onLoad();
        }


        private void onLoad()
        {

            initializeGlobalObjects();

            WindowConstants = new WindowConstants();
            CanvasPositions = new CanvasPositions();


            if (observ_monuments != null)
            {
                foreach(Monument m in observ_monuments ) {
                    AddMonumentToMonumentsView(m);
                    if (m.monumentPin != null)
                    {
                        addMonumentPinToMap(m);
                    }
                }
            }

            /* Initializing font sizes */
            WindowConstants.HeaderFontSize = 20;
            WindowConstants.FormFontSize = 16; //do 18 je ok

            WindowConstants.RowSpacing = 18; //setting space between rows in grid

            /* Memorizing default monument icon and picture */
            DEFAULT_ICON = new BitmapImage(new Uri(monumentIcon.Source.ToString()));
            DEFAULT_PICTURE = new BitmapImage(new Uri(monumentPicture.Source.ToString()));

            //inserting enums to comboboxes
            climateType.ItemsSource = Enum.GetValues(typeof(ClimateType)).Cast<ClimateType>();
            touristStatus.ItemsSource = Enum.GetValues(typeof(TouristStatus)).Cast<TouristStatus>();

            initializeNoMonumentType();

        }


        private void initializeNoMonumentType()
        {

            foreach(MonumentType type in observ_monum_types)
            {
                if(type.ID.Equals("0"))
                {
                    return;
                }
            }

            MonumentType no_type = new MonumentType();
            no_type.ID = "0";
            no_type.Name = "No_type";
            no_type.Icon_path = "icons/MonumentIcon.png";
            observ_monum_types.Add(no_type);
        }

        private void initializeGlobalObjects()
        {
            WindowConstants = new WindowConstants();
            CanvasPositions = new CanvasPositions();

            StringBuilder sb = new StringBuilder();
            bool notLoaded = false;

            /* Loading collections from files */
            observ_monuments = IO_Serializer.deserializeMonuments();
            if (observ_monuments == null)
            {
                sb.Append("No monuments-");
                observ_monuments = new ObservableCollection<Monument>();
                notLoaded = true;
            }


            observ_monum_tags = IO_Serializer.deserializeMonumentTags();
            if (observ_monum_tags == null)
            {
                sb.Append("No tags-");
                observ_monum_tags = new ObservableCollection<MonumentTag>();
                notLoaded = true;
            }


            observ_monum_types = IO_Serializer.deserializeMonumentTypes();
            if(observ_monum_types == null)
            {
                sb.Append("No types");
                observ_monum_types = new ObservableCollection<MonumentType>();
                notLoaded = true;
            }

            sb.Append(" have been loaded");

            if(notLoaded)
            {
                NotifyUser(sb.ToString());
            }

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
            CanvasPositions.RemoveLeft = (mainWindowWidth / 2) - (RemoveMonumentGrid.Width / 2);

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

        /*
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



        worldMap.Children.Add(pin);
    } */

        public Monument selectedDisplayMonument = null;
        private void PinClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pushpin p = sender as Pushpin;
            selectedDisplayMonument = getMonumentById(p.Content.ToString());
            OpenDisplayInfo();
            ChangeDisplayInfo(selectedDisplayMonument);

            NotifyUser(p.Content.ToString());
        }

        private void ChangeDisplayInfo(Monument monument)
        {
            DisplayInfoName.Text = monument.Name;
            DisplayInfoDescription.Text = monument.Description;
            DisplayInfoType.Text = monument.Type != null ? monument.Type.ToString() : "No type";
            DisplayInfoClimate.Text = monument.Climate.ToString();
            DisplayInfoEcoEndangered.Text = monument.IsEcoEndangered ? "Yes" : "No";
            DisplayInfoInHumanSettlement.Text = monument.IsInSettlement ? "Yes" : "No";
            DisplayInfoHasEndangeredSpecies.Text = monument.ContainsEndangeredSpecies ? "Yes" : "No";
            DisplayInfoTouristStatus.Text = monument.TourStatus.ToString();
            DisplayInfoAnnualIncome.Text = monument.AnnualIncome.ToString();
            DisplayInfoDiscoveryDate.Text = monument.DateOfDiscovery;

            DisplayInfoImage.ImageSource = new BitmapImage(new Uri(monument.Picture_path, UriKind.Relative));
        }

        //postavljanje granice za zoom
        private void worldMap_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            double z = worldMap.ZoomLevel;

            //setting min zoom
            if (z > 16)
            {
                worldMap.ZoomLevel = 16;
            }

            //settin max zoom
            if (z < 3.6)
            {
                worldMap.ZoomLevel = 3.6;
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

                //setting monument type
                int colonID = monumentType.SelectedItem.ToString().IndexOf(':'); //used to extract id from string
                string typeID = monumentType.SelectedItem.ToString().Substring(0, colonID);

                monument.Type = findMonumentType(typeID);

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
                monument.DateOfDiscovery = discoveryDate.Text;


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

                    if (!typeID.Equals("0"))
                    {
                        //set the monument icon from monument type
                        if (monument.Type != null)
                        {
                            monument.Icon_path = monument.Type.Icon_path;
                        } else
                        {
                            monument.Icon_path = "icons/MonumentIcon.png";
                        }

                    } else
                    {
                        monument.Icon_path = "icons/MonumentIcon.png";
                    }


                } else
                {
                    //get source icon extension from path
                    string sourceExtension = getFileExtensionFromPath(newMonumentIconPath.Text);

                    //get destination path
                    string destFileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + sourceExtension; //to avoid duplicate names
                    string destinationPath = GetDestinationPath(destFileName, "icons");

                    //copy source image to local folder
                    File.Copy(newMonumentIconPath.Text, destinationPath);
                    monument.Icon_path = destinationPath;
                }

                AddMonumentToMonumentsView(monument);
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

            //check if the user selected a color
            if (tagColorCode.Text.Equals(""))
            {
                ColorPicker_Tag.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                ColorPicker_Tag.BorderBrush = Brushes.Transparent;
            }


            if (!checkForEmptyFields(newTagGrid)) {

                if(findMonumentTag(tagID.Text) != null)
                {
                    MessageBox.Show("ID already exists");
                    return;
                }

                MonumentTag tag = new MonumentTag();

                tag.ID = tagID.Text;
                tag.Description = tagDescr.Text;
                tag.Color = tagColorCode.Text;

                observ_monum_tags.Add(tag);
                closeTagWindow();

                //serializing
                if(IO_Serializer.serializeMonumentTags(observ_monum_tags))
                {
                    NotifyUser("Tag successfully added");
                } else
                {
                    NotifyUser("Failed to save tags");
                }

            }


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

            if(!checkForEmptyFields(newMonumTypeGrid))
            {

                if(monumentTypeIconPath.Text.Equals(""))
                {
                    //TODO: manje seljacko obavestenje
                    MessageBox.Show("Please select icon");
                    return;
                }

                if(findMonumentType(monumentTypeID.Text) != null)
                {
                    MessageBox.Show("ID already exists");
                    return;
                }

                MonumentType type = new MonumentType();

                type.ID = monumentTypeID.Text;
                type.Name = monumentTypeName.Text;
                type.Description = monumentTypeDescr.Text;

                /** Uploading icon **/

                //get source icon extension from path
                string sourceExtension = getFileExtensionFromPath(monumentTypeIconPath.Text);

                //get destination path
                string destFileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + sourceExtension; //to avoid duplicate names
                string destinationPath = GetDestinationPath(destFileName, "monument_type_icons");

                //copy source image to local folder
                File.Copy(monumentTypeIconPath.Text, destinationPath);
                type.Icon_path = destinationPath;

                closeMonumentTypeDialog();
                observ_monum_types.Add(type);

                if(IO_Serializer.serializeMonumentTypes(observ_monum_types))
                {
                    NotifyUser("New monument type added");
                } else
                {
                    NotifyUser("Failed to save monument types");
                }

            }

        }

        private void cancelMonumentTypeBtn_Click(object sender, RoutedEventArgs e)
        {

            closeMonumentTypeDialog();

        }

        private void closeMonumentTypeDialog()
        {
            clearInputs(newMonumTypeGrid);
            newMonumTypeGridHolder.Visibility = Visibility.Hidden;
        }

        private void addTagToMonumentBtn_Click(object sender, RoutedEventArgs e)
        {

            //TODO: proveri ima li praznih polja


        }


        private void cancelTagBtn_Click(object sender, RoutedEventArgs e)
        {

            closeTagWindow();
        }


        private void closeTagWindow()
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
                        textBox.BorderBrush = (Brush)brushConverter.ConvertFrom("#540a0a");
                        isEmpty = true;
                    } else
                    {
                        textBox.BorderBrush = Brushes.Transparent;
                    }

                } else if(input is Border)
                {
                    Border border = ((Border)input);

                    if(border.Child is ComboBox)
                    {
                        ComboBox comboBox = ((ComboBox)border.Child);

                        if (comboBox.SelectedItem == null)
                        {

                            isEmpty = true;

                            //make border red
                            border.BorderBrush = (Brush)brushConverter.ConvertFrom("#540a0a");
                        }
                        else
                        {
                            //make border default
                            border.BorderBrush = Brushes.Transparent;
                        }
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
                    textBox.BorderBrush = Brushes.Transparent;
    
                } else if(input is Border)
                {
                    Border border = ((Border)input);
                    border.BorderBrush = Brushes.Transparent;

                    if(border.Child is ComboBox)
                    {
                        ComboBox combobox = ((ComboBox)border.Child);
                        combobox.SelectedItem = null;
                    }

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


        private MonumentType findMonumentType(string id)
        {

            foreach (MonumentType type in observ_monum_types)
            {
                if (type.ID.Equals(id))
                {
                    return type;
                }
            }

            return null;
        }

        private Monument getMonumentById(string id)
        {
            foreach (Monument monument in observ_monuments)
            {
                if (monument.ID.Equals(id))
                {
                    return monument;

                }
            }

            return null;

        }


        private MonumentTag findMonumentTag(string id)
        {
            foreach (MonumentTag tag in observ_monum_tags)
            {
                if (tag.ID.Equals(id))
                {
                    return tag;
                }
            }

            return null;
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


        private void searchClicked(object sender, MouseButtonEventArgs e)
        {
            searchTextBox.Text = string.Empty;
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            txt.Background = (Brush)brushConverter.ConvertFrom("#0a3f54");
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            txt.Background = (Brush)brushConverter.ConvertFrom("#093647");
        }

        private void comboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            ((Border)combo.Parent).BorderBrush = (Brush)brushConverter.ConvertFrom("#0a3f54");
        }

        private void comboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            ((Border)combo.Parent).BorderBrush = (Brush)brushConverter.ConvertFrom("#093647"); ;
        }


        private void OpenDisplayInfo()
        {
            if (!isNewMonumentWindowInfoShown)
            {

                DoubleAnimation double_anim = new DoubleAnimation
                {
                    From = -DisplayMonumentInfoHolder.Width,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AutoReverse = false
                };

                DisplayMonumentInfoHolder.BeginAnimation(Canvas.RightProperty, double_anim);
                isNewMonumentWindowInfoShown = true;
            }
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

                    /* ***********************
                     * DRAG AND DROP METHODS *
                     * ********************* */

        private void GridMonument_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            Grid child = grid.Children[0] as Grid;
            var bg = new SolidColorBrush();
            Color color = (Color)ColorConverter.ConvertFromString("#093647");
            bg.Opacity = 0.2;
            bg.Color = color;
            child.Background = bg;
        }

        private void GridMonument_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            Grid child = grid.Children[0] as Grid;
            var bg = new SolidColorBrush();
            Color color = (Color)ColorConverter.ConvertFromString("#093647");
            bg.Opacity = 0.5;
            bg.Color = color;
            child.Background = bg;
        }


        private Monument selectedMonumentObject = null;
        private void GridMonument_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedMonument = (Grid) sender;
            selectedMonumentObject = getMonumentById(selectedMonument.Tag.ToString());

            DoubleAnimation anim = new DoubleAnimation
            {
                From = -50,
                To = 100,
                Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                AutoReverse = false
            };
            var ease = new BackEase();
            ease.EasingMode = EasingMode.EaseInOut;
            anim.EasingFunction = ease;
            RemoveMonumentGrid.BeginAnimation(Canvas.TopProperty, anim);


            var imgSource = new BitmapImage(new Uri(selectedMonumentObject.Icon_path, UriKind.Relative));
            ImageBrush img = new ImageBrush();
            img.ImageSource = imgSource;
            img.Stretch = Stretch.UniformToFill;
            CursorIcon.Background = img;
            CursorIcon.Width = 80;
            CursorIcon.Height = 80;
        }

        private void Window_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (selectedMonument != null)
            {
                this.Cursor = Cursors.Hand;
                CursorIcon.Visibility = Visibility.Visible;


                var pos = e.GetPosition(this);
                Canvas.SetLeft(CursorIcon, pos.X + 10);
                Canvas.SetTop(CursorIcon, pos.Y + 10);
            }
            else
            {
                this.Cursor = null;
                CursorIcon.Visibility = Visibility.Hidden;
            }

            if (RemoveMonumentGrid.IsMouseOver)
            {
                Brush col = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7a1616"));
                RemoveMonumentGrid.Background = col;
            }
            else
            {
                Brush col = (SolidColorBrush)(new BrushConverter().ConvertFrom("#093647"));
                RemoveMonumentGrid.Background = col;
            }

        }


        private void addMonumentPinToMap(Monument monument)
        {
            BitmapImage imgSource = new BitmapImage(new Uri(monument.Icon_path, UriKind.Relative));
            ControlTemplate tmp = new ControlTemplate(typeof(Pushpin));
            FrameworkElementFactory fact = new FrameworkElementFactory(typeof(Image));
            fact.SetValue(Image.SourceProperty, imgSource);
            fact.SetValue(Image.WidthProperty, 45.0);
            fact.SetValue(Image.StretchProperty, Stretch.UniformToFill);
            tmp.VisualTree = fact;

            Location location = new Location(monument.monumentPin.latitude, monument.monumentPin.longitude);


            Pushpin pin = new Pushpin();
            pin.Template = tmp;
            pin.Location = location;
            pin.Content = monument.ID;

            pin.MouseDown += PinClicked;

            worldMap.Children.Add(pin);


        }

        private void Window_OnMouseUp(object sender, MouseButtonEventArgs e)
        {

            if (worldMap.IsMouseOver && selectedMonument != null)
            {
                if (!PinExistsOnMap(selectedMonument.Tag.ToString()))
                {
                    e.Handled = true;

                    Point mousePosition = e.GetPosition(this);
                    mousePosition.Y = mousePosition.Y - 30;
                    mousePosition.X = mousePosition.X - 3;
                    Location pinLocation = worldMap.ViewportPointToLocation(mousePosition);
                    selectedMonumentObject.monumentPin = new MonumentPin(pinLocation);
                    BitmapImage imgSource = new BitmapImage(new Uri(selectedMonumentObject.Icon_path, UriKind.Relative));
                    //ImageBrush img = new ImageBrush(imgSource);

                    ControlTemplate tmp = new ControlTemplate(typeof(Pushpin));
                    FrameworkElementFactory fact = new FrameworkElementFactory(typeof(Image));
                    fact.SetValue(Image.SourceProperty, imgSource);
                    fact.SetValue(Image.WidthProperty, 45.0);
                    fact.SetValue(Image.StretchProperty, Stretch.UniformToFill);
                    tmp.VisualTree = fact;

                    Pushpin pin = new Pushpin();
                    pin.Template = tmp;
                    pin.Location = pinLocation;
                    pin.Content = selectedMonument.Tag.ToString();

                    pin.MouseDown += PinClicked;

                    worldMap.Children.Add(pin);
                }
                else
                {
                    NotifyUser("That monument is already placed on map");
                }
            }

            if (RemoveMonumentGrid.IsMouseOver && selectedMonument != null)
            {
                RemoveMonument();

            }
            if (selectedMonument != null)
            {
                DoubleAnimation anim = new DoubleAnimation
                {
                    From = 100,
                    To = -50,
                    Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                    AutoReverse = false,

                };
                var ease = new BackEase();
                ease.EasingMode = EasingMode.EaseInOut;
                anim.EasingFunction = ease;

                RemoveMonumentGrid.BeginAnimation(Canvas.TopProperty, anim);
            }

            IO_Serializer.serializeMonuments(observ_monuments);

            selectedMonumentObject = null;
            selectedMonument = null;
        }

        private void RemoveMonument()
        {
            MonumentsStackPanel.Children.Remove(selectedMonument); //deletes monument from view
            Pushpin pin = GetPinFromMapById(selectedMonument.Tag.ToString());
            if (pin != null)
            {
                worldMap.Children.Remove(pin);
            }

            observ_monuments.Remove(selectedMonumentObject);
            IO_Serializer.serializeMonuments(observ_monuments);

        }

        private bool PinExistsOnMap(string id)
        {
            foreach(Pushpin p in worldMap.Children){
                if (((string)p.Content).Equals(id))
                {
                    return true;
                }
            }

            return false;
        }

        private Pushpin GetPinFromMapById(string id)
        {
            foreach(Pushpin p in worldMap.Children) {
                if (id.Equals(p.Content.ToString()))
                {
                    return p;
                }
            }

            return null;
        }

        private void AddMonumentToMonumentsView(Monument monument)
        {
            var mainGrid = new Grid();
            mainGrid.Height = 160;
            mainGrid.Margin = new Thickness(0, 5, 0, 5);
            (mainGrid as UIElement).MouseEnter += GridMonument_MouseEnter;
            (mainGrid as UIElement).MouseLeave += GridMonument_MouseLeave;
            (mainGrid as UIElement).MouseDown += GridMonument_MouseDown;


            var mainBrush = new ImageBrush();
            mainBrush.Stretch = Stretch.UniformToFill;
            mainBrush.ImageSource = new BitmapImage(new Uri(monument.Picture_path, UriKind.Relative));
            mainGrid.Background = mainBrush;

            var childGrid = new Grid();

            Color color = (Color)ColorConverter.ConvertFromString("#093647");
            var secondBrush = new SolidColorBrush();
            secondBrush.Opacity = 0.5;
            secondBrush.Color = color;
            childGrid.Background = secondBrush;


            var imageGrid = new Grid();
            BitmapImage iconSource = new BitmapImage(new Uri(monument.Icon_path, UriKind.Relative));
            ImageBrush iconImage = new ImageBrush();
            iconImage.ImageSource = iconSource;
            iconImage.Stretch = Stretch.UniformToFill;
            imageGrid.Background = iconImage;
            imageGrid.Width = 80;
            imageGrid.Height = 80;


            childGrid.Children.Add(imageGrid);
            mainGrid.Tag = monument.ID; //setting tag as id of monument
            mainGrid.Children.Add(childGrid);

            MonumentsStackPanel.Children.Add(mainGrid);


        }


                    /******************
                     * DELETE METHODS *
                     * ****************/

        private bool deleteTag(string tagID)
        {

            MonumentTag tag = findMonumentTag(tagID);

            if(tag != null)
            {
                return observ_monum_tags.Remove(tag);
            }

            return false;

        }


        private bool deleteMonumentType(string typeID)
        {

            MonumentType type = findMonumentType(typeID);

            if(type != null)
            {
                return observ_monum_types.Remove(type);
            }

            return false;

        }


    }
}
