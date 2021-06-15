using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WeakSum
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && File.Exists(args[0]))
            {
                List<byte> checksumOutputs = new List<byte>();
                using (FileStream stream = new FileStream(args[0], FileMode.Open))
                {
                    while (stream.Position < stream.Length - 1)
                    {
                        byte[] fileData = new byte[512];
                        stream.Read(fileData, 0, 512);
                        checksumOutputs.AddRange(WeakSum(fileData));
                        GC.Collect();
                    }
                    Console.WriteLine("File weaksum is " + WeakSumString(checksumOutputs.ToArray()));
                }
            } else if (args.Length == 1 && !File.Exists(args[0]))
            {
                Console.WriteLine("String weaksum is " + WeakSumString(Encoding.UTF8.GetBytes(args[0])));
            }
            else
            {
                Console.Write("Enter a string to create a weaksum from\n>");
                Console.WriteLine("String weaksum is " + WeakSumString(Encoding.UTF8.GetBytes(Console.ReadLine())));
            }
        }

        static byte[] WeakSum(byte[] inputData)
        {
            // If The Input Data Is Empty, Insert A Zero
            if(inputData.Length == 0) inputData = new byte[] { Convert.ToByte(0) };

            // Build The Inital Connotation
            string initalConnotation = Convert.ToString(inputData.Length, 2);
            foreach (byte b in inputData)
                initalConnotation += Convert.ToString(Convert.ToInt32(b), 2);

            // Convert Inital Connotation To Int64s
            List<ulong> parts = new List<ulong>();
            while(initalConnotation.Length > 0)
            {
                int length = initalConnotation.Length >= 63 ? 63 : initalConnotation.Length;
                parts.Add(Convert.ToUInt64(initalConnotation.Substring(0, length), 2));
                initalConnotation = initalConnotation.Substring(length);
            }

            // Add All Int64s To One BigInteger
            BigInteger totalSum = 0;
            foreach (ulong part in parts) totalSum += part;

            // Binary String of BigInt Turned Into Length Of 16 * 8 Characters
            int targetLength = 16 * 8;
            while (totalSum.ToBinaryString().Length < targetLength) totalSum = BigInteger.Parse(totalSum + "5" + totalSum);
            while (totalSum.ToBinaryString().Length > targetLength) totalSum /= 2;
            while (totalSum.ToBinaryString().Length < targetLength) totalSum *= 2;

            // Scramble The String
            string binaryString = totalSum.ToBinaryString();
            char[] binaryChars = binaryString.ToCharArray();
            Random scrambleRnd = new Random(inputData.Length);
            for(int i = 0; i < 10240; i++)
            {
                int aP = scrambleRnd.Next(0, 127), bP = scrambleRnd.Next(0, 127);
                char aC = binaryChars[aP], bC = binaryChars[bP];
                binaryChars[aP] = bC;
                binaryChars[bP] = aC;
            }
            binaryString = new string(binaryChars);

            // Convert Sum Into Bytes
            byte[] outputData = new byte[16];
            for (int i = 0; i < 16; i++)
                outputData[i] = Convert.ToByte(binaryString.Substring(i * 8, 8), 2);
            return outputData;
        }

        static string WeakSumString(byte[] inputData)
        {
            inputData = WeakSum(inputData);
            StringBuilder hex = new StringBuilder(inputData.Length * 2);
            foreach (byte b in inputData)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }
    }

    /// <summary>
    /// Extension methods to convert <see cref="System.Numerics.BigInteger"/>
    /// instances to hexadecimal, octal, and binary strings.
    /// </summary>
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// Converts a <see cref="BigInteger"/> to a binary string.
        /// </summary>
        /// <param name="bigint">A <see cref="BigInteger"/>.</param>
        /// <returns>
        /// A <see cref="System.String"/> containing a binary
        /// representation of the supplied <see cref="BigInteger"/>.
        /// </returns>
        public static string ToBinaryString(this BigInteger bigint)
        {
            var bytes = bigint.ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base2 = new StringBuilder(bytes.Length * 8);

            // Convert first byte to binary.
            var binary = Convert.ToString(bytes[idx], 2);

            // Ensure leading zero exists if value is positive.
            if (binary[0] != '0' && bigint.Sign == 1)
            {
                base2.Append('0');
            }

            // Append binary string to StringBuilder.
            base2.Append(binary);

            // Convert remaining bytes adding leading zeros.
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            return base2.ToString();
        }

        /// <summary>
        /// Converts a <see cref="BigInteger"/> to a hexadecimal string.
        /// </summary>
        /// <param name="bigint">A <see cref="BigInteger"/>.</param>
        /// <returns>
        /// A <see cref="System.String"/> containing a hexadecimal
        /// representation of the supplied <see cref="BigInteger"/>.
        /// </returns>
        public static string ToHexadecimalString(this BigInteger bigint)
        {
            return bigint.ToString("X");
        }

        /// <summary>
        /// Converts a <see cref="BigInteger"/> to a octal string.
        /// </summary>
        /// <param name="bigint">A <see cref="BigInteger"/>.</param>
        /// <returns>
        /// A <see cref="System.String"/> containing an octal
        /// representation of the supplied <see cref="BigInteger"/>.
        /// </returns>
        public static string ToOctalString(this BigInteger bigint)
        {
            var bytes = bigint.ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base8 = new StringBuilder(((bytes.Length / 3) + 1) * 8);

            // Calculate how many bytes are extra when byte array is split
            // into three-byte (24-bit) chunks.
            var extra = bytes.Length % 3;

            // If no bytes are extra, use three bytes for first chunk.
            if (extra == 0)
            {
                extra = 3;
            }

            // Convert first chunk (24-bits) to integer value.
            int int24 = 0;
            for (; extra != 0; extra--)
            {
                int24 <<= 8;
                int24 += bytes[idx--];
            }

            // Convert 24-bit integer to octal without adding leading zeros.
            var octal = Convert.ToString(int24, 8);

            // Ensure leading zero exists if value is positive.
            if (octal[0] != '0' && bigint.Sign == 1)
            {
                base8.Append('0');
            }

            // Append first converted chunk to StringBuilder.
            base8.Append(octal);

            // Convert remaining 24-bit chunks, adding leading zeros.
            for (; idx >= 0; idx -= 3)
            {
                int24 = (bytes[idx] << 16) + (bytes[idx - 1] << 8) + bytes[idx - 2];
                base8.Append(Convert.ToString(int24, 8).PadLeft(8, '0'));
            }

            return base8.ToString();
        }
    }
}
