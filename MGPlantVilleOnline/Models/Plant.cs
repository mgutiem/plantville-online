using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGPlantVilleOnline.Models
{
    /// <summary>
    /// Plant Class
    /// </summary>
    public class Plant
    {
        public Seed Seed { get; set; }
        public DateTime HarvestTime { get; set; }
        public int Quantity { get; set; }
        public bool IsSpoiled => DateTime.Now > HarvestTime.AddMinutes(15); //Spoiling time for each plant if not harvested is 15 minutes

        public Plant(Seed seed)
        {
            Seed = seed;
            HarvestTime = DateTime.Now;
            Quantity = 1;
        }
    }
}
