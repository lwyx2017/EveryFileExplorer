using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using GCNWii.UI;

namespace GCNWii.JSystem
{
    public class THP:FileFormat<THP.THPIdentifier>, IViewable//,IWriteable
    {
        public THP(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new THPHeader(er);
                er.BaseStream.Position = Header.ComponentDataOffset;
                Components = new THPComponents(er, Header.Version);
                long NextFrameOffset = Header.FirstFrameOffset;
                UInt32 NextFrameSize = Header.FirstFrameSize;
                Frames = new THPFrame[Header.NrFrames];
                for (int i = 0; i < Header.NrFrames; i++)
                {
                    er.BaseStream.Position = NextFrameOffset;
                    Frames[i] = new THPFrame(er, (int)(Components.THPInfos[1] != null ? ((THPComponents.THPAudioInfo)Components.THPInfos[1]).NrData : 0), 
                    (int)(Components.THPInfos[1] != null ? ((THPComponents.THPAudioInfo)Components.THPInfos[1]).NrChannels : 0));
                    UInt32 nextframesize = Frames[i].Header.NextTotalSize;
                    NextFrameOffset += NextFrameSize;
                    NextFrameSize = nextframesize;
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new THPViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "JSystem Cutscenes (*.thp)|*.thp";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.BigEndian);
            Header.Write(er);
            Components.Write(er);
            foreach (THPFrame f in Frames) f.Write(er);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public THPHeader Header;
        public class THPHeader
        {
            public THPHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "THP\0") throw new SignatureNotCorrectException(Signature, "THP\0", er.BaseStream.Position - 4);
                Version = (er.ReadUInt32() == 0x00011000 ? 1.1f : 1.0f);
                MaxBufferSize = er.ReadUInt32();
                MaxAudioSamples = er.ReadUInt32();
                FPS = er.ReadSingle();
                NrFrames = er.ReadUInt32();
                FirstFrameSize = er.ReadUInt32();
                DataSize = er.ReadUInt32();
                ComponentDataOffset = er.ReadUInt32();
                OffsetsDataOffset = er.ReadUInt32();
                FirstFrameOffset = er.ReadUInt32();
                LastFrameOffset = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((UInt32)(Version == 1.1f ? 0x00011000 : 0x00010000));
                er.Write(MaxBufferSize);
                er.Write(MaxAudioSamples);
                er.Write(FPS);
                er.Write(NrFrames);
                er.Write(FirstFrameSize);
                er.Write(DataSize);
                er.Write(ComponentDataOffset);
                er.Write(OffsetsDataOffset);
                er.Write(FirstFrameOffset);
                er.Write(LastFrameOffset);
            }
            public string Signature;
            public Single Version;
            public UInt32 MaxBufferSize;
            public UInt32 MaxAudioSamples;
            public Single FPS;
            public UInt32 NrFrames;
            public UInt32 FirstFrameSize;
            public UInt32 DataSize;
            public UInt32 ComponentDataOffset;
            public UInt32 OffsetsDataOffset;
            public UInt32 FirstFrameOffset;
            public UInt32 LastFrameOffset;
        }

        public THPComponents Components;
        public class THPComponents
        {
            public THPComponents(EndianBinaryReader er, float Version)
            {
                NrComponents = er.ReadUInt32();
                ComponentTypes = er.ReadBytes(16);
                THPInfos = new THPInfo[16];
                for (int i = 0; i < 16; i++)
                {
                    switch (ComponentTypes[i])
                    {
                        case 0://Video
                            THPInfos[i] = new THPVideoInfo(er, Version);
                            break;
                        case 1://Audio
                            THPInfos[i] = new THPAudioInfo(er, Version);
                            break;
                        case 0xFF://Nothing
                            THPInfos[i] = null;
                            break;
                    }
                }
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(NrComponents);
                er.Write(ComponentTypes, 0, 16);
                foreach (THPInfo i in THPInfos)
                {
                    if (i != null) i.Write(er);
                }
            }
            public UInt32 NrComponents;
            public Byte[] ComponentTypes; //16
            public THPInfo[] THPInfos;//16

            public class THPInfo { public virtual void Write(EndianBinaryWriter er) { } }
            public class THPVideoInfo : THPInfo
            {
                public THPVideoInfo(EndianBinaryReader er, float Version)
                {
                    Width = er.ReadUInt32();
                    Height = er.ReadUInt32();
                    if (Version == 1.1f) VideoFormat = er.ReadUInt32();
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(Width);
                    er.Write(Height);
                    er.Write(VideoFormat);
                }
                public UInt32 Width;
                public UInt32 Height;
                public UInt32 VideoFormat;
            }
            public class THPAudioInfo : THPInfo
            {
                public THPAudioInfo(EndianBinaryReader er, float Version)
                {
                    NrChannels = er.ReadUInt32();
                    Frequentie = er.ReadUInt32();
                    NrSamples = er.ReadUInt32();
                    if (Version == 1.1f) NrData = er.ReadUInt32();
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(NrChannels);
                    er.Write(Frequentie);
                    er.Write(NrSamples);
                    er.Write(NrData);
                }
                public UInt32 NrChannels;
                public UInt32 Frequentie;
                public UInt32 NrSamples;
                public UInt32 NrData;
            }
        }

