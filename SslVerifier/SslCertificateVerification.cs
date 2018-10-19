using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SslVerifier
{
	public class SslCertificateVerification
	{
		private string _url { get; set; }
		private int _tsmId { get; set; }
		private List<string> _insecureLink;
		private bool _validCertificate { get; set; }
		private DateTime _certificateExpiry { get; set; }

		public SslCertificateVerification(string url)
		{
			this._url = url;
			this._insecureLink = new List<string>();
			if (!url.Contains("@"))
			{
				RequestASite();
			}	
		}
		public SslCertificateVerification(int tsmId, string url)
		{
			this._url = url;
			this._tsmId = tsmId;
			this._insecureLink = new List<string>();
			RequestASite();
		}
		private void RequestASite()
		{
			try
			{
				HttpWebRequest request = WebRequest.CreateHttp(this._url);
				//request.AllowAutoRedirect = false;
				request.Timeout = 1000;
				request.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) { }
			}
			catch (Exception ex)
			{
				Console.WriteLine(this._url);
				Console.WriteLine(ex.Message);
			}
			Seperator();
			ExtractBodyData();
		}

		private void ExtractBodyData()
		{
			HtmlWeb client = new HtmlWeb();

			string htmlCode = client.ToString();
			try
			{
				var document = client.Load(_url);
				SearchBodyDataForNonSecureLinks(document.ParsedText);
				//PrintContentOfScriptTags(document);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void SearchBodyDataForNonSecureLinks(string extractedHtml)
		{

			string searchForUrls = @"(http|ftp|)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?";
			//string pageContainsTechnicweb = @"(technicweb)";

			MatchCollection match = Regex.Matches(extractedHtml, searchForUrls);

			foreach (Match m in match)
			{
				var sitesExtracted = String.Concat(m.Groups[0]);

				if (m.Value.Contains("http:"))
				{
					_insecureLink.Add(sitesExtracted);
				}
			}
			PrintInsecureLinks(_insecureLink);
			InsertExtractedDataIntoSslVerifierTblSites();
		}
		private static void PrintContentOfScriptTags(HtmlDocument document)
		{
			//Debug.WriteLine();

			var nodes = document.DocumentNode.SelectNodes($"//script");
			if (nodes != null)
				foreach (var node in nodes)
				{
					Console.WriteLine(node.OuterHtml);
				}
		}
		private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				this._validCertificate = true;
				Console.WriteLine(this._url);
				Console.WriteLine($"Certificate OK, it expires on {certificate.GetExpirationDateString()}");
				this._certificateExpiry = DateTime.Parse(certificate.GetExpirationDateString());
				return true;
			}
			else
			{
				this._validCertificate = false;
				Console.WriteLine("Certificate ERROR");
				return false;
			}
		}

		private void InsertExtractedDataIntoSslVerifierTblSites()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["LocalHost"].ConnectionString;
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				try
				{
					conn.Open();
					SqlCommand sqlCom = new SqlCommand("INSERT INTO " +
						   "[SSL].[dbo].[tblSites] " +
						   "VALUES (@website, @validCertificate, @insecureLinks)", conn);

					sqlCom.Parameters.AddWithValue("@website", this._url);
					sqlCom.Parameters.AddWithValue("@validCertificate", this._validCertificate.ToString());
					sqlCom.Parameters.AddWithValue("@insecureLinks", this._insecureLink.Count);
					//sqlCom.Parameters.AddWithValue("@certificateExpiry", this._certificateExpiry);
					//sqlCom.Parameters.AddWithValue("@tsmId", this._tsmId);
					sqlCom.ExecuteNonQuery();
					//SqlDataReader reader = sqlCom.ExecuteReader();
					conn.Close();
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
				}				
			}
		}

		private void Seperator()
		{
			Console.WriteLine("==================================================================================");
		}

		private void PrintInsecureLinks(List<String> insecureLinks)
		{
			if (insecureLinks.Count > 0)
			{
				Console.WriteLine("This site is not fully secure it has the following insecure Links:");
				foreach (var link in insecureLinks)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(link);
					Console.ResetColor();
				}
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("This site should be fully secure");
				Console.ResetColor();
			}

			Seperator();
		}
	}
}
