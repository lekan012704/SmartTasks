using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class FilteredOverdueTask
    {
        public string Week { get; set; }
        public string UserEmail { get; set; }
        public int? MinOverDueCount { get; set; }
    }
}