        public THPFrame[] Frames;
        public class THPFrame
        {
            public THPFrame(EndianBinaryReader er, int NrAudioData, int NrAudioChannels)
            {
                Header = new THPFrameHeader(er, NrAudioData > 0);
                ImageData = new byte[Header.ImageSize];
                er.BaseStream.Read(ImageData, 0, (int)Header.ImageSize);
                if (NrAudioData > 0)
                {
                    AudioFrames = new THPAudioFrame[NrAudioData];
                    for (int i = 0; i < NrAudioData; i++) AudioFrames[i] = new THPAudioFrame(er, NrAudioChannels);
                }
            }

            public void Write(EndianBinaryWriter er)
            {
                Header.Write(er);
                er.Write(ImageData, 0, ImageData.Length);
                if (AudioFrames != null)
                {
                    AudioFrames[0].Write(er);
                    uint size = (UInt32)(ImageData.Length + 16 + 80 + AudioFrames[0].Data.Length);
                    er.Write(new byte[size % 4], 0, (int)size % 4);
                }
                else
                {
                    uint size = (UInt32)(ImageData.Length + 12);
                    er.Write(new byte[size % 4], 0, (int)size % 4);
                }
            }
            public THPFrameHeader Header;
            public byte[] ImageData;
            public THPAudioFrame[] AudioFrames;
            public class THPFrameHeader
            {
                public THPFrameHeader(EndianBinaryReader er, bool ContainsAudio)
                {
                    NextTotalSize = er.ReadUInt32();
                    PrevTotalSize = er.ReadUInt32();
                    ImageSize = er.ReadUInt32();
                    if (ContainsAudio) AudioSize = er.ReadUInt32();
                }
                public void Write(EndianBinaryWriter er)
                {
                    er.Write(NextTotalSize);
                    er.Write(PrevTotalSize);
                    er.Write(ImageSize);
                    if (AudioSize != 0) er.Write(AudioSize);
                }
                public UInt32 NextTotalSize;
                public UInt32 PrevTotalSize;
                public UInt32 ImageSize;
                public UInt32 AudioSize;
            }
            public class THPAudioFrame
            {
                public THPAudioFrame(EndianBinaryReader er, int NrAudioChannels)
                {
                    Header = new THPAudioFrameHeader(er);
                    Data = new byte[(int)Header.ChannelSize * NrAudioChannels];
                    er.BaseStream.Read(Data, 0, (int)Header.ChannelSize * NrAudioChannels);

                }
                
                public void Write(EndianBinaryWriter er)
                {
                    Header.Write(er);
                    er.Write(Data, 0, Data.Length);
                }

                public THPAudioFrameHeader Header;
                public class THPAudioFrameHeader
                {
                    public THPAudioFrameHeader(EndianBinaryReader er)
                    {
                        ChannelSize = er.ReadUInt32();
                        NrSamples = er.ReadUInt32();
                        Table1 = er.ReadInt16s(16);
                        Table2 = er.ReadInt16s(16);
                        Channel1Prev1 = er.ReadInt16();
                        Channel1Prev2 = er.ReadInt16();
                        Channel2Prev1 = er.ReadInt16();
                        Channel2Prev2 = er.ReadInt16();
                    }
                    public void Write(EndianBinaryWriter er)
                    {
                        er.Write(ChannelSize);
                        er.Write(NrSamples);
                        er.Write(Table1, 0, 16);
                        er.Write(Table2, 0, 16);
                        er.Write(Channel1Prev1);
                        er.Write(Channel1Prev2);
                        er.Write(Channel2Prev1);
                        er.Write(Channel2Prev2);
                    }
                    public UInt32 ChannelSize;
                    public UInt32 NrSamples;
                    public short[] Table1;
                    public short[] Table2;
                    public Int16 Channel1Prev1;
                    public Int16 Channel1Prev2;
                    public Int16 Channel2Prev1;
                    public Int16 Channel2Prev2;
                }
                public byte[] Data;
            }
            public Bitmap ToBitmap()
            {
                byte[] Result;
                int start = 0, end = 0;
                unsafe
                {
                    byte* data = stackalloc byte[ImageData.Length];
                    Marshal.Copy(ImageData, 0, (IntPtr)data, ImageData.Length);
                    int newSize = countRequiredSize(data, ImageData.Length, ref start, ref end);
                    byte* buff = stackalloc byte[newSize];
                    convertToRealJpeg(buff, data, ImageData.Length, start, end);
                    Result = new byte[newSize];
                    Marshal.Copy((IntPtr)buff, Result, 0, newSize);
                }
                return new Bitmap(new MemoryStream(Result));
            }
            private unsafe int countRequiredSize(byte* data, int size, ref int start, ref int end)
            {
                start = 2 * size;
                int count = 0;
                int j;
                for (j = size - 1; data[j] == 0; --j); //search end of data
                if (data[j] == 0xd9) //thp file
                    end = j - 1;
                else if (data[j] == 0xff) //mth file
                    end = j - 2;
                for (int i = 0; i < end; ++i)
                {
                    if (data[i] == 0xff)
                    {
                        if (data[i + 1] == 0xda && start == 2 * size)
                            start = i;
                        if (i > start)
                            ++count;
                    }
                }
                return size + count;
            }

