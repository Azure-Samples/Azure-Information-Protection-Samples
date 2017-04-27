using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zlib;
using OpenMcdf;
using SI.Mobile.RPMSGViewer.Lib;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public abstract class RpmsgHandler
	{
		public delegate void DecryptSuccessDelegate(DRMContent data, EndUserLicense eul);
		public delegate void DecryptErrorDelegate(Exception ex);

		protected DecryptSuccessDelegate OnDecryptSuccess { get; set; }
		protected DecryptErrorDelegate OnDecryptError  { get; set; }

		protected MessageRpmsg m_MessageRpmsg;

		public RpmsgHandler(MessageRpmsg messageRpmsg, DecryptSuccessDelegate onDecryptSuccess, DecryptErrorDelegate onDecryptError)
		{
			m_MessageRpmsg = messageRpmsg;
			OnDecryptSuccess = onDecryptSuccess;
			OnDecryptError = onDecryptError;
		}

		public abstract void DecryptImpl();
		public void Decrypt()
		{
			LogUtils.Log("MsgRpmsgDecryptStart");
			
			DecryptImpl();
		}


	}
}
