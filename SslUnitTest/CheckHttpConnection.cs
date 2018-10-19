using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SslVerifier;

namespace SslUnitTest
{
	[TestClass]
	public class CheckHttpConnection
	{
		[TestMethod]
		public void ConstructorsUrl_AndTheConstructorsPropertyUrl_AreTheSame()
		{
			//Arrange	
			var expected = "https://www.google.com";

			//Act
			var actual = new SslCertificateVerification(1, "https://www.google.com");

			//Assert
			Assert.AreSame(expected, actual);
		}

		[TestMethod]

		public void ExpectedInsecureLinkList_AndActualInsecureLinkList_AreTheSame()
		{
			//Arrange	
			var expected = new SslCertificateVerification(1, "https://www.google.com");
			//expected.RequestASite();

			//Act
			var actual = new SslCertificateVerification(1, "https://www.google.com");
			
			//Assert
			Assert.AreSame(expected, actual);
		}

	}
}
