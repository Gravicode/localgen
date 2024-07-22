using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace localgen.Models
{
    public class AIModel
    {
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public string FolderName { get; set; }
        public bool IsVision { get; set; } = false;
    }
}
