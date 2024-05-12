using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InventoryManagement
{


    public class Nimike
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Code { get; set; }
        public string Units { get; set; }
        public string Alarm { get; set; }
        public DateTime DateAdded { get; set; }
        public Brush BackgroundColor { get; set; } = Brushes.White; // Default background color is white

        //public float Price { get; set; }
    }
}

