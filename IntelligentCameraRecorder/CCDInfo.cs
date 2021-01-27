using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentCameraRecorder
{
    class CCDInfo
    {
        public string ccd_name;
        public string columns_names;
        public string columns_values;
        public bool isDirty = false;
        public Boolean isThisCCD(string ccdn)
        {
            return ccd_name.Equals(ccdn);
        }
        public void pushValues(string vals)
        {
            isDirty = true;
            columns_values = vals;
        }

        public string popValues()
        {
            isDirty = false;
            return columns_values;
        }
    }
}
