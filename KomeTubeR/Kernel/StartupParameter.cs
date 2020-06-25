using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomeTubeR.Kernel
{
    public class StartupParameter
    {
        public string Url { get; set; }
        public string OutputFilePath { get; set; }
        public bool IsHide { get; set; }
        public bool IsClose { get; set; }
        public bool IsParsed { get; set; }
    }
}