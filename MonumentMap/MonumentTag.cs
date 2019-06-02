using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonumentMap
{
    [Serializable]
    public class MonumentTag
    {
        public string ID { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }


        public MonumentTag()
        {

        }
    }
}
