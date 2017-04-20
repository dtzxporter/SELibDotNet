using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///
///   ExtendedBinaryWriter.cs
///   Author: DTZxPorter
///   Written for the SE Format Project
///

namespace SELib.Utilities
{
    /// <summary>
    /// Supports custom methods for manipulating data between c++ streams and .net ones
    /// </summary>
    internal class ExtendedBinaryWriter : BinaryWriter
    {
        public ExtendedBinaryWriter(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Writes a null-terminated string to the stream
        /// </summary>
        /// <param name="value">The string to write</param>
        public void WriteNullTermString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            Write(buffer);
            Write((byte)0);
        }
    }
}
