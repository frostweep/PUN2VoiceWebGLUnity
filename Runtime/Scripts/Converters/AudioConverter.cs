using System;
using System.Collections.Generic;
using Microphone = FrostweepGames.MicrophonePro.Microphone;

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

        public static float[] InterleaveChannelsDataFromStream(Microphone.StreamData streamData)
        {
            int length = 0;

            for (int i = 0; i < streamData.ChannelsData.Count; i++)
                length += streamData.ChannelsData[i].Length;

            float[] result = new float[length];
            int index = 0, inputIndex = 0;

            while (index < length)
            {
                for (int i = 0; i < streamData.ChannelsData.Count; i++)
                    result[index++] = streamData.ChannelsData[i][inputIndex];
                inputIndex++;
            }

            return result;
        }
    }
}