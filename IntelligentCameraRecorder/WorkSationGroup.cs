using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentCameraRecorder
{
    class WorkSationGroup
    {
        private string currentParameterFileName;
        private int workstationNUM;
        private int currentEnabledWorkstationID=0;
        private bool isWorkstationGroupFull = false;
        private WorkStation[] worksations;
        public  WorkSationGroup(string curFileName)
        {
            currentParameterFileName = curFileName;
            WorkstationNUM = int.Parse(Utility.GetValue("camera", "workstations", "1", currentParameterFileName));
            worksations = new WorkStation[WorkstationNUM];
            for(int i = 0; i < WorkstationNUM; i++)
            {
                // worksations[i].WorkstationID = i + 1;
                worksations[i] = new WorkStation();
                worksations[i].IncludedCCDs = Utility.GetValue("camera", "station" + (i + 1), " ", currentParameterFileName);
            }
        }

        public bool IsWorkstationGroupFull { get => isWorkstationGroupFull; set => isWorkstationGroupFull = value; }
        public int CurrentEnabledWorkstationID { get => currentEnabledWorkstationID; set => currentEnabledWorkstationID = value; }
        public int WorkstationNUM { get => workstationNUM; set => workstationNUM = value; }

        public int getWorksationID(string ccdStr)
        {
            int iRet = 1;
            for(int i=0; i < WorkstationNUM; i++)
            {
                if (Utility.csvStringMatched(worksations[i].IncludedCCDs, ccdStr))
                {
                    iRet = i; //
                    break;
                }
                    
            }
            return iRet;
        }
        
        public void setEnabledWorksationID(int id)
        {
            CurrentEnabledWorkstationID = id;
            if (CurrentEnabledWorkstationID >= WorkstationNUM)
                isWorkstationGroupFull = true;
        }
        /*
        public void increaseEnabledWorkstation(string strCCD)
        {
            if (worksations[getWorksationID(strCCD)].IsWorking)
                return; //如果当前工位已经启动工作了，就直接返回。
            worksations[getWorksationID(strCCD)].IsWorking = true;
            CurrentEnabledWorkstationID++;
            if (CurrentEnabledWorkstationID >= WorkstationNUM)
            {
                CurrentEnabledWorkstationID = WorkstationNUM;
                IsWorkstationGroupFull = true;
            }
        }
        */
    }
}
