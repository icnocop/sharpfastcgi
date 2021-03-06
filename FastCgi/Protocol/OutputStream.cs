#region License
//*****************************************************************************/
// Copyright (c) 2012 Luigi Grilli
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//*****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using ByteArray = Grillisoft.ImmutableArray.ImmutableArray<byte>;

namespace Grillisoft.FastCgi.Protocol
{
	/// <summary>
	/// Output stream to write data to
	/// </summary>
	public class OutputStream : Stream
	{
		private MemoryStream _cache = new MemoryStream(256);

		public event EventHandler<FlushEventArgs> Flushing;

		private ByteArray _array = ByteArray.Empty;

		public OutputStream()
		{
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Flush()
		{
			this.WriteCache();

			if (_array.Count <= 0)
				return; //nothing to flush

			if (this.Flushing != null)
				this.Flushing(this, new FlushEventArgs(_array));

            _array.Dispose();
			_array = ByteArray.Empty;
		}

		public override long Length
		{
			get { return _array.Count; }
		}

		public override long Position { get; /* TODO: add setter checks */ set; }

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

        /// <summary>
        /// Free space in cache
        /// </summary>
        private int FreeCache
        {
            get { return _cache.Capacity - (int)_cache.Length; }
        }

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count <= this.FreeCache)
			{
				_cache.Write(buffer, offset, count);
			}
			else
			{
				this.WriteCache();
				_array = _array.Concat(new ByteArray(buffer, count, offset), true);
			}
		}

		private void WriteCache()
		{
			if (_cache.Length <= 0)
				return;

			_array = _array.Concat(new ByteArray(_cache.ToArray()), true);
			_cache.SetLength(0);
		}
	}

	public class FlushEventArgs : EventArgs
	{
		public FlushEventArgs(ByteArray data)
		{
			this.Data = data;
		}

		public ByteArray Data { get; private set; }
	}
}
