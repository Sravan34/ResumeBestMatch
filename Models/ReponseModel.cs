using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeTextExtract.Models
{
    public class ReponseModel
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsResponse { get; set; }

        public string Data { get; set; }

        public string DataMessage { get; set; }
    }
}
