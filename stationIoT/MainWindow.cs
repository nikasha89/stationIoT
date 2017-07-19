using System;
using Gtk;
using stationIoT;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using AwokeKnowing.GnuplotCSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Collections;


public partial class MainWindow: Gtk.Window
{
	protected const string URLGET = "https://api.thingspeak.com/channels/284296/fields/4.json";
	protected const string URLPOST = "https://api.thingspeak.com/talkbacks/16252/commands.json";
	protected const string urlParametersGET = "?api_key=SX2LN61B4U6ABF32";
	protected const string urlParametersPOST = "?api_key=XV3LVOKL9CJF1I4Q&command_string=";
	private double[] Date;
	private double[] Light;

	protected bool firsTimeAuto = true;
	public class Channel
	{
		public int id { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public string latitude { get; set; }
		public string longitude { get; set; }
		public string field1 { get; set; }
		public string field2 { get; set; }
		public string field3 { get; set; }
		public string field4 { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
		public int last_entry_id { get; set; }
	}

	public class Feed
	{
		public string created_at { get; set; }
		public int entry_id { get; set; }
		public string field4 { get; set; }
	}

	public class dataObject
	{
		public Channel channel { get; set; }
		public List<Feed> feeds { get; set; }
	}

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	/*
	protected bool Post_Call_API(string url)
	{
		var res = true;
		HttpClient client = new HttpClient();
		client.BaseAddress = new Uri(url);

		// Add an Accept header for JSON format.
		client.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/json"));

		// List data response.
		HttpResponseMessage response = client.GetAsync(urlParametersPOST).Result;
		if (!response.IsSuccessStatusCode) {
			System.Console.WriteLine ("Error al enviar comando a url: {0}", url);
			res = false;
		} 
		else
			System.Console.WriteLine ("Enviado y Recibido --> Comando a url: {0}", url);

		return res;
	}*/

	protected void Post_Call_API(string url, string command){
		var client = new HttpClient ();

		client.BaseAddress = new Uri(URLPOST+urlParametersPOST+command);
		client.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/json"));
		//client.Headers[HttpRequestHeader.ContentType] = "application/json"; 
		//var result = client.UploadString(url, "POST", json);
		var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{"api_key", "XV3LVOKL9CJF1I4Q"},
				{"command_string", command}
			});
		//content.Headers.ContentType = "application/json";
		var result = client.PostAsync(urlParametersPOST+command, content).Result;
		string resultContent = result.Content.ReadAsStringAsync().Result;
		//string s = client.UploadString(url, "POST", json);
		Console.WriteLine(resultContent);
	}

	protected void muestraVisualizacion (object sender, EventArgs e)
	{
		HttpClient client = new HttpClient();
		client.BaseAddress = new Uri(URLGET);

		// Add an Accept header for JSON format.
		client.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/json"));

		string dataObjects = "";
		// List data response.
		HttpResponseMessage response = client.GetAsync(urlParametersGET).Result;
		if (response.IsSuccessStatusCode)
		{				
			// Parse the response body.	
			dataObjects = response.Content.ReadAsStringAsync().Result;
			Console.WriteLine ("{0}", dataObjects);

			var serializer = new JavaScriptSerializer();
			var obj = serializer.DeserializeObject (dataObjects);
			Dictionary<string, object> dString = obj as Dictionary<string, object>;
			//Console.WriteLine ("Ahora serializado: {0}", feedObj);

			IList feeds = dString["feeds"] as IList;	
			int i = 0;
			Date = new double[feeds.Count];
			Light = new double[feeds.Count];
			foreach(var feed in feeds)
			{
				Dictionary<string, object> f = feed as Dictionary<string, object>;
				DateTime dateTime = Convert.ToDateTime(f ["created_at"]);
				var dateUNIX = (TimeZoneInfo.ConvertTimeToUtc (dateTime) - new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
				Date [i] = Convert.ToDouble(dateUNIX);
				//(string)f ["created_at"];
				Light[i] = Convert.ToDouble(f ["field4"]);
				i++;

			}

			//GnuPlot.HoldOn();
			GnuPlot.Plot(Date, Light, "with points pt 2");
		}
		else
		{
			Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
		}
	}

		
	protected void estadoAutoON (object sender, EventArgs e)
	{
		MessageDialog msdSame = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "Se ha Cambiado el modo Auto");
		msdSame.Title="Comando Enviado";
		Auto.Label = "Auto: ON";

	}

	protected void estadoAutoOFF (object sender, EventArgs e)
	{
		Auto.Label = "Auto: OFF";
	}

	protected void estadoAuto (object sender, EventArgs e)
	{
		string str = "";
		if (firsTimeAuto) 
		{
			Auto.Label = "Auto: ON";
			firsTimeAuto = false;
			str = "auto_on";
		} 
		else 
		{
			Auto.Label = "Auto: OFF";
			firsTimeAuto = true;
			str = "auto_off";
		}
		Post_Call_API (URLPOST, str);

			
	}

	protected void cambiaVelocidad (object sender, EventArgs e)
	{
		cambiaCombo (speed);
	}

	protected void cambiaCombo(ComboBox cb)
	{
		TreeIter iter;
		if (cb.GetActiveIter (out iter))
			Console.WriteLine ("Modificado a:"+(string) cb.Model.GetValue (iter, 0));

		string urlAux = URLPOST;
		Post_Call_API (urlAux, (string)cb.Model.GetValue(iter, 0));
	}

	protected void exit (object sender, EventArgs e)
	{
		Application.Quit ();
	}

		
}