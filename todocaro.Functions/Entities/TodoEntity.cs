using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todocaro.Functions.Entities
{
   public  class TodoEntity : TableEntity
    {
        public DateTime createdTime { get; set; }

        public string TaskDescription { get; set; }

        public bool IsCompleted { get; set; }
    }
}
