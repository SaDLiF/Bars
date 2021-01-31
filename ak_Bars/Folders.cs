using System;
using System.Collections.Generic;
using System.Text;

namespace ak_Bars
{
    class Folders
    {
        public string name { get; set; }
        public string path { get; set; }
        public Type type { get; set; }
    }

    public enum Type
    {
        Folder, File, Back
    }
}
