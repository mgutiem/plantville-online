using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGPlantVilleOnline.Models
{
    /// <summary>
    /// Seed Class
    /// </summary>
    public class Seed
    {
        public string Name { get; set; }
        public int SeedPrice { get; set; }
        public int HarvestPrice { get; set; }
        public TimeSpan HarvestDuration { get; set; }

        //Constructor
        public Seed(string name, int seedPrice, int harvestPrice, TimeSpan harvestDuration)
        {
            Name = name;
            SeedPrice = seedPrice;
            HarvestPrice = harvestPrice;
            HarvestDuration = harvestDuration;
        }
    }
}
