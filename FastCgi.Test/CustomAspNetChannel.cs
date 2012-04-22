﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastCgi.Test
{
	[Serializable]
	public class CustomAspNetChannel : FastCgi.Protocol.FastCgiChannel
	{
		private Dictionary<ushort, CustomAspNetRequest> _request = new Dictionary<ushort, CustomAspNetRequest>();

		public CustomAspNetChannel()
		{
			this.Properties = new Protocol.ChannelProperties()
			{
				MaximumConnections = 1,
				MaximumRequests = 1,
				SupportMultiplexedConnection = false
			};
		}

		protected override Protocol.Request CreateRequest(ushort requestId, Protocol.BeginRequestMessageBody body)
		{
			return new CustomAspNetRequest(requestId, body);
		}

		protected override void AddRequest(Protocol.Request request)
		{
			_request.Add(request.Id, (CustomAspNetRequest)request);
		}

		protected override void RemoveRequest(Protocol.Request request)
		{
			_request.Remove(request.Id);
		}

		protected override Protocol.Request GetRequest(ushort requestId)
		{
			return _request[requestId];
		}

		protected override void ExecuteRequest(Protocol.Request request)
		{
			request.Execute();
		}

		protected override void AbortRequest(Protocol.Request request)
		{
			request.Abort();
		}
	}
}