            private unsafe void convertToRealJpeg(byte* dest, byte* src, int srcSize, int start, int end)
            {
                int di = 0;
                for (int i = 0; i < srcSize; ++i, ++di)
                {
                    dest[di] = src[i];
                    if (src[i] == 0xff && i > start && i < end)
                    {
                        ++di;
                        dest[di] = 0;
                    }
                }
            }

            private unsafe struct DecStruct
            {
                public byte* currSrcByte;
                public UInt32 blockCount;
                public byte index;
                public byte shift;
            }

            private unsafe void thpAudioInitialize(ref DecStruct s, byte* srcStart)
            {
                s.currSrcByte = srcStart;
                s.blockCount = 2;
                s.index = (byte)((*s.currSrcByte >> 4) & 0x7);
                s.shift = (byte)(*s.currSrcByte & 0xf);
                ++s.currSrcByte;
            }
            private unsafe Int32 thpAudioGetNewSample(ref DecStruct s)
            {
                if ((s.blockCount & 0xf) == 0)
                {
                    s.index = (byte)((*s.currSrcByte >> 4) & 0x7);
                    s.shift = (byte)(*s.currSrcByte & 0xf);
                    ++s.currSrcByte;
                    s.blockCount += 2;
                }

                Int32 ret;
                if ((s.blockCount & 1) != 0)
                {
                    Int32 t = (Int32)((*s.currSrcByte << 28) & 0xf0000000);
                    ret = t >> 28; //this has to be an arithmetic shift
                    ++s.currSrcByte;
                }
                else
                {
                    Int32 t = (Int32)((*s.currSrcByte << 24) & 0xf0000000);
                    ret = t >> 28; //this has to be an arithmetic shift
                }

                ++s.blockCount;
                return ret;
            }

