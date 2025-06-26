using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class OverdueTaskStatsDto
    {
        public string Week { get; set; }                  
        public string AssignedUserEmail { get; set; }            
        public string Priority { get; set; }            
        public int OverdueCount { get; set; }            
        public double AvgDaysOverdue { get; set; }       
    }

}
    