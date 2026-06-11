using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonFiles;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.SND;
using NDS.UI;

namespace NDS.NitroSystem.SND
{
    public class STRM : FileFormat<STRM.STRMIdentifier>, IViewable, IConvertable, IWriteable, IFileCreatable
    {
        public STRM()
        {
            Header = new STRMHeader();
            Head = new HeadSection();
            StrmData = new DataSection();
        }
        public STRM(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new STRMHeader(er);
                Head = new HeadSection(er);
                StrmData = new DataSection(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new STRMViewer(this);
        }

        public string GetConversionFileFilters()
        {
            return "Wave File (*.wav)|*.wav";
        }

        public bool Convert(int FilterIndex, String Path)
        {
            switch (FilterIndex)
            {
                case 0:
                    byte[] Data = ToWave().Write();
                    File.Create(Path).Close();
                    File.WriteAllBytes(Path, Data);
                    return true;
                default:
                    return false;
            }
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Stream (*.strm)|*.strm";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            Head.Write(er);
            StrmData.Write(er);
            long endPos = m.Position;
            m.Position = 8;
            er.Write((uint)endPos);
            m.Position = endPos;
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public bool CreateFromFile()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = WAV.Identifier.GetFileFilter();
            if (f.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(f.FileName))
            {
                try
                {
                    WAV wav = new WAV(File.ReadAllBytes(f.FileName));
                    ushort bitsPerSample = wav.Wave.FMT.BitsPerSample;
                    ushort channels = wav.Wave.FMT.NrChannel;
                    byte waveType;
                    if (bitsPerSample == 16)
                        waveType = 2;
                    else if (bitsPerSample == 8)
                        waveType = 0;
                    else
                        return false;
                    List<byte[]> channelDatas = new List<byte[]>();
                    for (int ch = 0; ch < channels; ch++) channelDatas.Add(wav.GetChannelData(ch));
                    List<byte[]> encodedChannels = new List<byte[]>();
                    uint samplesPerChannel = 0;
                    foreach (var chData in channelDatas)
                    {
                        if (waveType == 2)
                        {
                            short[] pcm16 = new short[chData.Length / 2];
                            Buffer.BlockCopy(chData, 0, pcm16, 0, chData.Length);
                            IMAADPCMEncoder encoder = new IMAADPCMEncoder();
                            byte[] adpcm = encoder.Encode(pcm16);
                            encodedChannels.Add(adpcm);
                            if (samplesPerChannel == 0)
                                samplesPerChannel = (uint)pcm16.Length;
                        }
                        else if (waveType == 0)
                        {
                            byte[] signedData = new byte[chData.Length];
                            for (int i = 0; i < chData.Length; i++)
                                signedData[i] = (byte)(chData[i] ^ 0x80);
                            encodedChannels.Add(signedData);
                            if (samplesPerChannel == 0)
                                samplesPerChannel = (uint)chData.Length;
                        }
                    }
                    byte[] dataBlock;
                    if (channels == 1)
                    {
                        dataBlock = encodedChannels[0];
                    }
                    else
                    {
                        dataBlock = new byte[encodedChannels[0].Length + encodedChannels[1].Length];
                        Buffer.BlockCopy(encodedChannels[0], 0, dataBlock, 0, encodedChannels[0].Length);
                        Buffer.BlockCopy(encodedChannels[1], 0, dataBlock, encodedChannels[0].Length, encodedChannels[1].Length);
                    }
                    Header = new STRMHeader();
                    Head = new HeadSection();
                    StrmData = new DataSection();
                    Head.NrChannel = (byte)channels;
                    Head.nSampleRate = (ushort)wav.Wave.FMT.SampleRate;
                    Head.nWaveType = waveType;
                    Head.bLoop = 0;
                    Head.nLoopOffset = 0;
                    Head.nTime = (ushort)(samplesPerChannel / wav.Wave.FMT.SampleRate);
                    Head.nBlock = 1;
                    Head.nBlockLength = 0;
                    Head.nBlockSample = 0;
                    Head.nLastBlockLen = (uint)encodedChannels[0].Length;
                    Head.nLastBlockSample = samplesPerChannel;
                    Head.nSample = samplesPerChannel;
                    Head.SectionSize = 0x50;
                    Head.nDataOffset = 0x10 + 8 + Head.SectionSize;
                    StrmData.Data = dataBlock;
                    StrmData.SectionSize = (uint)(8 + dataBlock.Length);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public STRMHeader Header;

        public class STRMHeader
        {
            public STRMHeader()
            {
                Signature = "STRM";
                Endianness = 0xFEFF;
                HeaderSize = 0x10;
                Version = 256;
                NrBlocks = 2;
            }
            public STRMHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "STRM") throw new SignatureNotCorrectException(Signature, "STRM", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                Version = er.ReadUInt16();
                FileSize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                NrBlocks = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(Version);
                er.Write(FileSize);
                er.Write(HeaderSize);
                er.Write(NrBlocks);
            }
            public string Signature;
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 NrBlocks;
        }

        public HeadSection Head;
        public class HeadSection
        {
            public HeadSection()
            {
                Signature = "HEAD";
                SectionSize = 0x80;
                NrChannel = 1;
                nBlock = 1;
                nWaveType = 0;
            }
            public HeadSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "HEAD") throw new SignatureNotCorrectException(Signature, "HEAD", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                nWaveType = er.ReadByte();
                bLoop = er.ReadByte();
                NrChannel = er.ReadByte();
                Unknown = er.ReadByte();
                nSampleRate = er.ReadUInt16();
                nTime = er.ReadUInt16();
                nLoopOffset = er.ReadUInt32();
                nSample = er.ReadUInt32();
                nDataOffset = er.ReadUInt32();
                nBlock = er.ReadUInt32();
                nBlockLength = er.ReadUInt32();
                nBlockSample = er.ReadUInt32();
                nLastBlockLen = er.ReadUInt32();
                nLastBlockSample = er.ReadUInt32();
                er.ReadBytes(32);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                er.Write(nWaveType);
                er.Write(bLoop);
                er.Write(NrChannel);
                er.Write(Unknown);
                er.Write(nSampleRate);
                er.Write(nTime);
                er.Write(nLoopOffset);
                er.Write(nSample);
                er.Write(nDataOffset);
                er.Write(nBlock);
                er.Write(nBlockLength);
                er.Write(nBlockSample);
                er.Write(nLastBlockLen);
                er.Write(nLastBlockSample);
                er.Write(new byte[32]);
            }
            public string Signature;
            public uint SectionSize;
            public byte nWaveType;
            public byte bLoop;
            public byte NrChannel;
            public byte Unknown;
            public ushort nSampleRate;
            public ushort nTime;
            public uint nLoopOffset;
            public uint nSample;
            public uint nDataOffset;
            public uint nBlock;
            public uint nBlockLength;
            public uint nBlockSample;
            public uint nLastBlockLen;
            public uint nLastBlockSample;
        }

        public DataSection StrmData;
        public class DataSection
        {
            public DataSection()
            {
                Signature = "DATA";
                Data = Array.Empty<byte>();
                SectionSize = 8;
            }
            public DataSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                Data = er.ReadBytes((int)(SectionSize - 8));
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((uint)(8 + Data.Length));
                er.Write(Data);
            }
            public string Signature;
            public uint SectionSize;
            public byte[] Data;
        }

        private byte[] GetBlock(int WaveType, uint nBlockLen, byte[] Data, ref int offset)
        {
            byte[] array;
            switch (WaveType)
            {
                case 2:
                    {
                        IMAADPCMDecoder decoder = new IMAADPCMDecoder();
                        short[] pcmSamples = decoder.GetWaveData(Data, offset, (int)nBlockLen);
                        array = new byte[pcmSamples.Length * 2];
                        Buffer.BlockCopy(pcmSamples, 0, array, 0, array.Length);
                        break;
                    }
                case 1:
                    array = Data.ToList().GetRange(offset, (int)nBlockLen).ToArray();
                    break;
                default:
                    array = IOUtil.SignedPCM8ToUnsigned(Data.ToList().GetRange(offset, (int)nBlockLen).ToArray());
                    break;
            }
            offset += (int)nBlockLen;
            return array;
        }

        private byte[] CombineStereo(byte[] Left, byte[] Right, int WaveType)
        {
            List<byte> list = new List<byte>();
            if (WaveType > 0)
            {
                for (int i = 0; i < Left.Length; i += 2)
                {
                    list.Add(Left[i]);
                    list.Add(Left[i + 1]);
                    list.Add(Right[i]);
                    list.Add(Right[i + 1]);
                }
            }
            else
            {
                for (int i = 0; i < Left.Length; i++)
                {
                    list.Add(Left[i]);
                    list.Add(Right[i]);
                }
            }
            return list.ToArray();
        }

        private byte[] GetChannelsStereo(byte[] Data, uint nBlock, uint nBlockLen, uint nLastBlockLen, int WaveType)
        {
            List<byte> left = new List<byte>();
            List<byte> right = new List<byte>();
            int offset = 0;
            for (int i = 0; i < nBlock - 1; i++)
            {
                left.AddRange(GetBlock(WaveType, nBlockLen, Data, ref offset));
                right.AddRange(GetBlock(WaveType, nBlockLen, Data, ref offset));
            }
            left.AddRange(GetBlock(WaveType, nLastBlockLen, Data, ref offset));
            right.AddRange(GetBlock(WaveType, nLastBlockLen, Data, ref offset));
            return CombineStereo(left.ToArray(), right.ToArray(), WaveType);
        }

        private byte[] GetChannelMono(byte[] Data, uint nBlock, uint nBlockLen, uint nLastBlockLen, int WaveType)
        {
            List<byte> list = new List<byte>();
            int offset = 0;
            for (int i = 0; i < nBlock - 1; i++)
            {
                list.AddRange(GetBlock(WaveType, nBlockLen, Data, ref offset));
            }
            list.AddRange(GetBlock(WaveType, nLastBlockLen, Data, ref offset));
            return list.ToArray();
        }

        public WAV ToWave()
        {
            byte[] rawData = new byte[StrmData.Data.Length];
            Buffer.BlockCopy(StrmData.Data, 0, rawData, 0, rawData.Length);
            byte[] decodedData;
            if (Head.NrChannel == 2)
            {
                decodedData = GetChannelsStereo(rawData, Head.nBlock, Head.nBlockLength, Head.nLastBlockLen, Head.nWaveType);
            }
            else
            {
                decodedData = GetChannelMono(rawData, Head.nBlock, Head.nBlockLength, Head.nLastBlockLen, Head.nWaveType);
            }
            if (Head.nWaveType == 0)
            {
                return new WAV(decodedData, Head.nSampleRate, 8, Head.NrChannel);
            }
            return new WAV(decodedData, Head.nSampleRate, 16, Head.NrChannel);
        }

        public class STRMIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Audio;
            }

            public override string GetFileDescription()
            {
                return "Nitro Stream (STRM)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Stream (*.strm)|*.strm";
            }

            public override Bitmap GetIcon()
            {
                return Resource.speaker;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'T' && File.Data[2] == 'R' && File.Data[3] == 'M') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}