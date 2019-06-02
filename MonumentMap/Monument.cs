using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonumentMap
{
    public enum ClimateType { POLAR=0, CONTINENTAL=1, MODERATE_CONTINENTAL=2, DESERT=3, TROPICAL=4, SUBTROPICAL=5 }
    public enum TouristStatus { EXPLOITED=0, AVAILABLE=1, UNAVAILABLE=2 }

    [Serializable]
    public class Monument
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon_path { get; set; }
        public string Picture_path { get; set; }

        public MonumentType Type { get; set; }
        public ClimateType Climate { get; set; }
        public TouristStatus TourStatus { get; set; }

        public bool IsEcoEndangered { get; set; }
        public bool ContainsEndangeredSpecies { get; set; }
        public bool IsInSettlement { get; set; }

        public double AnnualIncome { get; set; } //displayed in $USD
        public DateTime DateOfDiscovery { get; set; }

        public Monument() {  }


    }
}
