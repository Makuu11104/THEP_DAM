using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BeamRebar.Utils
{
    public static class DocumentUtils
    {
        public static Document Document;
        public static Element ToElement(this Reference rf) => Document.GetElement(rf);
    }
}

