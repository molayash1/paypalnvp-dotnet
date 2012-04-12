using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace NeoSmart.PayPalNvp
{
	public class PayPal
	{
		public const double NvpVersion = 87.0;
		public const string NvpEndPoint = "https://api-3t.paypal.com/nvp";
		//public const string NvpEndPoint = "https://api-3t.sandbox.paypal.com/nvp";
		
		private string _user;
		private string _pass;
		private string _sig;
		
		public PayPal (string user, string pass, string signature)
		{
			_user = user;
			_pass = pass;
			_sig = signature;
		}
		
		private string EncodeNvpString (Dictionary<string, string> fields)
		{
			string nvpstr = string.Empty;
			
			foreach (var key in fields.Keys) {
				nvpstr += string.Format ("{0}={1}&", Uri.EscapeUriString (key.ToUpper ()), Uri.EscapeUriString (fields [key]));
			}
			
			return nvpstr;
		}
		
		private Dictionary<string, string> DecodeNvpString (string nvpstr)
		{
			var nvpMap = new Dictionary<string, string> ();
			
			string[] pairs = nvpstr.Split ('&');
			foreach (var pair in pairs) {
				string[] halves = pair.Split ('=');
				
				nvpMap [halves [0]] = halves.Length == 2 ? halves [1] : string.Empty;
			}
			
			return nvpMap;
		}
		
		public Dictionary<string, string> GenericNvp (string method, Dictionary<string, string> fields)
		{
			//Add some default PayPal-specific values
			fields ["USER"] = _user;
			fields ["PWD"] = _pass;
			fields ["SIGNATURE"] = _sig;
			fields ["VERSION"] = NvpVersion.ToString ();
			fields ["METHOD"] = method;
			
			string nvpstr = EncodeNvpString (fields);
			
			//Send the POST request to PayPal
			var request = (HttpWebRequest)WebRequest.Create (NvpEndPoint);
			request.CachePolicy = new System.Net.Cache.RequestCachePolicy (System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = nvpstr.Length;
			using (var writer = new StreamWriter(request.GetRequestStream())) {
				writer.Write (nvpstr);
			}
			
			//Get the result
			var response = request.GetResponse ();
			using (var reader = new StreamReader(response.GetResponseStream())) {
				nvpstr = reader.ReadToEnd ();
			}
			
			return DecodeNvpString (nvpstr);
		}
		
		//Start of PayPal convenience methods
		public Dictionary<string, string> SetExpressCheckout (Dictionary<string, string> fields)
		{
			string method = "SetExpressCheckout";
			return GenericNvp (method, fields);
		}
		
		public Dictionary<string, string> GetExpressCheckoutDetails (Dictionary<string, string> fields)
		{
			string method = "GetExpressCheckoutDetails";
			return GenericNvp (method, fields);
		}
		
		public Dictionary<string, string> DoExpressCheckoutPayment (Dictionary<string, string> fields)
		{
			string method = "DoExpressCheckoutPayment";
			return GenericNvp (method, fields);
		}
	}
}
