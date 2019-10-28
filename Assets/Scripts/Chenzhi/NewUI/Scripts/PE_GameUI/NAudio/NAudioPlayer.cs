using UnityEngine;
using System.IO;
using System;
using NAudio.Wave;
using NAudio.Flac;

public class NAudioPlayer
{
    /// <summary> NAudio支持的解码的音频文件格式</summary>
    public enum SupportFormatType
    {
        NULL,
        mp3,    //lz-2016.12.29 只有windows支持
        flac,
        //wav,  //lz-2016.12.29 用Unity自带的wav解码速度要快一些
        //aiff  //lz-2016.12.29 解码噪音很大
    }

    public static AudioClip GetClipByType(byte[] data, SupportFormatType type)
    {
        try
        {
            MemoryStream memoryStream = new MemoryStream(data);
            WaveStream wave = null;

            switch (type)
            {
                case SupportFormatType.mp3:
                    wave = new Mp3FileReader(memoryStream);
                    break;
                case SupportFormatType.flac:
                    wave = new FlacReader(memoryStream);
                    break;
                    //case SupportFormatType.wav:
                    //    wave = new WaveFileReader(memoryStream);
                    //    break;
                    //case SupportFormatType.aiff:
                    //    wave = new AiffFileReader(memoryStream);
                    //    break;
            }

            WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(wave);
            WAV wav = new WAV(AudioMemStream(waveStream).ToArray());
            Debug.Log(wav);
            AudioClip audioClip = AudioClip.Create(string.Format("{0}Sound", type.ToString()), wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            return audioClip;
        }
        catch (Exception e)
        {
            Debug.Log("NAudioPlayer.GetClipByType() Error:"+e.Message);
            return null;
        }
    }

    private static MemoryStream AudioMemStream(WaveStream waveStream)
    {
        MemoryStream outputStream = new MemoryStream();
        using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
        {
            byte[] bytes = new byte[waveStream.Length];
            waveStream.Position = 0;
            waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
            waveFileWriter.Write(bytes, 0, bytes.Length);
            waveFileWriter.Flush();
        }
        return outputStream;
    }
}

/* From http://answers.unity3d.com/questions/737002/wav-byte-to-audioclip.html */
public class WAV
{

    // convert two bytes to one float in the range -1 to 1
    static float bytesToFloat(byte firstByte, byte secondByte)
    {
        // convert two bytes to one short (little endian)
        short s = (short)((secondByte << 8) | firstByte);
        // convert to range from -1 to (just below) 1
        return s / 32768.0F;
    }

    static int bytesToInt(byte[] bytes, int offset = 0)
    {
        int value = 0;
        for (int i = 0; i < 4; i++)
        {
            value |= ((int)bytes[offset + i]) << (i * 8);
        }
        return value;
    }
    // properties
    public float[] LeftChannel { get; internal set; }
    public float[] RightChannel { get; internal set; }
    public int ChannelCount { get; internal set; }
    public int SampleCount { get; internal set; }
    public int Frequency { get; internal set; }

    public WAV(byte[] wav)
    {
        // Determine if mono or stereo
        ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

        // Get the frequency
        Frequency = bytesToInt(wav, 24);

        // Get past all the other sub chunks to get to the data subchunk:
        int pos = 12;   // First Subchunk ID from 12 to 16

        // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
        while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
        {
            pos += 4;
            int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        // Pos is now positioned to start of actual sound data.
        SampleCount = (int)((wav.Length - pos) * 0.5f);     // 2 bytes per sample (16 bit sound mono)
        if (ChannelCount == 2) SampleCount = (int)(SampleCount* 0.5f);        // 4 bytes per sample (16 bit stereo)

        // Allocate memory (right will be null if only mono sound)
        LeftChannel = new float[SampleCount];
        if (ChannelCount == 2) RightChannel = new float[SampleCount];
        else RightChannel = null;

        // Write to double array/s:
        int i = 0;

        while (i < SampleCount)
        {
            LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
            pos += 2;
            if (ChannelCount == 2)
            {
                RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
            }
            i++;
        }
    }

    public override string ToString()
    {
        return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
    }
}
