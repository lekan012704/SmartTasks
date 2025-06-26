using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Enums
{
    public enum CompanyType
    {
        Private,
        Government,
        NGO,
        Educational,
        Other
    }
    public enum TaskPriority
    {
        Low =1,
        Medium =2,
        High =3,
        Urgent=4
    }
    public enum TaskStatuses
    {
        New = 1,
        InProgress = 2,
        Completed = 3,
        Blocked = 4,
        OnHold = 5
    }

}
    