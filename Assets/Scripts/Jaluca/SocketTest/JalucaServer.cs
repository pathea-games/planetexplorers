using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

public class JalucaServer : MonoBehaviour {
	const int portNo = 8000;
	// Use this for initialization
	void Start () {
		System.Net.IPAddress localAdd = System.Net.IPAddress.Parse ("127.0.0.1");
		Socket soc = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		TcpListener listener = new TcpListener (localAdd, portNo);
		listener.Start ();
		Console.WriteLine ("Server is starting...\n");
		byte[] rcvb = new byte[1024];
		int rcvlen = 0;
		while (true) 
		{
			rcvlen = soc.Receive(rcvb, rcvb.Length, 0);
				
			if (rcvlen < 0)
				rcvlen = 0;
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
