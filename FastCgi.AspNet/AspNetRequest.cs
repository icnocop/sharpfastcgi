﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using FastCgi.Protocol;
using ByteArray = FastCgi.ImmutableArray.ImmutableArray<byte>;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FastCgi.AspNet
{
	public class AspNetRequest : Request
	{
		public AspNetRequest(ushort id, BeginRequestMessageBody body)
			: base(id, body)
		{
			this.Status = String.Empty;
			this.Headers = new NameValueCollection();
		}

		public string VirtualPath { get; set; }

		public string PhysicalPath { get; set; }

		public bool HeaderSent { get; protected set; }

		public string Status { get; protected set; }

		public NameValueCollection Headers { get; protected set; }

		public void SetStatus(int statusCode, string statusDescription)
		{
			if (this.HeaderSent)
				throw new InvalidOperationException("Cannot set status on a response that has already been flushed");

			this.Status = String.Format("{0} {1} {2}", this.Parameters.GetValue("SERVER_PROTOCOL"), statusCode, statusDescription);
		}

		public void SetHeader(string name, string value)
		{
			if (this.HeaderSent)
				throw new InvalidOperationException("Cannot set headers on a response that has already been flushed");

			this.Headers.Set(name, value);
		}

		protected override void OnOutputStreamFlushing(FlushEventArgs args)
		{
			if (!this.HeaderSent)
			{
				args = new FlushEventArgs(this.SerializeHeaders() + args.Data);
				this.HeaderSent = true;
			}

			base.OnOutputStreamFlushing(args);
		}

		public override void Execute()
		{
			System.Web.HttpRuntime.ProcessRequest(new FastCgiWorkerRequest(this));
		}

		/// <summary>
		/// This is single threaded so we can't abort the request
		/// </summary>
		public override void Abort()
		{
			throw new InvalidOperationException("Abort is not available");
		}

		protected virtual ByteArray SerializeHeaders()
		{
			StringBuilder builder = new StringBuilder();

			if (String.IsNullOrEmpty(this.Status))
			{
				builder.Append(this.Status);
				builder.Append("\r\n");
			}

			foreach (string key in this.Headers.Keys)
			{
				builder.Append(key);
				builder.Append(": ");
				builder.Append(this.Headers[key]);
				builder.Append("\r\n");
			}

			builder.Append("\r\n");

			return new ByteArray(Encoding.UTF8.GetBytes(builder.ToString()));
		}
	}
}