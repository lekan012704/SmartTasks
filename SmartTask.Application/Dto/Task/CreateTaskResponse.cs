using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Task
{
    public class CreateTaskResponse
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string AssignedUserId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
