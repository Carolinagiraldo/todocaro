using System;
using System.Collections.Generic;
using System.Text;

namespace todocaro.common.Models
{
    public class todo
    {
        public DateTime createdTime { get; set; }

        public  string TaskDescription { get; set; }

        public bool  IsCompleted { get; set; } 

    }
}
