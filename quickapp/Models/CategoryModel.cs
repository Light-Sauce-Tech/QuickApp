using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quickapp.Models
{
    internal class CategoryModel
    {
        public string Name { get; set; }
        public List<AppModel> Applications { get; set; }
    }
}
