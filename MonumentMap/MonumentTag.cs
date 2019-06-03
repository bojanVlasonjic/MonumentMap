using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonumentMap
{
    [Serializable]
    public class MonumentTag: INotifyPropertyChanged
    {

        private string _id;
        private string _color;
        private string _description;


        public string ID {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
                OnPropertyChanged(ID);
            }
        }


        public string Color {

            get
            {
                return _color;
            }

            set
            {
                _color = value;
                OnPropertyChanged(Color);
            }
        }


        public string Description {

            get
            {
                return _description;
            }

            set
            {
                _description = value;
                OnPropertyChanged(Description);
            }

        }


        public MonumentTag()
        {

        }

        public override string ToString()
        {
            return ID;
        }


        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
