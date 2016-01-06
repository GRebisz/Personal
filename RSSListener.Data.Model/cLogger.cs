using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class cLogger
    {
        //text
        public bool writetext = false;
        public string wrtextdesc = String.Empty;
        public bool overwriteiffound = false;
        //Event Log
        public bool writeevents = false;
        public string sourcename = "";
        //Generically global
        public List<cEvent> EventInformation = new List<cEvent>();
        public cLogger()
        {
            System.Configuration.AppSettingsReader apr = new System.Configuration.AppSettingsReader();
            sourcename  = apr.GetValue("EventLogSourceName", "".GetType()).ToString();
            if (!System.Diagnostics.EventLog.SourceExists(sourcename))
            {
                System.Diagnostics.EventLog.CreateEventSource(sourcename, "GrebiszDEVEventlog");
            }
        }
        public bool addEvent(cEvent ev)
        {
            if (ev == null)
            {
                throw new ArgumentNullException("ev", "To add an event, it must not be null");
            }
            EventInformation.Add(ev);
            if (writeevents)
            {
                System.Diagnostics.EventLogEntryType type = System.Diagnostics.EventLogEntryType.Information;
                switch (ev.Severity)
                {
                    case Severity.FATAL:
                        type = System.Diagnostics.EventLogEntryType.Error;
                        break;
                    case Severity.Error:
                        type = System.Diagnostics.EventLogEntryType.Error;
                        break;
                    case Severity.Warning:
                        type = System.Diagnostics.EventLogEntryType.Warning;
                        break;
                    default:
                        type = System.Diagnostics.EventLogEntryType.Information;
                        break;
                }
                System.Diagnostics.EventLog.WriteEntry(sourcename, ev.ToString(), type);
            }
            return true;
        }
        public void writeEvents()
        {
            string output = "";
            foreach (cEvent ev in EventInformation)
            {
                output += "~~~New Event~~~\n" + ev.ToString() + "\n~~~End Of Event~~~\n";
            }
            if (writetext)
            {
                if (System.IO.File.Exists(wrtextdesc))
                {
                    if (overwriteiffound)
                    {
                        System.IO.File.Delete(wrtextdesc);
                    }
                    else
                    {
                        System.Diagnostics.EventLog.WriteEntry(sourcename, "A file with the path \"" + wrtextdesc + "\" already exists and the overwrite flag was set to false", System.Diagnostics.EventLogEntryType.Error);
                        throw new Exception("A file with the path \"" + wrtextdesc + "\" already exists and the overwrite flag was set to false");
                    }
                }
                System.IO.File.WriteAllText(wrtextdesc, output);
            }
        }
    }
    public class cEvent
    { 
        public Severity Severity;
        public string text = "";
        public Exception ex = null;
        public cEvent(Severity sev, string texttolog, Exception additionalexception = null)
        {
            this.Severity = sev;
            this.text = texttolog;
            ex = additionalexception;
        }
        public override string ToString()
        {
            string stack = "";
            if(ex != null)
            {
                stack = "Exception Message: " + ex.Message + "\nStack:\n" +  ex.StackTrace + "\n\n" + ex.ToString();
            }
            return "Priority: " + Severity.ToString() + "\n"
                                + "Text:\n" + text + "\n"
                                + "Exception Information:\n" + stack; 
        }
    }
    public enum Severity
    {
        FATAL = 0,
        Warning = 2,
        Error = 1,
        Information = 3
    }
}
