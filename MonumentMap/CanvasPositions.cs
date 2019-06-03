using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;


namespace MonumentMap
{
    public class CanvasPositions: INotifyPropertyChanged
    {

        private double _top;
        private double _left;

        private double _width;
        private double _height;

        private double _scrollViewerHeights;
        private double _monumentsScrollViewerHeight;
        private double _removeLeft;
        private double _findOnMapLeft;

        public CanvasPositions() { }


        public double FindOnMapLeft
        {
            get
            {
                return _findOnMapLeft;
            }
            set
            {
                _findOnMapLeft = value;
                OnPropertyChanged(nameof(FindOnMapLeft));
            }
        }

        public double RemoveLeft
        {
            get
            {
                return _removeLeft;
            }
            set
            {
                _removeLeft = value;
                OnPropertyChanged(nameof(RemoveLeft));
            }
        }

        public double MonumentsScrollViewerHeight
        {
            get
            {
                return _monumentsScrollViewerHeight;
            }
            set
            {
                _monumentsScrollViewerHeight = value;
                OnPropertyChanged(nameof(MonumentsScrollViewerHeight));
            }
        }

        public double ScrollViewerHeights
        {
            get
            {
                return _scrollViewerHeights;
            }
            set
            {
                _scrollViewerHeights = value - 50;
                MonumentsScrollViewerHeight = _scrollViewerHeights - 38;
                OnPropertyChanged(nameof(ScrollViewerHeights));
            }
        }

        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
        

        public double Top
        {
            get
            {
                return _top;
            }

            set
            {
                _top = value;
                OnPropertyChanged(nameof(Top));
            }
        }


        public double Left
        {
            get
            {
                return _left;
            }

            set
            {
                _left = value;
                OnPropertyChanged(nameof(Left));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
