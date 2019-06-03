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
        private const string monumentsFile = "monuments.mon";
        private const string monumentTypesFile = "monumentTypes.typ";
        private const string monumentTagsFile = "monumentTags.tag";

        private string monumentPath;
        private string monumentTypePath;
        private string monumentTagPath;

        private BinaryFormatter formatter = new BinaryFormatter();


        public IOSerializer()
        {
            monumentPath = Path.Combine(directoryName, monumentsFile);
            monumentTypePath = Path.Combine(directoryName, monumentTypesFile);
            monumentTagPath = Path.Combine(directoryName, monumentTagsFile);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

                                /***************
                                 * SERIALIZERS *
                                 * *************/

        public bool serializeMonuments(ObservableCollection<Monument> monuments)
        {

            try
            {
                
                using(Stream stream = new FileStream(monumentPath, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, monuments);
                }

                return true;

            } catch(Exception)
            {
                
                return false;
            }
        }

        public bool serializeMonumentTypes(ObservableCollection<MonumentType> monumentTypes)
        {

            try
            {

                using (Stream stream = new FileStream(monumentTypePath, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, monumentTypes);
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }


        public bool serializeMonumentTags(ObservableCollection<MonumentTag> tags)
        {
            try
            {
                using (Stream stream = new FileStream(monumentTagPath, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, tags);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            

        }



                                            /***************
                                            * DESERIALIZERS *
                                            * *************/

        public ObservableCollection<Monument> deserializeMonuments()
        {

            ObservableCollection<Monument> observableMonums = null;

            
            if(File.Exists(monumentPath))
            {

                try
                {

                    using (Stream stream = new FileStream(monumentPath, FileMode.Open, FileAccess.Read))
                    {
                        observableMonums = (ObservableCollection<Monument>)formatter.Deserialize(stream);
                    } 


                } catch(Exception e)
                {
                    return null;
                }
                
            }

            return observableMonums;
        }


        public ObservableCollection<MonumentType> deserializeMonumentTypes()
        {
            ObservableCollection<MonumentType> observableTypes = null;

            if(File.Exists(monumentTypePath))
            {
                try
                {
                    using(Stream stream = new FileStream(monumentTypePath, FileMode.Open, FileAccess.Read))
                    {
                        observableTypes = (ObservableCollection<MonumentType>)formatter.Deserialize(stream);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return observableTypes;
        }


        public ObservableCollection<MonumentTag> deserializeMonumentTags()
        {
            ObservableCollection<MonumentTag> observableTags = null;

            if (File.Exists(monumentTagPath))
            {
                try
                {
                    using (Stream stream = new FileStream(monumentTagPath, FileMode.Open, FileAccess.Read))
                    {
                        observableTags = (ObservableCollection<MonumentTag>)formatter.Deserialize(stream);
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            return observableTags;
        }


    }
}
