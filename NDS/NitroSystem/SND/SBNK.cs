using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using CommonFiles;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND
{
    public class SBNK : FileFormat<SBNK.SBNKIdentifier>
    {
        public SBNK(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SBNKHeader(er);
                SBNKDataSection = new DataSection(er);
            }
            finally
            {
                er.Close();
            }
        }

        public SBNKHeader Header;
        public class SBNKHeader
        {
            public SBNKHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SBNK") throw new SignatureNotCorrectException(Signature, "SBNK", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                Version = er.ReadUInt16();
                FileSize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                NrBlocks = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriterEx er)
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

        public DataSection SBNKDataSection;
        public class DataSection
        {
            public DataSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                er.ReadBytes(32);
                nCount = er.ReadUInt32();
                Ins = new SbnkInstrument[nCount];
                for (int i = 0; i < nCount; i++)
                {
                    Ins[i] = new SbnkInstrument(er);
                }
            }
            public class SbnkInstrument
            {
                public class iRecord0
                {

                }

                public class iRecord_16
                {
                    public iRecord_16(EndianBinaryReader er)
                    {
                        nrSwav = er.ReadInt16();
                        nrSwar = er.ReadInt16();
                        Note = er.ReadByte();
                        AttackRate = er.ReadByte();
                        DecayRate = er.ReadByte();
                        SustainLevel = er.ReadByte();
                        ReleaseRate = er.ReadByte();
                        Pan = er.ReadByte();
                    }
                    public short nrSwav;
                    public short nrSwar;
                    public byte Note;
                    public byte AttackRate;
                    public double RealAttackRate;
                    public byte DecayRate;
                    public double RealDecayRate;
                    public byte SustainLevel;
                    public double RealSustainLevel;
                    public byte ReleaseRate;
                    public double RealReleaseRate;
                    public byte Pan;
                    public double RealPan;
                    public SWAV WaveData;
                }

                public class iRecord16
                {
                    public class info
                    {
                        public info(EndianBinaryReader er)
                        {
                            Unknown = er.ReadBytes(2);
                            nrSwav = er.ReadInt16();
                            nrSwar = er.ReadInt16();
                            Note = er.ReadByte();
                            AttackRate = er.ReadByte();
                            DecayRate = er.ReadByte();
                            SustainLevel = er.ReadByte();
                            ReleaseRate = er.ReadByte();
                            Pan = er.ReadByte();
                        }
                        public byte[] Unknown;
                        public short nrSwav;
                        public short nrSwar;
                        public byte Note;
                        public byte AttackRate;
                        public double RealAttackRate;
                        public byte DecayRate;
                        public double RealDecayRate;
                        public byte SustainLevel;
                        public double RealSustainLevel;
                        public byte ReleaseRate;
                        public double RealReleaseRate;
                        public byte Pan;
                        public double RealPan;
                        public SWAV WaveData;
                    }
                    public iRecord16(EndianBinaryReader er)
                    {
                        lNote = er.ReadByte();
                        uNote = er.ReadByte();
                        int regionCount = uNote - lNote + 1;
                        Info = new info[regionCount];
                        for (int i = 0; i < regionCount; i++)
                        {
                            Info[i] = new info(er);
                        }
                    }
                    public byte lNote;
                    public byte uNote;
                    public info[] Info;
                }

                public class iRecord17
                {
                    public class info
                    {
                        public info(EndianBinaryReader er)
                        {
                            Unknown = er.ReadBytes(2);
                            nrSwav = er.ReadInt16();
                            nrSwar = er.ReadInt16();
                            Note = er.ReadByte();
                            AttackRate = er.ReadByte();
                            DecayRate = er.ReadByte();
                            SustainLevel = er.ReadByte();
                            ReleaseRate = er.ReadByte();
                            Pan = er.ReadByte();
                        }
                        public byte[] Unknown;
                        public short nrSwav;
                        public short nrSwar;
                        public byte Note;
                        public byte AttackRate;
                        public double RealAttackRate;
                        public byte DecayRate;
                        public double RealDecayRate;
                        public byte SustainLevel;
                        public double RealSustainLevel;
                        public byte ReleaseRate;
                        public double RealReleaseRate;
                        public byte Pan;
                        public double RealPan;
                        public SWAV WaveData;
                    }
                    public iRecord17(EndianBinaryReader er)
                    {
                        Regions = new byte[8];
                        for (int i = 0; i < 8; i++)
                        {
                            Regions[i] = er.ReadByte();
                            if (regionCount == 0 && Regions[i] == 0)
                            {
                                regionCount = i;
                            }
                        }
                        Info = new info[regionCount];
                        for (int i = 0; i < regionCount; i++)
                        {
                            Info[i] = new info(er);
                        }
                    }
                    public byte[] Regions;
                    public info[] Info;
                    public int regionCount;
                }
                public SbnkInstrument(EndianBinaryReader er)
                {
                    fRecord = er.ReadByte();
                    nOffset = er.ReadInt16();
                    reserved = er.ReadByte();
                    long position = er.BaseStream.Position;
                    er.BaseStream.Position = nOffset;
                    if (fRecord == 0)
                    {
                        iInfo = new iRecord0();
                    }
                    else if (fRecord < 16)
                    {
                        iInfo = new iRecord_16(er);
                    }
                    else if (fRecord == 16)
                    {
                        iInfo = new iRecord16(er);
                    }
                    else if (fRecord == 17)
                    {
                        iInfo = new iRecord17(er);
                    }
                    er.BaseStream.Position = position;
                }
                public byte fRecord;
                public short nOffset;
                public byte reserved;
                public object iInfo;
            }
            public string Signature;
            public uint SectionSize;
            public uint nCount;
            public SbnkInstrument[] Ins;
        }

        private static byte[] AttackTimeTable = new byte[19]
        {
            0, 1, 5, 14, 26, 38, 51, 63, 73, 84,
            92, 100, 109, 116, 123, 127, 132, 137, 143
        };

        private static ushort[] sustainLevTable = new ushort[128]
        {
            64813, 64814, 64815, 64885, 64935, 64974, 65006, 65033, 65056, 65076,
            65094, 65111, 65126, 65140, 65153, 65165, 65176, 65187, 65197, 65206,
            65215, 65223, 65231, 65239, 65247, 65254, 65260, 65267, 65273, 65279,
            65285, 65291, 65297, 65302, 65307, 65312, 65317, 65322, 65326, 65331,
            65335, 65340, 65344, 65348, 65352, 65356, 65360, 65363, 65367, 65371,
            65374, 65378, 65381, 65384, 65387, 65391, 65394, 65397, 65400, 65403,
            65406, 65409, 65411, 65414, 65417, 65420, 65422, 65425, 65427, 65430,
            65433, 65435, 65437, 65440, 65442, 65445, 65447, 65449, 65451, 65454,
            65456, 65458, 65460, 65462, 65464, 65466, 65468, 65470, 65472, 65474,
            65476, 65478, 65480, 65482, 65484, 65486, 65487, 65489, 65491, 65493,
            65494, 65496, 65498, 65500, 65501, 65503, 65505, 65506, 65508, 65509,
            65511, 65513, 65514, 65516, 65517, 65519, 65520, 65522, 65523, 65525,
            65526, 65528, 65529, 65530, 65532, 65533, 65535, 0
        };

        private static double INTR_FREQUENCY = 1.0 / 192.0;

        public static SBNK InitDLS(SBNK k, SWAR[] waves)
        {
            for (int i = 0; i < k.SBNKDataSection.nCount; i++)
            {
                if (k.SBNKDataSection.Ins[i].fRecord == 0)
                {
                    DataSection.SbnkInstrument.iRecord0 iRecord = (DataSection.SbnkInstrument.iRecord0)k.SBNKDataSection.Ins[i].iInfo;
                }
                else if (k.SBNKDataSection.Ins[i].fRecord < 16)
                {
                    DataSection.SbnkInstrument.iRecord_16 iRecord_ = (DataSection.SbnkInstrument.iRecord_16)k.SBNKDataSection.Ins[i].iInfo;
                    if (k.SBNKDataSection.Ins[i].fRecord != 2 && k.SBNKDataSection.Ins[i].fRecord != 3 && k.SBNKDataSection.Ins[i].fRecord != 5)
                    {
                        iRecord_.WaveData = waves[iRecord_.nrSwar][iRecord_.nrSwav];
                    }
                    else
                    {
                        List<byte> list = new List<byte>();
                        if (k.SBNKDataSection.Ins[i].fRecord == 2)
                        {
                            for (int j = 0; j < iRecord_.nrSwav + 1; j++)
                            {
                                for (int l = 0; l < 21; l++)
                                {
                                    list.Add(127);
                                }
                            }
                            for (int j = 0; j < 8 - (iRecord_.nrSwav + 1); j++)
                            {
                                for (int l = 0; l < 21; l++)
                                {
                                    list.Add(129);
                                }
                            }
                        }
                        else
                        {
                            ushort lfsrState = 32767;
                            for (int j = 0; j < 352800; j++)
                            {
                                if ((lfsrState & 1) != 0)
                                {
                                    lfsrState = (ushort)((lfsrState >> 1) ^ 0x6000);
                                    list.Add(129);
                                }
                                else
                                {
                                    lfsrState >>= 1;
                                    list.Add(127);
                                }
                            }
                        }
                        iRecord_.WaveData = new SWAV(new SWAV.SWAVInfo
                        {
                            bLoop = 1,
                            nNonLoopLen = (uint)list.Count / 4,
                            nLoopOffset = 0,
                            nTime = (ushort)(44100 / list.Count),
                            nWaveType = 0,
                            nSampleRate = 44100
                        }, list.ToArray());
                    }
                    byte attackMult = ((iRecord_.AttackRate < 109) ? ((byte)(255 - iRecord_.AttackRate)) : AttackTimeTable[127 - iRecord_.AttackRate]);
                    short decayFallingRate = (short)GetFallingRate(iRecord_.DecayRate);
                    short releaseFallingRate = (short)GetFallingRate(iRecord_.ReleaseRate);
                    int attackStepCount = 0;
                    for (long attackPhase = 92544; attackPhase != 0; attackPhase = attackPhase * attackMult >> 8)
                    {
                        attackStepCount++;
                    }
                    iRecord_.RealAttackRate = (double)attackStepCount * INTR_FREQUENCY;
                    long sustainValue = ((iRecord_.SustainLevel != 127) ? (65536 - sustainLevTable[iRecord_.SustainLevel] << 7) : 0);
                    if (iRecord_.DecayRate == 127)
                    {
                        iRecord_.RealDecayRate = 0.001;
                    }
                    else
                    {
                        attackStepCount = 92544 / decayFallingRate;
                        iRecord_.RealDecayRate = (double)attackStepCount * INTR_FREQUENCY;
                    }
                    if (sustainValue == 0)
                    {
                        iRecord_.RealSustainLevel = 1.0;
                    }
                    else
                    {
                        iRecord_.RealSustainLevel = (double)(92544 - sustainValue) / 92544.0;
                    }
                    attackStepCount = 92544 / releaseFallingRate;
                    iRecord_.RealReleaseRate = (double)attackStepCount * INTR_FREQUENCY;
                    if (iRecord_.Pan == 0)
                    {
                        iRecord_.RealPan = -1.0;
                    }
                    else if (iRecord_.Pan == 127)
                    {
                        iRecord_.RealPan = 1.0;
                    }
                    else if (iRecord_.Pan == 64)
                    {
                        iRecord_.RealPan = 0.0;
                    }
                    else
                    {
                        iRecord_.RealPan = ((double)(int)iRecord_.Pan - 63.5) / 64.0;
                    }
                    if (iRecord_.RealReleaseRate < 0.001)
                    {
                        iRecord_.RealReleaseRate = 0.001;
                    }
                    if (iRecord_.RealDecayRate < 0.001)
                    {
                        iRecord_.RealDecayRate = 0.001;
                    }
                    if (iRecord_.RealAttackRate < 0.001)
                    {
                        iRecord_.RealAttackRate = 0.001;
                    }
                    long scaledAttackRate = (long)Math.Ceiling(Math.Log(iRecord_.RealAttackRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                    long scaledDecayRate = (long)Math.Ceiling(Math.Log(iRecord_.RealDecayRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                    long scaledSustainLevel = (long)(65536000.0 * iRecord_.RealSustainLevel);
                    long scaledReleaseRate = (long)Math.Ceiling(Math.Log(iRecord_.RealReleaseRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                    long scaledPan = 0L;
                    if (iRecord_.RealPan != 0.0)
                    {
                        scaledPan = (long)(iRecord_.RealPan * 32768000.0);
                    }
                    iRecord_.RealPan = scaledPan;
                    iRecord_.RealAttackRate = scaledAttackRate;
                    iRecord_.RealDecayRate = scaledDecayRate;
                    iRecord_.RealReleaseRate = scaledReleaseRate;
                    iRecord_.RealSustainLevel = scaledSustainLevel;
                    k.SBNKDataSection.Ins[i].iInfo = iRecord_;
                }
                else if (k.SBNKDataSection.Ins[i].fRecord == 16)
                {
                    DataSection.SbnkInstrument.iRecord16 iRecord2 = (DataSection.SbnkInstrument.iRecord16)k.SBNKDataSection.Ins[i].iInfo;
                    int noteCount = iRecord2.uNote - iRecord2.lNote + 1;
                    for (int m = 0; m < noteCount; m++)
                    {
                        iRecord2.Info[m].WaveData = waves[iRecord2.Info[m].nrSwar][iRecord2.Info[m].nrSwav];
                        byte attackMult = ((iRecord2.Info[m].AttackRate < 109) ? ((byte)(255 - iRecord2.Info[m].AttackRate)) : AttackTimeTable[127 - iRecord2.Info[m].AttackRate]);
                        short decayFallingRate = (short)GetFallingRate(iRecord2.Info[m].DecayRate);
                        short releaseFallingRate = (short)GetFallingRate(iRecord2.Info[m].ReleaseRate);
                        int attackStepCount = 0;
                        for (long attackPhase = 92544; attackPhase != 0; attackPhase = attackPhase * attackMult >> 8)
                        {
                            attackStepCount++;
                        }
                        iRecord2.Info[m].RealAttackRate = (double)attackStepCount * INTR_FREQUENCY;
                        long sustainValue = ((iRecord2.Info[m].SustainLevel != 127) ? (65536 - sustainLevTable[iRecord2.Info[m].SustainLevel] << 7) : 0);
                        if (iRecord2.Info[m].DecayRate == 127)
                        {
                            iRecord2.Info[m].RealDecayRate = 0.001;
                        }
                        else
                        {
                            attackStepCount = 92544 / decayFallingRate;
                            iRecord2.Info[m].RealDecayRate = (double)attackStepCount * INTR_FREQUENCY;
                        }
                        if (sustainValue == 0)
                        {
                            iRecord2.Info[m].RealSustainLevel = 1.0;
                        }
                        else
                        {
                            iRecord2.Info[m].RealSustainLevel = (double)(92544 - sustainValue) / 92544.0;
                        }
                        attackStepCount = 92544 / releaseFallingRate;
                        iRecord2.Info[m].RealReleaseRate = (double)attackStepCount * INTR_FREQUENCY;
                        if (iRecord2.Info[m].Pan == 0)
                        {
                            iRecord2.Info[m].RealPan = -1.0;
                        }
                        else if (iRecord2.Info[m].Pan == 127)
                        {
                            iRecord2.Info[m].RealPan = 1.0;
                        }
                        else if (iRecord2.Info[m].Pan == 64)
                        {
                            iRecord2.Info[m].RealPan = 0.0;
                        }
                        else
                        {
                            iRecord2.Info[m].RealPan = ((double)(int)iRecord2.Info[m].Pan - 63.5) / 64.0;
                        }
                        if (iRecord2.Info[m].RealReleaseRate < 0.001)
                        {
                            iRecord2.Info[m].RealReleaseRate = 0.001;
                        }
                        if (iRecord2.Info[m].RealDecayRate < 0.001)
                        {
                            iRecord2.Info[m].RealDecayRate = 0.001;
                        }
                        if (iRecord2.Info[m].RealAttackRate < 0.001)
                        {
                            iRecord2.Info[m].RealAttackRate = 0.001;
                        }
                        long scaledAttackRate = (long)Math.Ceiling(Math.Log(iRecord2.Info[m].RealAttackRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                        long scaledDecayRate = (long)Math.Ceiling(Math.Log(iRecord2.Info[m].RealDecayRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                        long scaledSustainLevel = (long)(65536000.0 * iRecord2.Info[m].RealSustainLevel);
                        long scaledReleaseRate = (long)Math.Ceiling(Math.Log(iRecord2.Info[m].RealReleaseRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                        long scaledPan = 0L;
                        if (iRecord2.Info[m].RealPan != 0.0)
                        {
                            scaledPan = (long)(iRecord2.Info[m].RealPan * 32768000.0);
                        }
                        iRecord2.Info[m].RealPan = scaledPan;
                        iRecord2.Info[m].RealAttackRate = scaledAttackRate;
                        iRecord2.Info[m].RealDecayRate = scaledDecayRate;
                        iRecord2.Info[m].RealReleaseRate = scaledReleaseRate;
                        iRecord2.Info[m].RealSustainLevel = scaledSustainLevel;
                    }
                    k.SBNKDataSection.Ins[i].iInfo = iRecord2;
                }
                else
                {
                    if (k.SBNKDataSection.Ins[i].fRecord != 17)
                    {
                        continue;
                    }
                    DataSection.SbnkInstrument.iRecord17 iRecord3 = (DataSection.SbnkInstrument.iRecord17)k.SBNKDataSection.Ins[i].iInfo;
                    for (int m = 0; m < iRecord3.regionCount; m++)
                    {
                        iRecord3.Info[m].WaveData = waves[iRecord3.Info[m].nrSwar][iRecord3.Info[m].nrSwav];
                        byte attackMult = ((iRecord3.Info[m].AttackRate < 109) ? ((byte)(255 - iRecord3.Info[m].AttackRate)) : AttackTimeTable[127 - iRecord3.Info[m].AttackRate]);
                        short decayFallingRate = (short)GetFallingRate(iRecord3.Info[m].DecayRate);
                        short releaseFallingRate = (short)GetFallingRate(iRecord3.Info[m].ReleaseRate);
                        int attackStepCount = 0;
                        for (long attackPhase = 92544; attackPhase != 0; attackPhase = attackPhase * attackMult >> 8)
                        {
                            attackStepCount++;
                        }
                        iRecord3.Info[m].RealAttackRate = (double)attackStepCount * INTR_FREQUENCY;
                        long sustainValue = ((iRecord3.Info[m].SustainLevel != 127) ? (65536 - sustainLevTable[iRecord3.Info[m].SustainLevel] << 7) : 0);
                        if (iRecord3.Info[m].DecayRate == 127)
                        {
                            iRecord3.Info[m].RealDecayRate = 0.001;
                        }
                        else
                        {
                            attackStepCount = 92544 / decayFallingRate;
                            iRecord3.Info[m].RealDecayRate = (double)attackStepCount * INTR_FREQUENCY;
                        }
                        if (sustainValue == 0)
                        {
                            iRecord3.Info[m].RealSustainLevel = 1.0;
                        }
                        else
                        {
                            iRecord3.Info[m].RealSustainLevel = (double)(92544 - sustainValue) / 92544.0;
                        }
                        attackStepCount = 92544 / releaseFallingRate;
                        iRecord3.Info[m].RealReleaseRate = (double)attackStepCount * INTR_FREQUENCY;
                        if (iRecord3.Info[m].Pan == 0)
                        {
                            iRecord3.Info[m].RealPan = -1.0;
                        }
                        else if (iRecord3.Info[m].Pan == 127)
                        {
                            iRecord3.Info[m].RealPan = 1.0;
                        }
                        else if (iRecord3.Info[m].Pan == 64)
                        {
                            iRecord3.Info[m].RealPan = 0.0;
                        }
                        else
                        {
                            iRecord3.Info[m].RealPan = ((double)(int)iRecord3.Info[m].Pan - 63.5) / 64.0;
                        }
                        if (iRecord3.Info[m].RealReleaseRate < 0.001)
                        {
                            iRecord3.Info[m].RealReleaseRate = 0.001;
                        }
                        if (iRecord3.Info[m].RealDecayRate < 0.001)
                        {
                            iRecord3.Info[m].RealDecayRate = 0.001;
                        }
                        if (iRecord3.Info[m].RealAttackRate < 0.001)
                        {
                            iRecord3.Info[m].RealAttackRate = 0.001;
                        }
                        long scaledAttackRate = (long)Math.Ceiling(Math.Log(iRecord3.Info[m].RealAttackRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                        long scaledDecayRate = (long)Math.Ceiling(Math.Log(iRecord3.Info[m].RealDecayRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                        long scaledSustainLevel = (long)(65536000.0 * iRecord3.Info[m].RealSustainLevel);
                        long scaledReleaseRate = (long)Math.Ceiling(Math.Log(iRecord3.Info[m].RealReleaseRate) / Math.Log(2.0) * 1200.0 * 65536.0);
                        long scaledPan = 0L;
                        if (iRecord3.Info[m].RealPan != 0.0)
                        {
                            scaledPan = (long)(iRecord3.Info[m].RealPan * 32768000.0);
                        }
                        iRecord3.Info[m].RealPan = scaledPan;
                        iRecord3.Info[m].RealAttackRate = scaledAttackRate;
                        iRecord3.Info[m].RealDecayRate = scaledDecayRate;
                        iRecord3.Info[m].RealReleaseRate = scaledReleaseRate;
                        iRecord3.Info[m].RealSustainLevel = scaledSustainLevel;
                    }
                    k.SBNKDataSection.Ins[i].iInfo = iRecord3;
                }
            }
            return k;
        }

        private static ushort GetFallingRate(byte DecayTime)
        {
            ulong fallingRate;
            if (DecayTime == 127)
            {
                fallingRate = 65535;
            }
            else if (DecayTime == 126)
            {
                fallingRate = 15360;
            }
            else if (DecayTime < 50)
            {
                fallingRate = (ulong)DecayTime * 2;
                fallingRate++;
                fallingRate &= 0xFFFF;
            }
            else
            {
                fallingRate = 7680;
                DecayTime = (byte)(126 - DecayTime);
                fallingRate /= DecayTime;
                fallingRate &= 0xFFFF;
            }
            return (ushort)fallingRate;
        }

        public static byte[] ToDLS(SBNK s)
        {
            List<KeyValuePair<int, int>> waveRefList = new List<KeyValuePair<int, int>>();
            List<WAV> waveList = new List<WAV>();
            int instrumentCount = 0;
            for (int i = 0; i < s.SBNKDataSection.Ins.Length; i++)
            {
                if (s.SBNKDataSection.Ins[i].fRecord == 0)
                {
                    DataSection.SbnkInstrument.iRecord0 iRecord = (DataSection.SbnkInstrument.iRecord0)s.SBNKDataSection.Ins[i].iInfo;
                }
                else if (s.SBNKDataSection.Ins[i].fRecord < 16)
                {
                    instrumentCount++;
                    DataSection.SbnkInstrument.iRecord_16 iRecord_ = (DataSection.SbnkInstrument.iRecord_16)s.SBNKDataSection.Ins[i].iInfo;
                    if (s.SBNKDataSection.Ins[i].fRecord == 1 && !waveRefList.Contains(new KeyValuePair<int, int>(iRecord_.nrSwar, iRecord_.nrSwav)))
                    {
                        waveRefList.Add(new KeyValuePair<int, int>(iRecord_.nrSwar, iRecord_.nrSwav));
                        waveList.Add(iRecord_.WaveData.ToWave());
                    }
                    else if (s.SBNKDataSection.Ins[i].fRecord > 1 && !waveRefList.Contains(new KeyValuePair<int, int>(-s.SBNKDataSection.Ins[i].fRecord, iRecord_.nrSwav)))
                    {
                        waveRefList.Add(new KeyValuePair<int, int>(-s.SBNKDataSection.Ins[i].fRecord, iRecord_.nrSwav));
                        waveList.Add(iRecord_.WaveData.ToWave());
                    }
                }
                else if (s.SBNKDataSection.Ins[i].fRecord == 16)
                {
                    instrumentCount++;
                    DataSection.SbnkInstrument.iRecord16 iRecord2 = (DataSection.SbnkInstrument.iRecord16)s.SBNKDataSection.Ins[i].iInfo;
                    int regionCount = iRecord2.uNote - iRecord2.lNote + 1;
                    for (int j = 0; j < regionCount; j++)
                    {
                        if (!waveRefList.Contains(new KeyValuePair<int, int>(iRecord2.Info[j].nrSwar, iRecord2.Info[j].nrSwav)))
                        {
                            waveRefList.Add(new KeyValuePair<int, int>(iRecord2.Info[j].nrSwar, iRecord2.Info[j].nrSwav));
                            waveList.Add(iRecord2.Info[j].WaveData.ToWave());
                        }
                    }
                }
                else
                {
                    if (s.SBNKDataSection.Ins[i].fRecord != 17)
                    {
                        continue;
                    }
                    instrumentCount++;
                    DataSection.SbnkInstrument.iRecord17 iRecord3 = (DataSection.SbnkInstrument.iRecord17)s.SBNKDataSection.Ins[i].iInfo;
                    for (int j = 0; j < iRecord3.regionCount; j++)
                    {
                        if (!waveRefList.Contains(new KeyValuePair<int, int>(iRecord3.Info[j].nrSwar, iRecord3.Info[j].nrSwav)))
                        {
                            waveRefList.Add(new KeyValuePair<int, int>(iRecord3.Info[j].nrSwar, iRecord3.Info[j].nrSwav));
                            waveList.Add(iRecord3.Info[j].WaveData.ToWave());
                        }
                    }
                }
            }
            MemoryStream memoryStream = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(memoryStream, Endianness.LittleEndian);
            List<long> positionStack = new List<long>();
            long savedPosition = 0L;
            er.Write("RIFF", Encoding.ASCII, nullTerminated: false);
            positionStack.Add(er.BaseStream.Position);
            er.Write(0);
            er.Write("DLS colh", Encoding.ASCII, nullTerminated: false);
            er.Write(4);
            er.Write(instrumentCount);
            er.Write("LIST", Encoding.ASCII, nullTerminated: false);
            positionStack.Add(er.BaseStream.Position);
            er.Write(0);
            er.Write("lins", Encoding.ASCII, nullTerminated: false);
            for (int i = 0; i < s.SBNKDataSection.nCount; i++)
            {
                if (s.SBNKDataSection.Ins[i].fRecord == 0)
                {
                    continue;
                }
                er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                positionStack.Add(er.BaseStream.Position);
                er.Write(0);
                er.Write("ins ", Encoding.ASCII, nullTerminated: false);
                er.Write("insh", Encoding.ASCII, nullTerminated: false);
                er.Write(12);
                if (s.SBNKDataSection.Ins[i].fRecord < 16)
                {
                    DataSection.SbnkInstrument.iRecord_16 iRecord_ = (DataSection.SbnkInstrument.iRecord_16)s.SBNKDataSection.Ins[i].iInfo;
                    er.Write(1);
                    er.Write(0);
                    er.Write(i);
                    er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write("lrgn", Encoding.ASCII, nullTerminated: false);
                    er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write("rgn2", Encoding.ASCII, nullTerminated: false);
                    er.Write("rgnh", Encoding.ASCII, nullTerminated: false);
                    er.Write(14);
                    er.Write((short)0);
                    er.Write((short)127);
                    er.Write((short)0);
                    er.Write((short)127);
                    er.Write((short)1);
                    er.Write((short)0);
                    er.Write((short)1);
                    er.Write("wsmp", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write(20);
                    er.Write((short)iRecord_.Note);
                    er.Write((short)0);
                    er.Write(0);
                    er.Write(1);
                    er.Write((int)iRecord_.WaveData.SWAVDataSection.Info.bLoop);
                    if (iRecord_.WaveData.SWAVDataSection.Info.bLoop == 1)
                    {
                        er.Write(16);
                        int loopEndSample = (int)(iRecord_.WaveData.SWAVDataSection.Info.nNonLoopLen * 4);
                        int loopStartSample = iRecord_.WaveData.SWAVDataSection.Info.nLoopOffset * 4;
                        int totalSampleCount = ((iRecord_.WaveData.SWAVDataSection.Info.nWaveType != 2) ? (loopStartSample + loopEndSample) : (loopStartSample + loopEndSample - 4));
                        int isAdpcm = 0;
                        if (iRecord_.WaveData.SWAVDataSection.Info.nWaveType == 2)
                        {
                            isAdpcm = 1;
                            loopStartSample *= 2;
                            loopStartSample = loopStartSample - 8 + 1;
                            loopEndSample = totalSampleCount * 2 + 1 - loopStartSample;
                        }
                        int bytesPerSample = ((iRecord_.WaveData.SWAVDataSection.Info.nWaveType == 0) ? 8 : 16) / 8;
                        double sampleRateMultiplier = 1.0;
                        int loopStart = ((isAdpcm == 0) ? (loopStartSample * (int)sampleRateMultiplier / bytesPerSample) : loopStartSample);
                        int loopLength = ((isAdpcm == 0) ? (loopEndSample * (int)sampleRateMultiplier / bytesPerSample) : loopEndSample);
                        er.Write(0);
                        er.Write(loopStart - 25);
                        er.Write(loopLength);
                    }
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                    er.Write("wlnk", Encoding.ASCII, nullTerminated: false);
                    er.Write(12);
                    er.Write((short)0);
                    er.Write((short)0);
                    er.Write(1);
                    if (s.SBNKDataSection.Ins[i].fRecord == 1)
                    {
                        er.Write(waveRefList.IndexOf(new KeyValuePair<int, int>(iRecord_.nrSwar, iRecord_.nrSwav)));
                    }
                    else
                    {
                        er.Write(waveRefList.IndexOf(new KeyValuePair<int, int>(-s.SBNKDataSection.Ins[i].fRecord, iRecord_.nrSwav)));
                    }
                    er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write("lar2", Encoding.ASCII, nullTerminated: false);
                    er.Write("art2", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write(8);
                    er.Write(5);
                    er.Write((short)0);
                    er.Write((short)0);
                    er.Write((short)4);
                    er.Write((short)0);
                    er.Write((uint)iRecord_.RealPan);
                    er.Write((short)0);
                    er.Write((short)0);
                    er.Write((short)518);
                    er.Write((short)0);
                    er.Write((uint)iRecord_.RealAttackRate);
                    er.Write((short)0);
                    er.Write((short)0);
                    er.Write((short)519);
                    er.Write((short)0);
                    er.Write((uint)iRecord_.RealDecayRate);
                    er.Write((short)0);
                    er.Write((short)0);
                    er.Write((short)522);
                    er.Write((short)0);
                    er.Write((uint)iRecord_.RealSustainLevel);
                    er.Write((short)0);
                    er.Write((short)0);
                    er.Write((short)521);
                    er.Write((short)0);
                    er.Write((uint)iRecord_.RealReleaseRate);
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                }
                else if (s.SBNKDataSection.Ins[i].fRecord == 16)
                {
                    DataSection.SbnkInstrument.iRecord16 iRecord2 = (DataSection.SbnkInstrument.iRecord16)s.SBNKDataSection.Ins[i].iInfo;
                    int regionCount = iRecord2.uNote - iRecord2.lNote + 1;
                    er.Write(regionCount);
                    er.Write((i == 127) ? int.MinValue : 0);
                    er.Write(i);
                    er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write("lrgn", Encoding.ASCII, nullTerminated: false);
                    for (int j = 0; j < regionCount; j++)
                    {
                        er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write("rgn2", Encoding.ASCII, nullTerminated: false);
                        er.Write("rgnh", Encoding.ASCII, nullTerminated: false);
                        er.Write(14);
                        er.Write((short)(iRecord2.lNote + j));
                        er.Write((short)(iRecord2.lNote + j));
                        er.Write((short)0);
                        er.Write((short)127);
                        er.Write((short)1);
                        er.Write((short)0);
                        er.Write((short)1);
                        er.Write("wsmp", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write(20);
                        er.Write((short)iRecord2.Info[j].Note);
                        er.Write((short)0);
                        er.Write(0);
                        er.Write(1);
                        er.Write((int)iRecord2.Info[j].WaveData.SWAVDataSection.Info.bLoop);
                        if (iRecord2.Info[j].WaveData.SWAVDataSection.Info.bLoop == 1)
                        {
                            er.Write(16);
                            int loopEndSample = (int)(iRecord2.Info[j].WaveData.SWAVDataSection.Info.nNonLoopLen * 4);
                            int loopStartSample = iRecord2.Info[j].WaveData.SWAVDataSection.Info.nLoopOffset * 4;
                            int totalSampleCount = ((iRecord2.Info[j].WaveData.SWAVDataSection.Info.nWaveType != 2) ? (loopStartSample + loopEndSample) : (loopStartSample + loopEndSample - 4));
                            int isAdpcm = 0;
                            if (iRecord2.Info[j].WaveData.SWAVDataSection.Info.nWaveType == 2)
                            {
                                isAdpcm = 1;
                                loopStartSample *= 2;
                                loopStartSample = loopStartSample - 8 + 1;
                                loopEndSample = totalSampleCount * 2 + 1 - loopStartSample;
                            }
                            int bytesPerSample = ((iRecord2.Info[j].WaveData.SWAVDataSection.Info.nWaveType == 0) ? 8 : 16) / 8;
                            double sampleRateMultiplier = 1.0;
                            int loopStart = ((isAdpcm == 0) ? (loopStartSample * (int)sampleRateMultiplier / bytesPerSample) : loopStartSample);
                            int loopLength = ((isAdpcm == 0) ? (loopEndSample * (int)sampleRateMultiplier / bytesPerSample) : loopEndSample);
                            er.Write(0);
                            er.Write(loopStart);
                            er.Write(loopLength);
                        }
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                        er.Write("wlnk", Encoding.ASCII, nullTerminated: false);
                        er.Write(12);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write(1);
                        er.Write(waveRefList.IndexOf(new KeyValuePair<int, int>(iRecord2.Info[j].nrSwar, iRecord2.Info[j].nrSwav)));
                        er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write("lar2", Encoding.ASCII, nullTerminated: false);
                        er.Write("art2", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write(8);
                        er.Write(5);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)4);
                        er.Write((short)0);
                        er.Write((uint)iRecord2.Info[j].RealPan);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)518);
                        er.Write((short)0);
                        er.Write((uint)iRecord2.Info[j].RealAttackRate);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)519);
                        er.Write((short)0);
                        er.Write((uint)iRecord2.Info[j].RealDecayRate);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)522);
                        er.Write((short)0);
                        er.Write((uint)iRecord2.Info[j].RealSustainLevel);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)521);
                        er.Write((short)0);
                        er.Write((uint)iRecord2.Info[j].RealReleaseRate);
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                    }
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                }
                else if (s.SBNKDataSection.Ins[i].fRecord == 17)
                {
                    DataSection.SbnkInstrument.iRecord17 iRecord3 = (DataSection.SbnkInstrument.iRecord17)s.SBNKDataSection.Ins[i].iInfo;
                    er.Write(iRecord3.regionCount);
                    er.Write(0);
                    er.Write(i);
                    er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                    positionStack.Add(er.BaseStream.Position);
                    er.Write(0);
                    er.Write("lrgn", Encoding.ASCII, nullTerminated: false);
                    for (int j = 0; j < iRecord3.regionCount; j++)
                    {
                        er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write("rgn2", Encoding.ASCII, nullTerminated: false);
                        er.Write("rgnh", Encoding.ASCII, nullTerminated: false);
                        er.Write(14);
                        er.Write((short)((j != 0) ? (iRecord3.Regions[j - 1] + 1) : 0));
                        er.Write((short)iRecord3.Regions[j]);
                        er.Write((short)0);
                        er.Write((short)127);
                        er.Write((short)1);
                        er.Write((short)0);
                        er.Write((short)1);
                        er.Write("wsmp", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write(20);
                        er.Write((short)iRecord3.Info[j].Note);
                        er.Write((short)0);
                        er.Write(0);
                        er.Write(1);
                        er.Write((int)iRecord3.Info[j].WaveData.SWAVDataSection.Info.bLoop);
                        if (iRecord3.Info[j].WaveData.SWAVDataSection.Info.bLoop == 1)
                        {
                            er.Write(16);
                            int loopEndSample = (int)(iRecord3.Info[j].WaveData.SWAVDataSection.Info.nNonLoopLen * 4);
                            int loopStartSample = iRecord3.Info[j].WaveData.SWAVDataSection.Info.nLoopOffset * 4;
                            int totalSampleCount = ((iRecord3.Info[j].WaveData.SWAVDataSection.Info.nWaveType != 2) ? (loopStartSample + loopEndSample) : (loopStartSample + loopEndSample - 4));
                            int isAdpcm = 0;
                            if (iRecord3.Info[j].WaveData.SWAVDataSection.Info.nWaveType == 2)
                            {
                                isAdpcm = 1;
                                loopStartSample *= 2;
                                loopStartSample = loopStartSample - 8 + 1;
                                loopEndSample = totalSampleCount * 2 + 1 - loopStartSample;
                            }
                            int bytesPerSample = ((iRecord3.Info[j].WaveData.SWAVDataSection.Info.nWaveType == 0) ? 8 : 16) / 8;
                            double sampleRateMultiplier = 1.0;
                            int loopStart = ((isAdpcm == 0) ? (loopStartSample * (int)sampleRateMultiplier / bytesPerSample) : loopStartSample);
                            int loopLength = ((isAdpcm == 0) ? (loopEndSample * (int)sampleRateMultiplier / bytesPerSample) : loopEndSample);
                            er.Write(0);
                            er.Write(loopStart);
                            er.Write(loopLength);
                        }
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                        er.Write("wlnk", Encoding.ASCII, nullTerminated: false);
                        er.Write(12);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write(1);
                        er.Write(waveRefList.IndexOf(new KeyValuePair<int, int>(iRecord3.Info[j].nrSwar, iRecord3.Info[j].nrSwav)));
                        er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write("lar2", Encoding.ASCII, nullTerminated: false);
                        er.Write("art2", Encoding.ASCII, nullTerminated: false);
                        positionStack.Add(er.BaseStream.Position);
                        er.Write(0);
                        er.Write(8);
                        er.Write(5);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)4);
                        er.Write((short)0);
                        er.Write((uint)iRecord3.Info[j].RealPan);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)518);
                        er.Write((short)0);
                        er.Write((uint)iRecord3.Info[j].RealAttackRate);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)519);
                        er.Write((short)0);
                        er.Write((uint)iRecord3.Info[j].RealDecayRate);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)522);
                        er.Write((short)0);
                        er.Write((uint)iRecord3.Info[j].RealSustainLevel);
                        er.Write((short)0);
                        er.Write((short)0);
                        er.Write((short)521);
                        er.Write((short)0);
                        er.Write((uint)iRecord3.Info[j].RealReleaseRate);
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                        savedPosition = er.BaseStream.Position;
                        er.BaseStream.Position = positionStack.Last();
                        er.Write((int)(savedPosition - positionStack.Last() - 4));
                        er.BaseStream.Position = savedPosition;
                        positionStack.Remove(positionStack.Last());
                    }
                    savedPosition = er.BaseStream.Position;
                    er.BaseStream.Position = positionStack.Last();
                    er.Write((int)(savedPosition - positionStack.Last() - 4));
                    er.BaseStream.Position = savedPosition;
                    positionStack.Remove(positionStack.Last());
                }
                er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                positionStack.Add(er.BaseStream.Position);
                er.Write(0);
                er.Write("INFO", Encoding.ASCII, nullTerminated: false);
                er.Write("INAM", Encoding.ASCII, nullTerminated: false);
                positionStack.Add(er.BaseStream.Position);
                er.Write(0);
                er.Write("Unnamed Instrument", Encoding.ASCII, nullTerminated: false);
                er.Write((short)0);
                savedPosition = er.BaseStream.Position;
                er.BaseStream.Position = positionStack.Last();
                er.Write((int)(savedPosition - positionStack.Last() - 4));
                er.BaseStream.Position = savedPosition;
                positionStack.Remove(positionStack.Last());
                savedPosition = er.BaseStream.Position;
                er.BaseStream.Position = positionStack.Last();
                er.Write((int)(savedPosition - positionStack.Last() - 4));
                er.BaseStream.Position = savedPosition;
                positionStack.Remove(positionStack.Last());
                savedPosition = er.BaseStream.Position;
                er.BaseStream.Position = positionStack.Last();
                er.Write((int)(savedPosition - positionStack.Last() - 4));
                er.BaseStream.Position = savedPosition;
                positionStack.Remove(positionStack.Last());
            }
            savedPosition = er.BaseStream.Position;
            er.BaseStream.Position = positionStack.Last();
            er.Write((int)(savedPosition - positionStack.Last() - 4));
            er.BaseStream.Position = savedPosition;
            positionStack.Remove(positionStack.Last());
            er.Write("ptbl", Encoding.ASCII, nullTerminated: false);
            positionStack.Add(er.BaseStream.Position);
            er.Write(0);
            er.Write(8);
            er.Write(waveRefList.Count);
            int cumulativeOffset = 0;
            for (int j = 0; j < waveRefList.Count; j++)
            {
                er.Write(cumulativeOffset);
                cumulativeOffset += waveList[j].Wave.DATA.Data.Length + 12 + 8 + 18 + 8 + 12 + 8 + 14;
            }
            savedPosition = er.BaseStream.Position;
            er.BaseStream.Position = positionStack.Last();
            er.Write((int)(savedPosition - positionStack.Last() - 4));
            er.BaseStream.Position = savedPosition;
            positionStack.Remove(positionStack.Last());
            er.Write("LIST", Encoding.ASCII, nullTerminated: false);
            positionStack.Add(er.BaseStream.Position);
            er.Write(0);
            er.Write("wvpl", Encoding.ASCII, nullTerminated: false);
            for (int j = 0; j < waveList.Count; j++)
            {
                er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                positionStack.Add(er.BaseStream.Position);
                er.Write(0);
                er.Write("wave", Encoding.ASCII, nullTerminated: false);
                er.Write("fmt ", Encoding.ASCII, nullTerminated: false);
                er.Write(18);
                er.Write(Convert.ToUInt16(waveList[j].Wave.FMT.AudioFormat));
                er.Write(waveList[j].Wave.FMT.NrChannel);
                er.Write(waveList[j].Wave.FMT.SampleRate);
                er.Write(waveList[j].Wave.FMT.ByteRate);
                er.Write(waveList[j].Wave.FMT.BlockAlign);
                er.Write(waveList[j].Wave.FMT.BitsPerSample);
                er.Write((short)0);
                er.Write("data", Encoding.ASCII, nullTerminated: false);
                er.Write(waveList[j].Wave.DATA.SectionSize);
                er.Write(waveList[j].Wave.DATA.Data, 0, waveList[j].Wave.DATA.Data.Length);
                er.Write("LIST", Encoding.ASCII, nullTerminated: false);
                positionStack.Add(er.BaseStream.Position);
                er.Write(0);
                er.Write("INFO", Encoding.ASCII, nullTerminated: false);
                er.Write("INAM", Encoding.ASCII, nullTerminated: false);
                positionStack.Add(er.BaseStream.Position);
                er.Write(0);
                er.Write("Unnamed Wave", Encoding.ASCII, nullTerminated: false);
                er.Write((short)0);
                savedPosition = er.BaseStream.Position;
                er.BaseStream.Position = positionStack.Last();
                er.Write((int)(savedPosition - positionStack.Last() - 4));
                er.BaseStream.Position = savedPosition;
                positionStack.Remove(positionStack.Last());
                savedPosition = er.BaseStream.Position;
                er.BaseStream.Position = positionStack.Last();
                er.Write((int)(savedPosition - positionStack.Last() - 4));
                er.BaseStream.Position = savedPosition;
                positionStack.Remove(positionStack.Last());
                savedPosition = er.BaseStream.Position;
                er.BaseStream.Position = positionStack.Last();
                er.Write((int)(savedPosition - positionStack.Last() - 4));
                er.BaseStream.Position = savedPosition;
                positionStack.Remove(positionStack.Last());
            }
            savedPosition = er.BaseStream.Position;
            er.BaseStream.Position = positionStack.Last();
            er.Write((int)(savedPosition - positionStack.Last() - 4));
            er.BaseStream.Position = savedPosition;
            positionStack.Remove(positionStack.Last());
            er.Write("LIST", Encoding.ASCII, nullTerminated: false);
            positionStack.Add(er.BaseStream.Position);
            er.Write(0);
            er.Write("INFO", Encoding.ASCII, nullTerminated: false);
            er.Write("INAM", Encoding.ASCII, nullTerminated: false);
            positionStack.Add(er.BaseStream.Position);
            er.Write(0);
            er.Write("Unnamed Instrumentset", Encoding.ASCII, nullTerminated: false);
            er.Write((short)0);
            savedPosition = er.BaseStream.Position;
            er.BaseStream.Position = positionStack.Last();
            er.Write((int)(savedPosition - positionStack.Last() - 4));
            er.BaseStream.Position = savedPosition;
            positionStack.Remove(positionStack.Last());
            savedPosition = er.BaseStream.Position;
            er.BaseStream.Position = positionStack.Last();
            er.Write((int)(savedPosition - positionStack.Last() - 4));
            er.BaseStream.Position = savedPosition;
            positionStack.Remove(positionStack.Last());
            savedPosition = er.BaseStream.Position;
            er.BaseStream.Position = positionStack.Last();
            er.Write((int)(savedPosition - positionStack.Last() - 4));
            er.BaseStream.Position = savedPosition;
            positionStack.Remove(positionStack.Last());
            byte[] result = memoryStream.ToArray();
            er.Close();
            return result;
        }

        public class SBNKIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Bank (SBNK)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Bank (*.sbnk)|*.sbnk";
            }

            public override Bitmap GetIcon()
            {
                return Resource.guitar;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'B' && File.Data[2] == 'N' && File.Data[3] == 'K') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}