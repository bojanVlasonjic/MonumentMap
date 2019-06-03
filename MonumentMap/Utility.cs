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


        public bool isMonumentTagUsed(string tagID, ObservableCollection<Monument> monuments)
        {
            foreach (Monument monum in monuments)
            {
                foreach (MonumentTag tag in monum.Tags)
                {
                    if (tag.ID.Equals(tagID))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public ObservableCollection<Monument> replaceMonument(ObservableCollection<Monument> monuments, Monument editedMonument)
        {

            for(int i = 0; i < monuments.Count; i++)
            {

                if(monuments[i].ID.Equals(editedMonument.ID))
                {
                    monuments[i] = editedMonument;
                    break;
                }

            }

            return monuments;

        }



    }
}
