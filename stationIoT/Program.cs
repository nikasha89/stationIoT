using System;
using Gtk;

namespace stationIoT
{
	class MainClass
	{
		public static void Main (string[] args)
		{			
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Title = "Estación Meteorológica";
			win.Show ();
			Application.Run ();
		}
	}
}
