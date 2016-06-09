﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace Unity3DRavenCS {
	using RavenOptionType = Dictionary<string, string>;

	public class Unity3DRavenCS {
		private DSN m_dsn;
		private bool m_valid;
		private RavenOptionType m_options;

		public Unity3DRavenCS(string dsnUri)
		{
			m_dsn = new DSN(dsnUri);
			if (!m_dsn.isValid) {
				m_valid = false;
				Debug.Log ("Unity3DRavenCS is disabled because the DSN is invalid.");
			} else {
				m_valid = true;
			}
		}

		public string CaptureMessage(string message)
		{
			string resultId = "";

			if (m_valid) 
			{
				MessagePacket packet = new MessagePacket ();
				packet.message = message;


				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_dsn.sentryUri);
				request.Method = "POST";
				request.Accept = "application/json";
				request.ContentType = "application/json; charset=utf-8";
				request.Headers.Add("X-Sentry-Auth", m_dsn.XSentryAuthHeader());
				request.UserAgent = m_dsn.UserAgent();

				using (Stream requestStream = request.GetRequestStream ()) 
				{
					using (StreamWriter streamWriter = new StreamWriter(requestStream)) 
					{
						streamWriter.Write(packet.ToJson());
					}
				}

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					using (Stream responseStream = response.GetResponseStream())
					{
						if (responseStream != null)
						{
							using (StreamReader streamReader = new StreamReader(responseStream))
							{
								string responseContent = streamReader.ReadToEnd();
								ResponsePacket responsePacket = JsonUtility.FromJson<ResponsePacket>(responseContent);
								resultId = responsePacket.id;
							}
						}
					}
				}
			}

			return resultId;
		}
	}
}
