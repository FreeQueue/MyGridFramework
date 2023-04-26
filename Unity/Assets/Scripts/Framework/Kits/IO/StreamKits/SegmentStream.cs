#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.ComponentModel;
using System.IO;
using Framework.Extensions;

namespace Framework.Kits.StreamKits
{
	public class SegmentStream : Stream
	{
		public readonly Stream baseStream;
		public readonly long baseOffset;
		public readonly long baseCount;

		/// <summary>
		/// Creates a new <see cref="SegmentStream"/> that wraps a subset of the source stream. This takes ownership of
		/// the source stream. The <see cref="SegmentStream"/> is dependent on the source and changes its underlying
		/// position.
		/// </summary>
		/// <param name="stream">The source stream, of which only a segment should be exposed. Ownership is transferred
		/// to the <see cref="SegmentStream"/>.</param>
		/// <param name="offset">The offset at which the segment starts.</param>
		/// <param name="count">The length of the segment.</param>
		public SegmentStream(Stream stream, long offset, long count)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (!stream.CanSeek)
				throw new ArgumentException("stream must be seekable.", nameof(stream));
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), "offset must be non-negative.");
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "count must be non-negative.");

			baseStream = stream;
			baseOffset = offset;
			baseCount = count;

			stream.Seek(baseOffset, SeekOrigin.Begin);
		}

		public override bool CanSeek => true;
		public override bool CanRead => baseStream.CanRead;
		public override bool CanWrite => baseStream.CanWrite;

		public override long Length => baseCount;

		public override long Position
		{
			get => baseStream.Position - baseOffset;
			set => baseStream.Position = baseOffset + value;
		}

		public override int ReadByte()
		{
			if (Position < Length)
				return baseStream.ReadByte();
			return -1;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var remaining = Length - Position;
			if (remaining <= 0)
				return 0;
			return baseStream.Read(buffer, offset, (int)Math.Min(remaining, count));
		}

		public override void WriteByte(byte value)
		{
			if (Position < Length)
				baseStream.WriteByte(value);
			throw new IOException("Attempted to write past the end of the stream.");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Position + count >= Length)
				throw new IOException("Attempted to write past the end of the stream.");
			baseStream.Write(buffer, offset, count);
		}

		public override void Flush() { baseStream.Flush(); }
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				default: throw new InvalidEnumArgumentException(nameof(origin), (int)origin, typeof(SeekOrigin));
				case SeekOrigin.Begin:
					return baseStream.Seek(baseOffset + offset, SeekOrigin.Begin) - offset;
				case SeekOrigin.Current:
					return baseStream.Seek(offset, SeekOrigin.Current) - offset;
				case SeekOrigin.End:
					return baseStream.Seek(offset, SeekOrigin.End) - offset;
			}
		}

		public override void SetLength(long value) { throw new NotSupportedException(); }

		public override bool CanTimeout => baseStream.CanTimeout;

		public override int ReadTimeout
		{
			get => baseStream.ReadTimeout;
			set => baseStream.ReadTimeout = value;
		}

		public override int WriteTimeout
		{
			get => baseStream.WriteTimeout;
			set => baseStream.WriteTimeout = value;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				baseStream.Dispose();
			base.Dispose(disposing);
		}

		private static long GetOverallNestedOffset(Stream stream, out Stream overallBaseStream)
		{
			var offset = 0L;
			overallBaseStream = stream;
			if (stream is SegmentStream segmentStream)
				offset += segmentStream.baseOffset + GetOverallNestedOffset(segmentStream.baseStream, out overallBaseStream);
			return offset;
		}

		/// <summary>
		/// Creates a new <see cref="Stream"/> that wraps a subset of the source stream without taking ownership of it,
		/// allowing it to be reused by the caller. The <see cref="Stream"/> is independent of the source stream and
		/// won't affect its position.
		/// </summary>
		/// <param name="stream">The source stream, of which only a segment should be exposed. Ownership is retained by
		/// the caller.</param>
		/// <param name="offset">The offset at which the segment starts.</param>
		/// <param name="count">The length of the segment.</param>
		public static Stream CreateWithoutOwningStream(Stream stream, long offset, int count)
		{
			var nestedOffset = offset + GetOverallNestedOffset(stream, out Stream parentStream);

			// Special case FileStream - instead of creating an in-memory copy,
			// just reference the portion of the on-disk file that we need to save memory.
			// We use GetType instead of 'is' here since we can't handle any derived classes of FileStream.
			if (parentStream.GetType() == typeof(FileStream))
			{
				var path = ((FileStream)parentStream).Name;
				return new SegmentStream(File.OpenRead(path), nestedOffset, count);
			}

			// For all other streams, create a copy in memory.
			// This uses more memory but is the only way in general to ensure the returned streams won't clash.
			stream.Seek(offset, SeekOrigin.Begin);
			return new MemoryStream(stream.ReadBytes(count));
		}
	}
}
