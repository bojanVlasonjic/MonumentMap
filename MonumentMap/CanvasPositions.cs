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

        public CanvasPositions() { }


        public double ScrollViewerHeights
        {
            get
            {
                return _scrollViewerHeights;
            }
            set
            {
                _scrollViewerHeights = value - 50;
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
