using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace IntelligentCameraRecorder
{
    class CCDInfo
    {
        public string ccd_name;
        public string columns_names;
        private Queue ccdVaulesQueue;
        private bool isDirty ;
        private int ccdValuesCounter = 0;

        public int CcdValuesCounter { get => ccdValuesCounter; set => ccdValuesCounter = value; }

        public int getValuesQueueLength()
        {
            if(ccdVaulesQueue!=null)
                return ccdVaulesQueue.Count;
            return 0;
        }
        public CCDInfo()
        {
            isDirty = false;
            ccdVaulesQueue = new Queue();
        }
        public Boolean isThisCCD(string ccdn)
        {
            return ccd_name.Equals(ccdn);
        }
        public void pushValues(string vals)
        {
            isDirty = true;
            ccdVaulesQueue.Enqueue(vals);
        }

        public string popValues()
        {
            if (ccdVaulesQueue.Count == 0)
            {
                isDirty = false;
                return "";
            }
            return (string)ccdVaulesQueue.Dequeue();
        }
    }
}
