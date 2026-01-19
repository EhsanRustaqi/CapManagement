using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.CarDtoModels
{
    public class RdwCarDto
    {

        public string? merk { get; set; }
        public string? handelsbenaming { get; set; }
        public string? eerste_kleur { get; set; }
        public string? voertuigsoort { get; set; }
        public string? brandstof_omschrijving { get; set; }
        public string? aantal_deuren { get; set; }
        public string? aantal_zitplaatsen { get; set; }
        public string? cilinderinhoud { get; set; }
        public string? massa_ledig_voertuig { get; set; }
        public string? vervaldatum_apk { get; set; }
        public string? datum_eerste_toelating { get; set; } // ✅ correct field name
    }
}
