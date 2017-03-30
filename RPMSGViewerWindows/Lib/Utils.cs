using System;
using System.Collections.Generic;
using System.Text;

namespace com.microsoft.rightsmanagement.windows.viewer.lib
{
    static class Utils
    {
        public static string OpenFileDialog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.*";
            dlg.Filter = "Protected JPEG |*.pJPG|Protected PNG|*.pPNG|Protected BMP|*.pBMP|Protected PDF|*.pPDF|Protected PDF V1|*.PDF|Protected TXT|*.pTXT";

            bool? dlgResult = dlg.ShowDialog();

            if (dlgResult == true)
                return dlg.FileName;
            else
                return null;
        }
    }
}
