using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VlcLib.Status
{
    public enum VlcState
    {
        Stopped,
        Playing,
        Paused
    }


    public class VlcStatus
    {

        private List<VlcFlux> fluxs = new List<VlcFlux>();

        public List<VlcFlux> Fluxs
        {
            get { return fluxs; }
            set { fluxs = value; }
        }

        public bool IsFullScreen { get; set; }
        public double AudioDelay { get; set; }
        public double SubsDelay { get; set; }
        public string VlcVersion { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan CurrentPossiton { get; set; }
        public VlcState State { get; set; }
        public string CurrentllyPlaying { get; set; }
        
    }
}
