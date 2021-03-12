using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentCameraRecorder
{
    class WorkStation
    {
       // private int workstationID;
        private string includedCCDs;
        private bool isWorking = false;

       // public int WorkstationID { get => workstationID; set => workstationID = value; }
        public string IncludedCCDs { get => includedCCDs; set => includedCCDs = value; }
        public bool IsWorking { get => isWorking; set => isWorking = value; }
    }
}
