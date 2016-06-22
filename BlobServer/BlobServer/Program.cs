using System;
using System.Windows.Forms;
/**
 * PlayerCommunications.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: The main entry point for the application.
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobServer
{
    static class Program
    {
        /// The main entry point for the application.
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BlobServer());
        }
    }
}
