using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.SQL.Interface
{
    interface Inserts
    {
        bool writeInformation();
        bool isValid();
    }
}
