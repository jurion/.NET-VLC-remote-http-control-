using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VlcLib.Status
{

    public enum FluxType
    {
        Audio,
        Video,
        SubTitles
    }

    public class VlcFlux
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FluxType FluxType { get; set; }
    }
}