            private unsafe int thpAudioDecode(Int16* destBuffer, THPFrame.THPAudioFrame Frame, bool separateChannelsInOutput, bool isInputStereo)
            {
                if (destBuffer == null || Frame == null) 
                    return 0;
                UInt32 channelInSize = Frame.Header.ChannelSize;
                UInt32 numSamples = Frame.Header.NrSamples;
                byte* srcChannel1 = stackalloc byte[Frame.Data.Length];
                Marshal.Copy(Frame.Data, 0, (IntPtr)srcChannel1, Frame.Data.Length);
                byte* srcChannel2 = srcChannel1 + channelInSize;
                Int16* table1 = stackalloc Int16[16];
                Marshal.Copy(Frame.Header.Table1, 0, (IntPtr)table1, 16);
                Int16* table2 = stackalloc Int16[16];
                Marshal.Copy(Frame.Header.Table2, 0, (IntPtr)table2, 16);
                Int16* destChannel1, destChannel2;
                UInt32 delta;
                if (separateChannelsInOutput)
                {
                    //separated channels in output
                    destChannel1 = destBuffer;
                    destChannel2 = destBuffer + numSamples;
                    delta = 1;
                }
                else
                {
                    //interleaved channels in output
                    destChannel1 = destBuffer;
                    destChannel2 = destBuffer + 1;
                    delta = 2;
                }
                DecStruct s = new DecStruct();
                if (!isInputStereo)
                {
                    //mono channel in input
                    thpAudioInitialize(ref s, srcChannel1);
                    Int16 prev1 = Frame.Header.Channel1Prev1;// *(Int16*)(srcBuffer + 72);
                    Int16 prev2 = Frame.Header.Channel1Prev2;//*(Int16*)(srcBuffer + 74);
                    for (int i = 0; i < numSamples; ++i)
                    {
                        Int64 res = (Int64)thpAudioGetNewSample(ref s);
                        res = ((res << s.shift) << 11); //convert to 53.11 fixedpoint
                        //these values are 53.11 fixed point numbers
                        Int64 val1 = table1[2 * s.index];
                        Int64 val2 = table1[2 * s.index + 1];
                        //convert to 48.16 fixed point
                        res = (val1 * prev1 + val2 * prev2 + res) << 5;
                        //rounding:
                        UInt16 decimalPlaces = (UInt16)(res & 0xffff);
                        if (decimalPlaces > 0x8000) //i.e. > 0.5
                                                    //round up
                            ++res;
                        else if (decimalPlaces == 0x8000) //i.e. == 0.5
                            if ((res & 0x10000) != 0)
                                //round up every other number
                                ++res;

                        //get nonfractional parts of number, clamp to [-32768, 32767]
                        Int32 final = (Int32)(res >> 16);
                        if (final > 32767) final = 32767;
                        else if (final < -32768) final = -32768;

                        prev2 = prev1;
                        prev1 = (Int16)final;
                        *destChannel1 = (Int16)final;
                        *destChannel2 = (Int16)final;
                        destChannel1 += delta;
                        destChannel2 += delta;
                    }
                }
                else
                {
                    //two channels in input - nearly the same as for one channel,
                    //so no comments here (different lines are marked with XXX)
                    thpAudioInitialize(ref s, srcChannel1);
                    Int16 prev1 = Frame.Header.Channel1Prev1;// *(Int16*)(srcBuffer + 72);
                    Int16 prev2 = Frame.Header.Channel1Prev2;//*(Int16*)(srcBuffer + 74);
                    for (int i = 0; i < numSamples; ++i)
                    {
                        Int64 res = (Int64)thpAudioGetNewSample(ref s);
                        res = ((res << s.shift) << 11);
                        Int64 val1 = table1[2 * s.index];
                        Int64 val2 = table1[2 * s.index + 1];
                        res = (val1 * prev1 + val2 * prev2 + res) << 5;
                        UInt16 decimalPlaces = (UInt16)(res & 0xffff);
                        if (decimalPlaces > 0x8000)
                            ++res;
                        else if (decimalPlaces == 0x8000)
                            if ((res & 0x10000) != 0)
                                ++res;
                        Int32 final = (Int32)(res >> 16);
                        if (final > 32767) final = 32767;
                        else if (final < -32768) final = -32768;
                        prev2 = prev1;
                        prev1 = (Int16)final;
                        *destChannel1 = (Int16)final;
                        destChannel1 += delta;
                    }

                    thpAudioInitialize(ref s, srcChannel2);//XXX
                    prev1 = Frame.Header.Channel2Prev1;// *(Int16*)(srcBuffer + 72);
                    prev2 = Frame.Header.Channel2Prev2;//*(Int16*)(srcBuffer + 74);
                    for (int j = 0; j < numSamples; ++j)
                    {
                        Int64 res = (Int64)thpAudioGetNewSample(ref s);
                        res = ((res << s.shift) << 11);
                        Int64 val1 = table2[2 * s.index];//XXX
                        Int64 val2 = table2[2 * s.index + 1];//XXX
                        res = (val1 * prev1 + val2 * prev2 + res) << 5;
                        UInt16 decimalPlaces = (UInt16)(res & 0xffff);
                        if (decimalPlaces > 0x8000)
                            ++res;
                        else if (decimalPlaces == 0x8000)
                            if ((res & 0x10000) != 0)
                                ++res;
                        Int32 final = (Int32)(res >> 16);
                        if (final > 32767) final = 32767;
                        else if (final < -32768) final = -32768;
                        prev2 = prev1;
                        prev1 = (Int16)final;
                        *destChannel2 = (Int16)final;
                        destChannel2 += delta;
                    }
                }
                return (int)numSamples;
            }

            public void ToPCM16(out byte[] Output)
            {
                if (AudioFrames == null || AudioFrames.Length == 0)
                {
                    Output = null;
                    return;
                }
                unsafe
                {
                    var audioFrame = AudioFrames[0];
                    int channelCount = audioFrame.Data.Length == audioFrame.Header.ChannelSize * 2 ? 2 : 1;
                    int sampleCount = (int)audioFrame.Header.NrSamples;
                    short* dst = stackalloc short[sampleCount * channelCount];
                    thpAudioDecode(dst, audioFrame, false, channelCount == 2);
                    Output = new byte[sampleCount * channelCount * 2];
                    Marshal.Copy((IntPtr)dst, Output, 0, Output.Length);
                }
            }
        }

        public THPFrame GetFrame(int Frame)
        {
            return Frames[Frame];
        }
        public void Close()
        {
            Frames = new THPFrame[0];
        }

        public class THPIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Videos;
            }

            public override string GetFileDescription()
            {
                return "JSystem Cutscenes (THP)";
            }

            public override string GetFileFilter()
            {
                return "JSystem Cutscenes (*.thp)|*.thp";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'T' && File.Data[1] == 'H' && File.Data[2] == 'P') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}