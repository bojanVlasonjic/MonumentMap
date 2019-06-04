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
        public bool isSearchResultsShown = false;
        public bool isTagDialogShown = false;
        public bool isTypeDialogShown = false;

        /***************** Observable collections *****************/
        public ObservableCollection<Monument> observ_monuments { get; set; }
        public ObservableCollection<MonumentType> observ_monum_types { get; set; }
        public ObservableCollection<MonumentTag> observ_monum_tags { get; set; }


        private double mainWindowHeight;
        private double mainWindowWidth;

        static BrushConverter brushConverter = new BrushConverter();

        static string BROWSE_ICONS_FILTER = "Icon Files (*.png, *.jpg, *.svg, *.eps, *.psd)|*.png;*.jpg;*.svg;*.eps;*.psd";
        static string BROWSE_PICS_FILTER = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files(*.*)|*.*";


        private const string DEFAULT_TYPE_ID = "0";
        private const string DEFAULT_TAG_ID = "No_tag";


        //default picture and icon
        private BitmapImage DEFAULT_ICON;
        private BitmapImage DEFAULT_PICTURE;

        private bool editingMonument = false;

        private int FONT_SIZE_CHANGE = 0;
        public string FONT_SIZE_TEXT { get; set; }
        private const int MAX_FONT_SIZE = 5;


        static IOSerializer IO_Serializer = new IOSerializer();
        static Utility utility = new Utility();


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

            updateFontSizeText();
        }

        private void updateFontSizeText()
        {
            FontSizeIndicator.Text = "+" + FONT_SIZE_CHANGE.ToString();
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

            initializeDefaultMonumentType();
            initializeDefaultTag();


        }


        private void initializeDefaultMonumentType()
        {

            foreach(MonumentType type in observ_monum_types)
            {
                if(type.ID.Equals(DEFAULT_TYPE_ID))
                {
                    return;
                }
            }

            MonumentType no_type = new MonumentType();
            no_type.ID = DEFAULT_TYPE_ID;
            no_type.Name = "No_type";
            no_type.Icon_path = "icons/MonumentIcon.png";
            observ_monum_types.Add(no_type);
        }


        private void initializeDefaultTag()
        {
            foreach(MonumentTag tag in observ_monum_tags)
            {
                if(tag.ID.Equals(DEFAULT_TAG_ID))
                {
                    return;
                }
            }

            MonumentTag defaultTag = new MonumentTag();

            defaultTag.ID = DEFAULT_TAG_ID;
            defaultTag.Description = "Default tag";
            defaultTag.Color = Brushes.White.ToString();

            observ_monum_tags.Add(defaultTag);
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
            CanvasPositions.FindOnMapLeft = (mainWindowWidth / 2) - (RemoveMonumentGrid.Width / 2) + 130;

            //centerPopUpWindows();
        }

        /*********************
        * REGULAR EXPRESSIONS *
        * *******************/

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        /*********************
        * MAP EVENT HANDLERS *
        * *******************/

        public Monument selectedDisplayMonument = null;
        private void PinClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pushpin p = sender as Pushpin;
            selectedDisplayMonument = getMonumentById(p.Content.ToString());
            OpenDisplayInfo();
            ChangeDisplayInfo(selectedDisplayMonument);
            
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
            DisplayInfoTag.Text = monument.Tags[0].ID;

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

                editingMonument = false;

                //changing heading and button text
                addMonumentBtn.Content = "Add monument";
                newMonumentHeading.Content = "Add monument";

                monumentID.IsEnabled = true;
            }
        }

        private void newMonumentBtn_Click(object sender, RoutedEventArgs e)
        {
            openNewMonumentWindow();
        }


        private void openNewMonumentWindow()
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
                if(monumentIdExists(monumentID.Text) && !editingMonument)
                {
                    MessageBox.Show("Monument id already exists");
                    return;
                }

                Monument monument;

                if(!editingMonument)
                {
                    monument = new Monument();
                } else
                {
                    monument = getMonumentById(monumentID.Text);
                }

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

                //parsing date
                monument.DateOfDiscovery = discoveryDate.Text;

                //initializing tag list
                monument.Tags = new List<MonumentTag>();
                MonumentTag selectedTag = findMonumentTag(newMonumentTag.SelectedItem.ToString());

                if(selectedTag != null)
                {
                    monument.Tags.Add(selectedTag);
                } else
                {
                    monument.Tags.Add(findMonumentTag(DEFAULT_TAG_ID)); //put default tag in case something wen't wrong
                }

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

                    if (!typeID.Equals(DEFAULT_TYPE_ID))
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

                string message;

                if(!editingMonument)
                {
                    AddMonumentToMonumentsView(monument);
                    observ_monuments.Add(monument);
                    message = "Monument added";
                } else
                {
                    //TODO: za losmija, promeniti sliku i ikonicu
                    Grid grid = GetMonumentGrid(monument.ID);

                    var mainBrush = new ImageBrush();
                    mainBrush.Stretch = Stretch.UniformToFill;
                    mainBrush.ImageSource = new BitmapImage(new Uri(monument.Picture_path, UriKind.Relative));
                    grid.Background = mainBrush;

                    var iconBrush = new ImageBrush();
                    iconBrush.Stretch = Stretch.UniformToFill;
                    iconBrush.ImageSource = new BitmapImage(new Uri(monument.Icon_path, UriKind.Relative));
                    ((grid.Children[0] as Grid).Children[0] as Grid).Background = iconBrush;


                    if (monument.monumentPin != null)
                    {
                        Pushpin pin = GetPinFromMapById(monument.ID);
                        worldMap.Children.Remove(pin);

                        addMonumentPinToMap(monument);
                    }


                    ChangeDisplayInfo(monument);
                    //replacing existing monument with the edited one
                    observ_monuments = utility.replaceMonument(observ_monuments, monument);
                    message = "Monument edited";
                }

                closeNewMonumentWindow();

                if(IO_Serializer.serializeMonuments(observ_monuments))
                {
                    NotifyUser(message);
                } else
                {
                    NotifyUser("Failed to save monuments");
                }

            }
        }

        private void addTagBtn_Click(object sender, RoutedEventArgs e)
        {


            if (!checkForEmptyFields(newTagGrid)) {

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

                //check whether id exists
                if (findMonumentTag(tagID.Text) != null)
                {
                    MessageBox.Show("ID already exists");
                    return;
                }

                //creating tag
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
            isTagDialogShown = true;

        }

        private void newMonumentTypeBtn_Click(object sender, RoutedEventArgs e)
        {

            newMonumTypeGridHolder.Visibility = Visibility.Visible;
            isTypeDialogShown = true;
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

            //clearing text for selected icon
            monumentTypeIconName.Text = "";

            newMonumTypeGridHolder.Visibility = Visibility.Hidden;

            isTypeDialogShown = false;
        }


        private void cancelTagBtn_Click(object sender, RoutedEventArgs e)
        {

            closeTagWindow();
        }


        private void closeTagWindow()
        {
            clearInputs(newTagGrid);

            ColorPicker_Tag.SelectedColor = null;

            newTagGridHolder.Visibility = Visibility.Hidden;

            isTagDialogShown = false;
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

                    if(border.Child is Button)
                    {
                        continue;
                    }

                    border.BorderBrush = Brushes.Transparent;

                    if(border.Child is ComboBox)
                    {
                        ComboBox combobox = ((ComboBox)border.Child);
                        combobox.SelectedItem = null;
                    }

                } else if(input is ComboBox)
                {
                    ((ComboBox)input).SelectedItem = null;
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
            ((Border)combo.Parent).BorderBrush = Brushes.White;
        }

        private void comboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            ((Border)combo.Parent).BorderBrush = (Brush)brushConverter.ConvertFrom("#093647");
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

            closeDisplayInfoWindow();
        }


        private void closeDisplayInfoWindow()
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

        private void ShowSearchResults()
        {

            if (isSearchResultsShown) return;
            SearchCancelButton.Visibility = Visibility.Visible;
            DoubleAnimation double_anim = new DoubleAnimation
            {
                From = -SearchGrid.Width,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                AutoReverse = false
            };

            SearchGrid.BeginAnimation(Canvas.RightProperty, double_anim);
            isSearchResultsShown = true;


        }

        private void CloseSearchResults()
        {
            if (!isSearchResultsShown) return;
            SearchCancelButton.Visibility = Visibility.Hidden;
            DoubleAnimation double_anim = new DoubleAnimation
            {
                From = 0,
                To = -SearchGrid.Width,
                Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                AutoReverse = false
            };

            SearchGrid.BeginAnimation(Canvas.RightProperty, double_anim);
            isSearchResultsShown = false;
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
            Color color = Colors.Black;
            bg.Opacity = 0.0;
            bg.Color = color;
            child.Background = bg;
        }

        private void GridMonument_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            Grid child = grid.Children[0] as Grid;
            var bg = new SolidColorBrush();
            Color color = Colors.Black;
            bg.Opacity = 0.3;
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

            if (selectedMonumentObject.monumentPin != null)
            {
                SearchMonumentGrid.BeginAnimation(Canvas.TopProperty, anim);
            }


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


            if (SearchMonumentGrid.IsMouseOver)
            {
                Brush col = (SolidColorBrush)(new BrushConverter().ConvertFrom("#206d3a"));
                SearchMonumentGrid.Background = col;
            }
            else
            {
                Brush col = (SolidColorBrush)(new BrushConverter().ConvertFrom("#072530"));
                SearchMonumentGrid.Background = col;
            }

        }


        private void addMonumentPinToMap(Monument monument)
        {
            BitmapImage imgSource = new BitmapImage(new Uri(monument.Icon_path, UriKind.Relative));
            ControlTemplate tmp = new ControlTemplate(typeof(Pushpin));
            FrameworkElementFactory fact = new FrameworkElementFactory(typeof(Image));
            fact.SetValue(Image.SourceProperty, imgSource);
            fact.SetValue(Image.WidthProperty, 50.0);
            fact.SetValue(Image.HeightProperty, 50.0);
            fact.SetValue(Image.StretchProperty, Stretch.UniformToFill);
            tmp.VisualTree = fact;

            Location location = new Location(monument.monumentPin.latitude, monument.monumentPin.longitude);


            Pushpin pin = new Pushpin();
            pin.Width = 50;
            pin.Height = 50;
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
                if (PinExistsOnMap(selectedMonument.Tag.ToString()))
                {
                    Pushpin pinToRemove = GetPinFromMapById(selectedMonument.Tag.ToString());

                    if(pinToRemove != null)
                    {
                        worldMap.Children.Remove(pinToRemove); //removing pin from map
                    }

                }

                //adding pin to map

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
                fact.SetValue(Image.WidthProperty, 50.0);
                fact.SetValue(Image.HeightProperty, 50.0);
                fact.SetValue(Image.StretchProperty, Stretch.UniformToFill);
                tmp.VisualTree = fact;

                Pushpin pin = new Pushpin();
                pin.Width = 50;
                pin.Height = 50;
                pin.Template = tmp;
                pin.Location = pinLocation;
                pin.Content = selectedMonument.Tag.ToString();

                pin.MouseDown += PinClicked;

                worldMap.Children.Add(pin);

            }

            if (RemoveMonumentGrid.IsMouseOver && selectedMonument != null)
            {
                RemoveMonument();
            }

            if (SearchMonumentGrid.IsMouseOver && selectedMonument != null)
            {
                worldMap.Center = new Location(selectedMonumentObject.monumentPin.latitude, selectedMonumentObject.monumentPin.longitude);
                worldMap.ZoomLevel = 16;
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
                if (selectedMonumentObject.monumentPin != null)
                {
                    SearchMonumentGrid.BeginAnimation(Canvas.TopProperty, anim);
                }

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
            (mainGrid as UIElement).MouseUp += GridMonument_MouseUp;


            var mainBrush = new ImageBrush();
            mainBrush.Stretch = Stretch.UniformToFill;
            mainBrush.ImageSource = new BitmapImage(new Uri(monument.Picture_path, UriKind.Relative));
            mainGrid.Background = mainBrush;

            var childGrid = new Grid();

            Color color = Colors.Black;
            var secondBrush = new SolidColorBrush();
            secondBrush.Opacity = 0.3;
            secondBrush.Color = color;
            childGrid.Background = secondBrush;


            var imageGrid = new Grid();
            BitmapImage iconSource = new BitmapImage(new Uri(monument.Icon_path, UriKind.Relative));
            ImageBrush iconImage = new ImageBrush();
            iconImage.ImageSource = iconSource;
            iconImage.Stretch = Stretch.UniformToFill;
            imageGrid.Background = iconImage;
            imageGrid.Width = 62;
            imageGrid.Height = 62;


            childGrid.Children.Add(imageGrid);
            mainGrid.Tag = monument.ID; //setting tag as id of monument
            mainGrid.Children.Add(childGrid);

            MonumentsStackPanel.Children.Add(mainGrid);


        }

        private void GridMonument_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedDisplayMonument = getMonumentById(selectedMonument.Tag.ToString());
            OpenDisplayInfo();
            ChangeDisplayInfo(selectedDisplayMonument);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void monumentTypeSelected(object sender, SelectionChangedEventArgs e)
        {

            if(monumentType.SelectedItem == null)
            {
                return;
            }

            //if the user selected the icon then don't change icon based on type
            if(!newMonumentIconPath.Text.Equals(""))
            {
                return;
            }

            //extract type ID from selected item
            int colonID = monumentType.SelectedItem.ToString().IndexOf(':'); //used to extract id from string
            string typeID = monumentType.SelectedItem.ToString().Substring(0, colonID);

            //if the selected type isn't no type
            if(!typeID.Equals(DEFAULT_TYPE_ID))
            {
                MonumentType type = findMonumentType(typeID);

                if(typeID != null)
                {
                    //change the default icon to type
                    monumentIcon.Source = new BitmapImage(new Uri(type.Icon_path));
                }
            } else
            {
                monumentIcon.Source = DEFAULT_ICON;
            }

        }


                    /******************
                     * DELETE METHODS *
                     * ****************/

        private bool deleteTag(string tagID)
        {

            MonumentTag tag = findMonumentTag(tagID);

            if (tag != null)
            {
                return observ_monum_tags.Remove(tag);
            }

            return false;

        }


        private bool deleteMonumentType(string typeID)
        {

            MonumentType type = findMonumentType(typeID);

            if (type != null)
            {
                return observ_monum_types.Remove(type);
            }

            return false;

        }


        private bool deleteMonument(string id)
        {

            Monument monumToDelete = getMonumentById(id);

            if(monumToDelete != null)
            {
                return observ_monuments.Remove(monumToDelete);
            }

            return false;

        }

        private void DeleteMonumentTypeButton_Click(object sender, RoutedEventArgs e)
        {

            if(DeleteMonumentTypeCombobox.SelectedItem == null)
            {
                MessageBox.Show("Please select type you want to delete");
                return;
            }

            //extract type ID from selected item
            int colonID = DeleteMonumentTypeCombobox.SelectedItem.ToString().IndexOf(':'); //used to extract id from string
            string typeID = DeleteMonumentTypeCombobox.SelectedItem.ToString().Substring(0, colonID);

            if(typeID.Equals(DEFAULT_TYPE_ID))
            {
                MessageBox.Show("You can't delete a default type");
                return;
            }


            MonumentType typeToDel = findMonumentType(typeID);

            if(typeToDel != null)
            {

                //check whether a monument with type to delete already exists
                if(utility.isMonumentTypeUsed(typeToDel.ID, observ_monuments))
                {
                    MessageBox.Show("Can't delete type used by monument");
                    return;
                }

                //deleting monument type
                observ_monum_types.Remove(typeToDel);
                IO_Serializer.serializeMonumentTypes(observ_monum_types);
                MessageBox.Show("Deleted");
            }

        }

        private void DeleteMonumentTagButton_Click(object sender, RoutedEventArgs e)
        {

            if(tagsToDelete.SelectedItem == null)
            {
                MessageBox.Show("Please select tag in order to delete it");
                return;
            }

            if(tagsToDelete.SelectedItem.ToString().Equals(DEFAULT_TAG_ID))
            {
                MessageBox.Show("You can't delete a default tag");
                return;
            }

            MonumentTag tagToDel = findMonumentTag(tagsToDelete.SelectedItem.ToString());

            if(tagToDel != null)
            {

                if(utility.isMonumentTagUsed(tagToDel.ID, observ_monuments))
                {
                    MessageBox.Show("Can't delete tag used by monument");
                    return;
                }

                observ_monum_tags.Remove(tagToDel);
                IO_Serializer.serializeMonumentTags(observ_monum_tags);
                MessageBox.Show("Deleted");
            }

        }

        private void comboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ComboBox box = sender as ComboBox;

            if(box.Parent is Border)
            {
                ((Border)box.Parent).BorderBrush = Brushes.White;
            }
        }

        private void comboBox_MouseLeave(object sender, MouseEventArgs e)
        {

            ComboBox box = sender as ComboBox;

            if (box.Parent is Border)
            {
                ((Border)box.Parent).BorderBrush = Brushes.Transparent;
            }

        }

        private void radioBtnGotFocus(object sender, RoutedEventArgs e)
        {
            ((RadioButton)sender).BorderBrush = Brushes.White;
        }

        private void radioBtnLostFocus(object sender, RoutedEventArgs e)
        {
            ((RadioButton)sender).BorderBrush = (Brush)brushConverter.ConvertFrom("#093647");
        }

        private void editMonumentBtn_Click(object sender, RoutedEventArgs e)
        {


            if(selectedDisplayMonument != null)
            {
                var monumentE = getMonumentById(selectedDisplayMonument.ID);

                if (monumentE == null)
                {
                    NotifyUser("Monument has been deleted");
                    return;
                }

                editingMonument = true;
                closeDisplayInfoWindow();

                //displaying values to textboxes
                monumentID.Text = selectedDisplayMonument.ID;
                monumentName.Text = selectedDisplayMonument.Name;
                monumentDescr.Text = selectedDisplayMonument.Description;
                annualIncome.Text = selectedDisplayMonument.AnnualIncome.ToString();
                discoveryDate.Text = selectedDisplayMonument.DateOfDiscovery;

                //displaying values to enum binded comboboxes
                climateType.SelectedIndex = (int)selectedDisplayMonument.Climate;
                touristStatus.SelectedIndex = (int)selectedDisplayMonument.TourStatus;

                //displaying values to observable collection binded comboboxes
                int index; //index of combobox element

                index = getComboBoxItemIndex(newMonumentTag.Items, selectedDisplayMonument.Tags[0].ID);
                if(index != -1)
                {
                    newMonumentTag.SelectedIndex = index;
                }

                index = getComboBoxItemIndex(monumentType.Items, selectedDisplayMonument.Type.ToString());
                if (index != -1)
                {
                    monumentType.SelectedIndex = index;
                }

                //checking radio buttons
                checkRadioButton(selectedDisplayMonument.IsEcoEndangered, radioEcoPosBtn, radioEcoNegBtn);
                checkRadioButton(selectedDisplayMonument.IsInSettlement, radioSettlementPosBtn, radioSettlementNegBtn);
                checkRadioButton(selectedDisplayMonument.ContainsEndangeredSpecies, radioSpeciesPosBtn, radioSpeciesNegBtn);

                //adding values to hidden fields for icon path and picture path
                newMonumentIconPath.Text = selectedDisplayMonument.Icon_path;
                newMonumentPicturePath.Text = selectedDisplayMonument.Picture_path;

                //changing icon and picture
                try
                {
                    monumentPicture.Source = new BitmapImage(new Uri(selectedDisplayMonument.Picture_path));
                }
                catch (UriFormatException)
                {
                    newMonumentPicturePath.Text = string.Empty; //leaving default pic
                }

                try
                {
                    monumentIcon.Source = new BitmapImage(new Uri(selectedDisplayMonument.Icon_path));
                }
                catch (UriFormatException)
                {
                    newMonumentIconPath.Text = string.Empty; //leaving default icon
                }

                //disabling id field
                monumentID.IsEnabled = false;

                //changing heading and button text
                addMonumentBtn.Content = "Save changes";
                newMonumentHeading.Content = "Edit monument";

                openNewMonumentWindow();

            }
        }


        private void checkRadioButton(bool condition, RadioButton positiveBtn, RadioButton negativeBtn)
        {

            if(condition)
            {
                positiveBtn.IsChecked = true;
            } else
            {
                negativeBtn.IsChecked = true;
            }

        }

        private int getComboBoxItemIndex(ItemCollection items, string itemToFind)
        {

            for(int i = 0; i < items.Count; i++)
            {

                if(items[i].ToString().Equals(itemToFind))
                {
                    return i;
                }

            }

            return -1;

        }


        /***********************
         * Font manage methods *
         * **********************/

        private void increaseFontBtn_Click(object sender, RoutedEventArgs e)
        {
            increaseAllFonts();
        }

        private void increaseAllFonts()
        {
            if (FONT_SIZE_CHANGE >= MAX_FONT_SIZE)
            {
                NotifyUser("Maximum font reached");
                return;
            }

            changeFontSize(1);
            FONT_SIZE_CHANGE++;
            
            updateFontSizeText();

        }

        private void decreaseFontBtn_Click(object sender, RoutedEventArgs e)
        {
            decreaseAllFonts();
        }

        private void decreaseAllFonts()
        {
            if (FONT_SIZE_CHANGE <= 0)
            {
                NotifyUser("Minimum font reached");
                return;
            }

            changeFontSize(-1);
            FONT_SIZE_CHANGE--;
            updateFontSizeText();
        }


        private void changeFontSize(int factor)
        {
            foreach (TextBlock tb in FindVisualChildren<TextBlock>(this))
            {
                tb.FontSize = tb.FontSize + factor;
            }
            foreach (TextBox tb in FindVisualChildren<TextBox>(this))
            {
                tb.FontSize = tb.FontSize + factor;
            }
            foreach (ComboBox cb in FindVisualChildren<ComboBox>(this))
            {
                cb.FontSize = cb.FontSize + factor;
            }
        }



        /*******************
         * COMMAND METHODS *
         * ****************/

        private void AddNewMonument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            openNewMonumentWindow();
        }

        private void AddNewType_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            newMonumTypeGridHolder.Visibility = Visibility.Visible;
            isTypeDialogShown = true;
        }

        private void AddNewTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            newTagGridHolder.Visibility = Visibility.Visible;
            isTagDialogShown = true;
        }

        private void IncreaseFont_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            increaseAllFonts();
        }

        private void DecreaseFont_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            decreaseAllFonts();
        }

        private void Search_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            searchTextBox.Focus();
        }

        private void ClosePopUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {

           if(isTypeDialogShown)
            {
                closeMonumentTypeDialog();
            }

           if(isTagDialogShown)
            {
                closeTagWindow();
            }

        }


        /*******************
         * SEARCH METHODS *
         * ****************/

        private void SearchMonumentsDisplay()
        {

            if (searchTextBox == null) return;

            string val = searchTextBox.Text.ToLower();
            string filterVal = FilterSearch.Text.ToLower();
            SearchResultsDisplay.Children.Clear();

            if (val.Equals(""))
            {
                foreach(Monument m in observ_monuments)
                {
                    addElementsToSearchDisplay(m);
                }
            }

            else
            {
                foreach(Monument m in observ_monuments)
                {
                    if (filterVal.Equals("id"))
                    {
                        if (m.ID.ToLower().Contains(val))
                        {
                            addElementsToSearchDisplay(m);
                        }
                    }

                    else if (filterVal.Equals("name"))
                    {
                        if (m.Name.ToLower().Contains(val))
                        {
                            addElementsToSearchDisplay(m);
                        }
                    }
                    else if (filterVal.Equals("type"))
                    {
                        if (m.Type.ToString().ToLower().Contains(val))
                        {
                            addElementsToSearchDisplay(m);
                        }
                    }
                    else if (filterVal.Equals("climate"))
                    {
                        if (m.Climate.ToString().ToLower().Contains(val))
                        {
                            addElementsToSearchDisplay(m);
                        }
                    }
                }
            }


        }

        private void addElementsToSearchDisplay(Monument monument)
        {
            var mainGrid = new Grid();
            mainGrid.Height = 50;
            mainGrid.Tag = monument.ID;
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition());

            (mainGrid as UIElement).MouseEnter += MouseOverSearchElement;
            (mainGrid as UIElement).MouseLeave += MouseLeaveSearchElement;
            (mainGrid as UIElement).MouseDown += MouseClickSearchElement;

            var text1 = new TextBlock();
            text1.FontSize += FONT_SIZE_CHANGE;
            text1.Foreground = Brushes.White;
            text1.Text = monument.ID;
            text1.HorizontalAlignment = HorizontalAlignment.Center;
            text1.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(text1, 0);

            var text2 = new TextBlock();
            text2.FontSize += FONT_SIZE_CHANGE;
            text2.Foreground = Brushes.White;
            text2.Text = monument.Name;
            text2.HorizontalAlignment = HorizontalAlignment.Center;
            text2.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(text2, 1);

            var text3 = new TextBlock();
            text3.FontSize += FONT_SIZE_CHANGE;
            text3.Foreground = Brushes.White;
            text3.Text = monument.Type.ToString();
            text3.HorizontalAlignment = HorizontalAlignment.Center;
            text3.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(text3, 2);

            var text4 = new TextBlock();
            text4.FontSize += FONT_SIZE_CHANGE;
            text4.Foreground = Brushes.White;
            text4.Text = monument.Climate.ToString();
            text4.HorizontalAlignment = HorizontalAlignment.Center;
            text4.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(text4, 3);

            mainGrid.Children.Add(text1);
            mainGrid.Children.Add(text2);
            mainGrid.Children.Add(text3);
            mainGrid.Children.Add(text4);

            SearchResultsDisplay.Children.Add(mainGrid);

        }

        private Grid GetMonumentGrid(string id)
        {

            foreach(Grid g in MonumentsStackPanel.Children)
            {
                if (g.Tag.ToString().Equals(id))
                {
                    return g;
                }
            }


            return null;
        }


        private void ChangeMonumentGridBG(Grid grid, Color c)
        {
            Grid childGrid = grid.Children[0] as Grid;
            Color color = c; //(Color)ColorConverter.ConvertFromString("#093647");
            var secondBrush = new SolidColorBrush();
            secondBrush.Opacity = 0.3;
            secondBrush.Color = color;
            childGrid.Background = secondBrush;
        }

        private Grid lastGridSearched = null;

        private void searchAndDisplayMonument(string id)
        {
            selectedDisplayMonument = getMonumentById(id);

            if (selectedDisplayMonument == null)
            {
                NotifyUser("Monument has been deleted");
                return;
            }

            Grid gridToView = GetMonumentGrid(id);
            UIElement container = VisualTreeHelper.GetParent(gridToView) as UIElement;
            Point relativeLocation = gridToView.TranslatePoint(new Point(0, 0), container);
            MonumentsGridScrollViewer.ScrollToVerticalOffset(relativeLocation.Y);

            if (lastGridSearched != null)
            {
                ChangeMonumentGridBG(lastGridSearched, Colors.Black);
            }

            lastGridSearched = gridToView;

            ChangeMonumentGridBG(gridToView, Colors.White);

            ChangeDisplayInfo(selectedDisplayMonument);


            if (selectedDisplayMonument.monumentPin != null)
            {
                Location location = new Location(selectedDisplayMonument.monumentPin.latitude, selectedDisplayMonument.monumentPin.longitude);
                worldMap.Center = location;
                worldMap.ZoomLevel = 16;
                OpenDisplayInfo();
            }


            CloseSearchResults();
        }

        private void MouseClickSearchElement(object sender, MouseButtonEventArgs e)
        {

            var grid = sender as Grid;
            string id = grid.Tag.ToString();
            searchAndDisplayMonument(id);


        }

        private void MouseLeaveSearchElement(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            grid.Background = (Brush)brushConverter.ConvertFrom("#072530"); //072530
        }

        private void MouseOverSearchElement(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            grid.Background = (Brush)brushConverter.ConvertFrom("#093647");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchMonumentsDisplay();
            ShowSearchResults();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            e.Handled = true;

            SearchOnClick();
        }

        private void SearchOnClick()
        {
            if (!FilterSearch.Text.Equals("ID")) return;
            string id = searchTextBox.Text;

            Monument m = getMonumentById(id);
            if (m == null)
            {
                NotifyUser("No monument with that Id");
                return;
            }
            else
            {
                searchAndDisplayMonument(id);
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {

            if (searchTextBox.Text.Equals(""))
            {
                SearchMonumentsDisplay();
                ShowSearchResults();
            }
            else
            {
                SearchOnClick();
            }

        }

        private void FilterSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchMonumentsDisplay();
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSearchResults();
        }



        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(Application.Current.Windows[0]);
            if (focusedControl is DependencyObject)
            {
                string str = HelpProvider.GetHelpKey((DependencyObject)focusedControl);
                HelpProvider.ShowHelp(str, this);
            }
        }


    }
}
