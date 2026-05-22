using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityMatch
{
    public class SuchKriterien
    {
        public List<string> klima { get; set; } = new List<string>();
        public List<string> landschaft { get; set; } = new List<string>();
        public List<string> charakter { get; set; } = new List<string>();
        public List<string> kultur { get; set; } = new List<string>();
        public List<string> aktivitaet { get; set; } = new List<string>();
        public List<string> kueche { get; set; } = new List<string>();
        public List<string> groesse { get; set; } = new List<string>();
    }
}
