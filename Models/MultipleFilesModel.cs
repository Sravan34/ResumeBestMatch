using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeTextExtract.Models
{
    public class MultipleFilesModel : ReponseModel
    {
        [Required(ErrorMessage = "Please select files")]
        public List<IFormFile> Files { get; set; }

        public object SkillSet { get; set; }
    }

    public class SkillSet
    {
        public int Id { get; set; }

        public string SkillName { get; set; }
    }
}
