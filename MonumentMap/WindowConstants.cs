using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonumentMap
{
    public class WindowConstants: INotifyPropertyChanged
    {
        private double _header_font_size { get; set; }
        private double _form_font_size { get; set; }
        private double _row_spacing { get; set; }


        public WindowConstants()
        {

        }

        public WindowConstants(int headerFontSize, int formLabelsSize)
        {
            this._header_font_size = headerFontSize;
            this._form_font_size = formLabelsSize;
        }


        /*************************
        * DataBinded properties *
        *************************/

        public double HeaderFontSize
        {
            get { return _header_font_size; }
            set
            {
                _header_font_size = value;
                OnPropertyChanged(nameof(HeaderFontSize));
            }
        }

        public double FormFontSize
        {
            get { return _form_font_size; }
            set
            {
                _form_font_size = value;
                OnPropertyChanged(nameof(FormFontSize));
            }
        }

        public double RowSpacing
        {
            get { return _row_spacing; }

            set
            {
                _row_spacing = value;
                OnPropertyChanged(nameof(RowSpacing));
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
