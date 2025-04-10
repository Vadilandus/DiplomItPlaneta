using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Client.Model.Strong;

namespace TeamCenterTry
{
    // Класс Cборочной Eдиницы для выгрузки из TeamCenter
    internal class Detail
    {
        public string Type = "Детали";

        private string _Designation = "Null";
        public string Designation { get { return _Designation; } set { if (value == null) { 
                    Console.WriteLine("value of Designation is null!"); return; } else { _Designation = value; } } }

        private string _Name = "Null";

        public string Name { get { return _Name; } set { if (value == null) { 
                    Console.WriteLine("value of Name is null!"); return;} else { _Name = value; } } }

        private double _h47_HR03;
        
        public double h47_HR03 { get { return _h47_HR03; } set {  _h47_HR03 = value;  } }

        private List<ItemRevision> _Versions;

        public List<ItemRevision> Versions { get => _Versions; set => _Versions = value; }

        private List<ModelObject> _Relations;

        public List<ModelObject> Relations { get => _Relations; set => _Relations = value; }

        

        public Detail() { }

        public Detail(string name, string designation) { this.Name = name; this.Designation = designation; }

    }
}
