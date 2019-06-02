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
    public class MonumentType: INotifyPropertyChanged
    {
        private string _id;
        private string _icon_path;
        private string _name;
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


        public string Icon_path
        {
            get
            {
                return _icon_path;
            }

            set
            {
                _icon_path = value;
                OnPropertyChanged(Icon_path);
            }
        }


        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                OnPropertyChanged(Name);
            }
        }


        public string Description
        {
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

        public MonumentType()
        {

        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }



        public override string ToString()
        {
            return ID + ": " + Name;
        }

    }
}
