using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SslVerifier
{
	class DataReader
	{
		public List<string> listOfUrls = new List<string>();
		public DataReader()
		{

			using (var reader = new StreamReader(@"C:\Users\sam.egger\source\repos\SslVerifier\listofsites.csv"))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line.Split(';');

					listOfUrls.Add(values[0]);
				}
			}
		}
	}
}
