using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NAudio.Midi;
using NDS.UI;

namespace NDS.NitroSystem.SND
{
    public class SSEQ : FileFormat<SSEQ.SSEQIdentifier>,IViewable
    {
        public SSEQ(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SSEQHeader(er);
                SSEQDataSection = new DataSection(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new SSEQViewer(this);
        }

        public SSEQHeader Header;
        public class SSEQHeader
        {
            public SSEQHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SSEQ") throw new SignatureNotCorrectException(Signature, "SSEQ", er.BaseStream.Position - 4);
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

        public DataSection SSEQDataSection;
        public class DataSection
        {
            public DataSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                SequenceOffset = er.ReadUInt32();
                int count = (int)(er.BaseStream.Length - er.BaseStream.Position);
                Data = er.ReadBytes(count);
                SSEQDecoder SSEQDec = new SSEQDecoder(Data);
                IList<MidiEvent>[] tracks = SSEQDec.GetTracks();
                loopend = SSEQDec.LoopEndTickOffset;
                loopstart = SSEQDec.LoopStartTickOffset;
                SequenceTreeNodes.Clear();
                SequenceTreeNodes.AddRange(SSEQDec.BuildSequenceTreeNodes());
                MidiEventCollection midiEventCollection = new MidiEventCollection(1, 48);
                IList<MidiEvent>[] array = tracks;
                foreach (IList<MidiEvent> initialEvents in array)
                {
                    midiEventCollection.AddTrack(initialEvents);
                }
                Midi = midiEventCollection;
                for (int i = 0; i < tracks.Length; i++)
                {
                    TrackNames.Add($"Track {i + 1}");
                }
            }
            public DataSection(byte[] Notes, int Offset)
            {
                SSEQDecoder SSEQDec = new SSEQDecoder(Notes, Offset);
                IList<MidiEvent>[] tracks = SSEQDec.GetTracks();
                loopend = SSEQDec.LoopEndTickOffset;
                loopstart = SSEQDec.LoopStartTickOffset;
                SequenceTreeNodes.Clear();
                SequenceTreeNodes.AddRange(SSEQDec.BuildSequenceTreeNodes());
                MidiEventCollection midiEventCollection = new MidiEventCollection(1, 48);
                IList<MidiEvent>[] array = tracks;
                foreach (IList<MidiEvent> initialEvents in array)
                {
                    midiEventCollection.AddTrack(initialEvents);
                }
                Midi = midiEventCollection;
                for (int i = 0; i < tracks.Length; i++)
                {
                    TrackNames.Add($"Track {i + 1}");
                }
            }
            public IReadOnlyList<TreeNode> GetSequenceTreeData() => SequenceTreeNodes;
            public IReadOnlyList<string> GetTrackNameList() => TrackNames;
            public int GetLoopStart()
            {
                return loopstart;
            }

            public int GetLoopEnd()
            {
                return loopend;
            }

            public int GetNrLoop()
            {
                return nrloop;
            }

            public class SSEQEvent
            {
                public int Type;

                public byte[] Params;

                public SSEQEvent(int Type, byte[] Params)
                {
                    this.Type = Type;
                    this.Params = Params;
                }
            }

            public string Signature;
            public uint SectionSize;
            private byte[] Data;
            public uint SequenceOffset;
            public MidiEventCollection Midi;
            public List<TreeNode> SequenceTreeNodes { get; } = new List<TreeNode>();
            public List<string> TrackNames { get; } = new List<string>();
            private int loopstart = -1;
            private int loopend = -1;
            private int nrloop = -1;
        }

        public class SSEQIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Sound;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Sequence (SSEQ)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Sequence (*.sseq)|*.sseq";
            }

            public override Bitmap GetIcon()
            {
                return Resource.note;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'S' && File.Data[2] == 'E' && File.Data[3] == 'Q') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}