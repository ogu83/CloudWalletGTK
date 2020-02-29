using System;
using Gtk;

namespace CloudWallet_GTK
{
    class MainClass
    {
        public static MainWindow win;
        public static void Main(string[] args)
        {
            Application.Init();
            win = new MainWindow();
            win.Show();
            Application.Run();
        }
    }
}
