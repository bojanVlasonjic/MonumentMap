using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonumentMap
{
    public class IOSerializer
    {
        private const string directoryName = "data";
        private const string fileName = "monuments.mon";

        private string fullPath = Path.Combine(directoryName, fileName);

        private BinaryFormatter formatter = new BinaryFormatter();


        public IOSerializer()
        {

        }


        public bool serializeMonuments(ObservableCollection<Monument> monuments)
        {
            if(!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            try
            {
                
                using(Stream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, monuments);
                    stream.Close();
                }

                return true;

            } catch(Exception e)
            {
                
                return false;
            }
        }


        public ObservableCollection<Monument> deserializeMonuments()
        {

            ObservableCollection<Monument> observableMonums = null;

            
            if(File.Exists(fullPath))
            {

                try
                {
                    using (Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        observableMonums = (ObservableCollection<Monument>)formatter.Deserialize(stream);
                        stream.Close();
                    }

                } catch(Exception e)
                {
                    return null;
                }
                
            }

            return observableMonums;
        }


        
    }
}
