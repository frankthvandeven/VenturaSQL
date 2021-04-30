using System;
using System.Collections.Generic;
using System.Text;

namespace VenturaSQLStudio {
    /// <summary>
    /// Parses a string into value names and value data.
    /// For example: 
    /// Venturasqlstudio=DesktopShortcut StartMenuShortcut Folder "VenturaSystem" Filename "VenturaSQL Studio.lnk" MainBinary
    /// The parser will find the following value names and data: Folder "VenturaSystem" and Filename "T....lnk"
    /// </summary>
    public class ValueDataParser
    {
        private string[] _switches;
        private List<SubValue> _results = new List<SubValue>();

        /// <summary>
        /// Constructor for ValueDataParser.
        /// </summary>
        /// <param name="switches">A list of value names to consider as switches.</param>
        public ValueDataParser(string[] switches)
        {
            if (switches == null)
                throw new ArgumentNullException("switches");

            _switches = switches;
        }
        
        
        /// <summary>
        /// Parses a string into value names and value data.
        /// The function expects value names and value data in pairs, 
        /// except for value names defined as switches, these value names have no data.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        public void ParseLine(string text)
        {
            
            List<string> founditems = new List<string>();
            StringBuilder sb = new StringBuilder(1024);

            QuoteState quotestate = QuoteState.Outside;

            for (int x = 0; x < text.Length; x++)
            {
                char current = text[x];

                if (quotestate == QuoteState.InsideQuotes)
                {
                    if (current == '\"' && text.Length > x + 1 && text[x + 1] == '\"')
                    {
                        sb.Append('\"');
                        x++; // a double quote is special, skip one
                    }
                    else if (current == '\"')
                    {
                        founditems.Add(sb.ToString());
                        sb.Length = 0;
                        quotestate = QuoteState.Outside;
                    }
                    else
                        sb.Append(current);
                }
                else if (quotestate == QuoteState.Outside)
                {
                    if (current == '\"')
                    {
                        sb.Length = 0;
                        quotestate = QuoteState.InsideQuotes;
                    }
                    else if (current == ' ')
                    {
                        if (sb.Length > 0)
                            founditems.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    else
                        sb.Append(current);
                }
            } // end for

            if (sb.Length > 0)
                founditems.Add(sb.ToString());

            // The list with found keywords and strings is complete.
            // Now split up the list in value names and value data.
            _results.Clear();

            SubValue currentsubvalue = null; ;

            foreach (string item in founditems)
            {
                if (currentsubvalue == null)
                {
                    currentsubvalue = new SubValue();
                    currentsubvalue.ValueName = item;

                    foreach (string switchitem in _switches)
                        if (item.ToLower() == switchitem.ToLower())
                        {
                            currentsubvalue.IsSwitch = true;
                            _results.Add(currentsubvalue);
                            currentsubvalue = null;
                            break;
                        }
                }
                else
                {
                    currentsubvalue.ValueData = item;
                    _results.Add(currentsubvalue);
                    currentsubvalue = null;
                }

            }

        } // end of method

        /// <summary>
        /// Returns true if the parsed line contains the specified switch.
        /// </summary>
        /// <param name="name">The switch to check for.</param>
        /// <returns>True if the switch was found.</returns>
        public bool LineContainsSwitch(string name)
        {
            name = name.ToLower();

            foreach (SubValue subvalue in _results)
            {
                if (subvalue.ValueName.ToLower() == name && subvalue.IsSwitch == true)
                    return true;
            }

            return false;
        }

        public bool LineContainsValue(string name)
        {
            name = name.ToLower();

            foreach (SubValue subvalue in _results)
            {
                if (subvalue.ValueName.ToLower() == name && subvalue.IsSwitch == false)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the ValueData for the specified ValueName.
        /// If the ValueName does not exist, the function returns an empty string.
        /// </summary>
        public string Get(string valuename)
        {
            valuename = valuename.ToLower();

            foreach (SubValue subvalue in _results)
            {
                if (subvalue.ValueName.ToLower() == valuename && subvalue.IsSwitch == false)
                    return subvalue.ValueData;
            }

            return "";
        }


        enum QuoteState
        {
            Outside,
            InsideQuotes
        }

    } // end of class

    public class SubValue
    {
        private bool _isswitch;
        public string ValueName;
        public string ValueData;
        public bool IsSwitch
        {
            get { return _isswitch; }
            internal set { _isswitch = value; }
        }
    } // end of class
    
} // end of namespace