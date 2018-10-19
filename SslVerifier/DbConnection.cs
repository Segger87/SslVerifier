using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SslVerifier
{
	public class DbConnection
	{
		private List<string> AgentUrl;
		public Dictionary<int, XElement> AgentSettings;

		public DbConnection()
		{
			AgentUrl = new List<string>();
			AgentSettings = new Dictionary<int,XElement>();
			EstablishDbConnection();
		}

		private void EstablishDbConnection()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				SqlCommand sqlCom = new SqlCommand("SELECT * " +
					   "FROM[tw_estateweb].[dbo].[tblAgents] WHERE AgentAccountEnabled = 1 ", conn)
				{
					CommandType = CommandType.Text
				};
				conn.Open();

				SqlDataReader reader = sqlCom.ExecuteReader();
				
				while (reader.Read())
				{
					IDataRecord dataRecord = reader;

					if (!string.IsNullOrEmpty(dataRecord["AgentSettings"].ToString()))
					{
						AgentSettings.Add(int.Parse(dataRecord["AgentId"].ToString()), XElement.Parse(dataRecord["AgentSettings"].ToString()));
					}
				}
				conn.Close();
			}
		}

		public void ExtractUrlsFromXml()
		{
			foreach (var agent in AgentSettings)
			{
				var primaryDomain = agent.Value.XPathSelectElement("domains/domain[@type='Primary']");

					if( primaryDomain != null)
				{
					var httpUrl = primaryDomain.Attribute("url").Value;
					httpUrl = "https://" + httpUrl;

					new SslCertificateVerification(agent.Key, httpUrl);
				}
			}
		}
	}
}
