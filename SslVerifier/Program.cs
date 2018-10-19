using System;

namespace SslVerifier
{
	class Program
	{
		static void Main(string[] args)
		{
			//var establishConnection = new DbConnection();
			//establishConnection.ExtractUrlsFromXml();
			var dataReader = new DataReader();

			foreach (var site in dataReader.listOfUrls)
			{
				new SslCertificateVerification(site);
			}
			
			Console.WriteLine("END - ALL SITES HAVE NOW BEEN CHECKED");
			Console.ReadLine();
		}	
	}
}
