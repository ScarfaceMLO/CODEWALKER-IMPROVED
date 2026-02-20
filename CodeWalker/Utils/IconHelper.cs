using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace CodeWalker
{
    public static class IconHelper
    {
        private static Icon _appIcon;
        public static Icon AppIcon
        {
            get
            {
                if (_appIcon == null)
                {
                    try 
                    {
                        if (File.Exists("CW.ico"))
                        {
                            _appIcon = new Icon("CW.ico");
                        }
                        else
                        {
                            _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                        }
                    } 
                    catch 
                    {
                        // Fallback in worst case
                    }
                }
                return _appIcon;
            }
        }
    }
}
