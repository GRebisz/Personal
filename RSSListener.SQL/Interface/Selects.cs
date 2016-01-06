using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSSListener.Data.Model;

namespace RSSListener.SQL.Interface
{
    interface Selects<T>
    {
        List<T> getAll();
        T getByID(Guid id);
        T getByName(string name);
    }
}
