using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeWalker.WinForms
{
    public class TabControlFix : TabControl
    {
        private const int TCM_ADJUSTRECT = 0x1328;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int WM_PAINT = 0x000F;
        private const int WM_ERASEBKGND = 0x0014;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == TCM_ADJUSTRECT && !DesignMode)
            {
                m.Result = (IntPtr)0;
                base.WndProc(ref m);

                RECT rect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                
                // Inflate the display rect to cover the 3D borders
                rect.Left -= 4;
                rect.Right += 4;
                rect.Bottom += 4;
                // Be very careful with Top. -1 might hide the line without eating text.
                rect.Top -= 1; 

                Marshal.StructureToPtr(rect, m.LParam, true);
                return;
            }

            if (m.Msg == WM_ERASEBKGND && !DesignMode)
            {
                // Intercept background erasure to prevent white flash/strip
                using (Graphics g = Graphics.FromHdc(m.WParam))
                {
                     // Force dark background
                     Color bg = (this.BackColor.R > 200) ? Color.FromArgb(45, 45, 48) : this.BackColor; 
                     using (SolidBrush brush = new SolidBrush(bg))
                     {
                         g.FillRectangle(brush, ClientRectangle);
                     }
                }
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);

            // Paint the background of the header area (to the right of the tabs)
            if (m.Msg == WM_PAINT && !DesignMode)
            {
                using (Graphics g = CreateGraphics())
                {
                     // Use specific dark color to match VS2015 theme or Control's BackColor
                     // FormUtils sets BackColor to wndback (approx 45,45,48)
                     // If BackColor is not set correctly, fallback to hardcoded dark.
                     Color bg = (this.BackColor.R > 200) ? Color.FromArgb(45, 45, 48) : this.BackColor; 
                     
                     using (SolidBrush brush = new SolidBrush(bg))
                     {
                         if (TabCount > 0)
                         {
                             Rectangle lastTabRect = GetTabRect(TabCount - 1);
                             // Fill remainder of the header strip
                             Rectangle headerBg = new Rectangle(
                                 lastTabRect.Right, 
                                 0, 
                                 Width - lastTabRect.Right, 
                                 lastTabRect.Height + 2 // Cover a bit more to be safe
                             );
                             g.FillRectangle(brush, headerBg);
                         }
                         else
                         {
                             // No tabs? Fill the whole header strip (approx height)
                             g.FillRectangle(brush, new Rectangle(0, 0, Width, 25));
                         }
                     }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); 
        }
    }
}
