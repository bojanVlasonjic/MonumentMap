using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MonumentMap
{
    public class Utility
    {

        public Utility()
        {

        }

        public bool isMonumentTypeUsed(string typeID, ObservableCollection<Monument> monuments)
        {

            foreach(Monument monum in monuments)
            {
                if(monum.Type.ID.Equals(typeID))
                {
                    return true;
                }
            }

            return false;

        }
    }
}
