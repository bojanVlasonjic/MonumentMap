using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonumentMap
{
    [Serializable]
    public class MonumentType
    {
        public string ID { get; set; }
        public string Icon_path { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public MonumentType()
        {

        }
    }
}
