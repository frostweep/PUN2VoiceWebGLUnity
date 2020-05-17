using System;
using System.Collections.Generic;

namespace FrostweepGames.WebGLPUNVoice
{
    public sealed class AudioConverter
    {
        public static byte[] FloatToByte(List<float> samples)
        {
            Int16[] intData = new Int16[samples.Count];

            Byte[] bytesData = new Byte[samples.Count * 2];

            int rescaleFactor = 32767;

            for (int i = 0; i < samples.Count; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                Byte[] byteArr = new Byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        public static List<float> ByteToFloat(byte[] bytesData)
        {
            int rescaleFactor = 32767;
            int length = bytesData.Length / 2;
            List<float> samples = new List<float>(length);

            for (int i = 0; i < length; i++)
            {
                samples.Add((float)BitConverter.ToInt16(bytesData, i * 2) / rescaleFactor);
            }

            return samples;
        }
    }
}