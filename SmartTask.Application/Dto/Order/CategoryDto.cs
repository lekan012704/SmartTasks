using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Order
{
    public class CategoryDto
    {
        [JsonProperty("category_id")]
        public int Id { get; set; }

        [JsonProperty("category")]
        public string Name { get; set; } = string.Empty;
    }

}
