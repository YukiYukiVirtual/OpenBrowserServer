using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Test
{
	public class SslServerTest
	{
		public static void Main(string[] args)
		{
			X509Certificate2 cert = new X509Certificate2("certificate.pfx", "password");
			try
			{
				TcpListener listener = new TcpListener(IPAddress.Any, 12345);
				listener.Start();
				for(;;)
				{
					Console.WriteLine("listening");
					TcpClient client = listener.AcceptTcpClient();
					Console.WriteLine("accepted");
					SslStream stream = new SslStream(client.GetStream(), false);
					try
					{
						stream.AuthenticateAsServer(cert, false, SslProtocols.Tls12 | SslProtocols.Tls13, false);
						StreamReader sr = new StreamReader(stream);
						string line;
						while ((line = sr.ReadLine()) != null && !line.Equals(""))
						{
							Console.WriteLine("received: " + line);
						}
						StreamWriter sw = new StreamWriter(stream);
						sw.Write("HTTP/1.0 200 OK\r\n");
						sw.Write("Conenction: close\r\n");
						sw.Write("Content-Type: text/plain\r\n");
						sw.Write("Content-Length: 5\r\n");
						sw.Write("\r\n");
						sw.Write("hello");
						sw.Flush();
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
					finally
					{
						stream.Close();
					}
				}
			}
			catch(System.Net.Sockets.SocketException e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
