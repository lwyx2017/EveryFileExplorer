using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Collections;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace _3DS
{
    public class DVLB : FileFormat<DVLB.DVLBIdentifier>//, IViewable
    {
        public DVLB(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new DVLBHeader(er);
                Dvlp = new DVLP(er);
                Dvle = new DVLE[Header.NrDVLE];
                for (int i = 0; i < Header.NrDVLE; i++)
                {
                    er.BaseStream.Position = Header.DVLEOffsets[i];
                    Dvle[i] = new DVLE(er);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
        }

        public DVLBHeader Header;
        public class DVLBHeader
        {
            public DVLBHeader(EndianBinaryReader er)
            {
                uint position = (uint)er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DVLB") throw new SignatureNotCorrectException(Signature, "DVLB", er.BaseStream.Position - 4);
                NrDVLE = er.ReadUInt32();
                DVLEOffsets = new uint[NrDVLE];
                for (int i = 0; i < NrDVLE; i++)
                {
                    DVLEOffsets[i] = position + er.ReadUInt32();
                }
            }
            public string Signature;
            public uint NrDVLE;
            public uint[] DVLEOffsets;
        }

        public DVLP Dvlp;
        public class DVLP
        {
            public DVLP(EndianBinaryReader er)
            {
                uint position = (uint)er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DVLP") throw new SignatureNotCorrectException(Signature, "DVLP", er.BaseStream.Position - 4);
                Flags = er.ReadUInt32();
                ProgramOffset = position + er.ReadUInt32();
                ProgramLength = er.ReadUInt32();
                ExtensionTableOffset = position + er.ReadUInt32();
                NrExtensionTableEntries = er.ReadUInt32();
                FileNameTableOffset = position + er.ReadUInt32();
                FileNameTableLength = er.ReadUInt32();
                position = (uint)er.BaseStream.Position;
                er.BaseStream.Position = ProgramOffset;
                Program = er.ReadBytes((int)(ProgramLength * 4));
                er.BaseStream.Position = ExtensionTableOffset;
                ExtensionTableEntries = new ExtensionTableEntry[NrExtensionTableEntries];
                for (int i = 0; i < NrExtensionTableEntries; i++)
                {
                    ExtensionTableEntries[i] = new ExtensionTableEntry(er);
                }
                er.BaseStream.Position = FileNameTableOffset;
                FileNameTable = new Dictionary<int, string>();
                int fileNameTableByteOffset = 0;
                while (er.BaseStream.Position < FileNameTableOffset + FileNameTableLength)
                {
                    string text = er.ReadStringNT(Encoding.ASCII);
                    FileNameTable.Add(fileNameTableByteOffset, text);
                    fileNameTableByteOffset += text.Length + 1;
                }
                while (er.BaseStream.Position % 4 != 0)
                {
                    er.ReadByte();
                }
            }
            public class ExtensionTableEntry
            {
                public enum Component
                {
                    X,
                    Y,
                    Z,
                    W
                }

                [Flags]
                public enum ComponentMask
                {
                    X = 8,
                    Y = 4,
                    Z = 2,
                    W = 1
                }

                public uint Descriptor;
                public uint Unknown1;

                public ExtensionTableEntry(EndianBinaryReader er)
                {
                    Descriptor = er.ReadUInt32();
                    Unknown1 = er.ReadUInt32();
                }

                public string GetOutputMask()
                {
                    string text = "";
                    text = (((Descriptor & 8) == 0) ? (text + "_") : (text + "x"));
                    text = (((Descriptor & 4) == 0) ? (text + "_") : (text + "y"));
                    text = (((Descriptor & 2) == 0) ? (text + "_") : (text + "z"));
                    if ((Descriptor & 1) != 0)
                    {
                        return text + "w";
                    }
                    return text + "_";
                }

                public string GetSource1Mask()
                {
                    string text = "";
                    text += GetComponentString((Component)((Descriptor >> 11) & 3));
                    text += GetComponentString((Component)((Descriptor >> 9) & 3));
                    text += GetComponentString((Component)((Descriptor >> 7) & 3));
                    return text + GetComponentString((Component)((Descriptor >> 5) & 3));
                }

                public string GetSource2Mask()
                {
                    string text = "";
                    text += GetComponentString((Component)((Descriptor >> 20) & 3));
                    text += GetComponentString((Component)((Descriptor >> 18) & 3));
                    text += GetComponentString((Component)((Descriptor >> 16) & 3));
                    return text + GetComponentString((Component)((Descriptor >> 14) & 3));
                }

                private string GetComponentString(Component c)
                {
                    switch (c)
                    {
                        case Component.X:
                            return "x";
                        case Component.Y:
                            return "y";
                        case Component.Z:
                            return "z";
                        case Component.W:
                            return "w";
                        default:
                            return "";
                    }
                }
            }
            public string Signature;
            public uint Flags;
            public uint ProgramOffset;
            public uint ProgramLength;
            public uint ExtensionTableOffset;
            public uint NrExtensionTableEntries;
            public uint FileNameTableOffset;
            public uint FileNameTableLength;
            public byte[] Program;
            public ExtensionTableEntry[] ExtensionTableEntries;
            public Dictionary<int, string> FileNameTable;
        }

        public DVLE[] Dvle;
        public class DVLE
        {
            public DVLE(EndianBinaryReader er)
            {
                uint position = (uint)er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DVLE") throw new SignatureNotCorrectException(Signature, "DVLE", er.BaseStream.Position - 4);
                Flags = er.ReadUInt32();
                ProgramMainOffset = er.ReadUInt32();
                ProgramMainEndOffset = er.ReadUInt32();
                Unknown1 = er.ReadUInt16();
                Unknown2 = er.ReadUInt16();
                Unknown3 = er.ReadUInt32();
                UniformTableOffset = position + er.ReadUInt32();
                NrUniforms = er.ReadUInt32();
                LabelTableOffset = position + er.ReadUInt32();
                NrLabels = er.ReadUInt32();
                Table2Offset = position + er.ReadUInt32();
                NrTable2Entries = er.ReadUInt32();
                VariableTableOffset = position + er.ReadUInt32();
                NrVariables = er.ReadUInt32();
                SymbolTableOffset = position + er.ReadUInt32();
                SymbolTableLength = er.ReadUInt32();
                er.BaseStream.Position = UniformTableOffset;
                Uniforms = new Uniform[NrUniforms];
                for (int i = 0; i < NrUniforms; i++)
                {
                    Uniforms[i] = new Uniform(er);
                }
                er.BaseStream.Position = LabelTableOffset;
                Labels = new Label[NrLabels];
                for (int i = 0; i < NrLabels; i++)
                {
                    Labels[i] = new Label(er);
                }
                er.BaseStream.Position = Table2Offset;
                Outputs = new Output[NrTable2Entries];
                for (int i = 0; i < NrTable2Entries; i++)
                {
                    Outputs[i] = new Output(er);
                }
                er.BaseStream.Position = VariableTableOffset;
                Variables = new Variable[NrVariables];
                for (int i = 0; i < NrVariables; i++)
                {
                    Variables[i] = new Variable(er);
                }
                er.BaseStream.Position = SymbolTableOffset;
                SymbolTable = new Dictionary<int, string>();
                int symbolTableByteOffset = 0;
                while (er.BaseStream.Position < SymbolTableOffset + SymbolTableLength)
                {
                    string text = er.ReadStringNT(Encoding.ASCII);
                    SymbolTable.Add(symbolTableByteOffset, text);
                    symbolTableByteOffset += text.Length + 1;
                }
                while (er.BaseStream.Position % 4 != 0)
                {
                    er.ReadByte();
                }
            }
            public class Uniform
            {
                public Uniform(EndianBinaryReader er)
                {
                    Unknown1 = er.ReadUInt16();
                    UniformID = er.ReadUInt16();
                    Value = new Vector4(er.ReadFloat24(), er.ReadFloat24(), er.ReadFloat24(), er.ReadFloat24());
                }
                public ushort Unknown1;
                public ushort UniformID;
                public Vector4 Value;
            }

            public class Label
            {
                public Label(EndianBinaryReader er)
                {
                    LabelID = er.ReadUInt16();
                    Unknown1 = er.ReadUInt16();
                    LabelProgramOffset = er.ReadUInt32();
                    Unknown2 = er.ReadUInt32();
                    SymbolOffset = er.ReadUInt32();
                }
                public ushort LabelID;
                public ushort Unknown1;
                public uint LabelProgramOffset;
                public uint Unknown2;
                public uint SymbolOffset;
            }

            public class Output
            {
                public Output(EndianBinaryReader er)
                {
                    Unknown1 = er.ReadUInt16();
                    Unknown2 = er.ReadUInt16();
                    OutputType = er.ReadUInt16();
                    RegisterID = er.ReadUInt16();
                }
                public ushort Unknown1;
                public ushort Unknown2;
                public ushort OutputType;
                public ushort RegisterID;
            }

            public class Variable
            {
                public Variable(EndianBinaryReader er)
                {
                    SymbolOffset = er.ReadUInt32();
                    StartRegister = er.ReadUInt16();
                    EndRegister = er.ReadUInt16();
                }
                public uint SymbolOffset;
                public ushort StartRegister;
                public ushort EndRegister;
            }
            public string Signature;
            public uint Flags;
            public uint ProgramMainOffset;
            public uint ProgramMainEndOffset;
            public ushort Unknown1;
            public ushort Unknown2;
            public uint Unknown3;
            public uint UniformTableOffset;
            public uint NrUniforms;
            public uint LabelTableOffset;
            public uint NrLabels;
            public uint Table2Offset;
            public uint NrTable2Entries;
            public uint VariableTableOffset;
            public uint NrVariables;
            public uint SymbolTableOffset;
            public uint SymbolTableLength;
            public Uniform[] Uniforms;
            public Label[] Labels;
            public Output[] Outputs;
            public Variable[] Variables;
            public Dictionary<int, string> SymbolTable;
        }

        private string GetInputName(DVLE d, uint r)
        {
            if (r >> 4 == 0)
            {
                for (int i = 0; i < d.NrVariables; i++)
                {
                    if (d.Variables[i].StartRegister == r)
                    {
                        return d.SymbolTable[(int)d.Variables[i].SymbolOffset];
                    }
                }
            }
            else if (r >> 4 == 2)
            {
                r = (r & 0xF) | 0x10;
                for (int i = 0; i < d.NrVariables; i++)
                {
                    if (d.Variables[i].StartRegister <= r && d.Variables[i].EndRegister >= r)
                    {
                        return d.SymbolTable[(int)d.Variables[i].SymbolOffset] + "[" + (r - d.Variables[i].StartRegister) + "]";
                    }
                }
            }
            return $"{r:X}";
        }

        private string GetOutputName(DVLE d, uint r)
        {
            if (r >> 4 == 0)
            {
                switch (r)
                {
                    case 0:
                        return "gl_Position";
                    case 2:
                        return "gl_Color";
                    case 4:
                        return "gl_TexCoord[0]";
                    case 6:
                        return "gl_TexCoord[1]";
                    case 8:
                        return "gl_Texcoord[2]";
                }
            }
            return $"{r:X}";
        }

        private void Dump(DVLE d, uint Offset, uint NrInstructions)
        {
            int programBytePosition = (int)(Offset * 4);
            DVLE.Label[] labels;
            for (int i = 0; i < NrInstructions; i++)
            {
                labels = d.Labels;
                foreach (DVLE.Label label in labels)
                {
                    if (label.LabelProgramOffset == programBytePosition / 4)
                    {
                        Console.WriteLine(d.SymbolTable[(int)label.SymbolOffset] + ":");
                    }
                }
                uint instructionRawData = IOUtil.ReadU32LE(Dvlp.Program, programBytePosition);
                programBytePosition += 4;
                uint opcode = instructionRawData >> 26;
                switch (opcode)
                {
                    case 1:
                        {
                            DVLP.ExtensionTableEntry extensionTableEntry = Dvlp.ExtensionTableEntries[instructionRawData & 0x3F];
                            Console.WriteLine("    DP3 - {0}.{1} dot {2}.{3} -> {4}.{5}", GetInputName(d, (instructionRawData >> 12) & 0x3F), extensionTableEntry.GetSource1Mask(), GetInputName(d, (instructionRawData >> 6) & 0x3F), extensionTableEntry.GetSource2Mask(), GetOutputName(d, (instructionRawData >> 20) & 0x3F), extensionTableEntry.GetOutputMask());
                            break;
                        }
                    case 2:
                        {
                            DVLP.ExtensionTableEntry extensionTableEntry = Dvlp.ExtensionTableEntries[instructionRawData & 0x3F];
                            Console.WriteLine("    DP4 - {0}.{1} dot {2}.{3} -> {4}.{5}", GetInputName(d, (instructionRawData >> 12) & 0x3F), extensionTableEntry.GetSource1Mask(), GetInputName(d, (instructionRawData >> 6) & 0x3F), extensionTableEntry.GetSource2Mask(), GetOutputName(d, (instructionRawData >> 20) & 0x3F), extensionTableEntry.GetOutputMask());
                            break;
                        }
                    case 8:
                        {
                            DVLP.ExtensionTableEntry extensionTableEntry = Dvlp.ExtensionTableEntries[instructionRawData & 0x3F];
                            Console.WriteLine("    MUL - {0}.{1} * {2}.{3} -> {4}.{5}", GetInputName(d, (instructionRawData >> 12) & 0x3F), extensionTableEntry.GetSource1Mask(), GetInputName(d, (instructionRawData >> 6) & 0x3F), extensionTableEntry.GetSource2Mask(), GetOutputName(d, (instructionRawData >> 20) & 0x3F), extensionTableEntry.GetOutputMask());
                            break;
                        }
                    case 11:
                        {
                            DVLP.ExtensionTableEntry extensionTableEntry = Dvlp.ExtensionTableEntries[instructionRawData & 0x3F];
                            Console.WriteLine("    UNKB - {0}.{1}, {2}.{3} -> {4}.{5}", GetInputName(d, (instructionRawData >> 12) & 0x3F), extensionTableEntry.GetSource1Mask(), GetInputName(d, (instructionRawData >> 6) & 0x3F), extensionTableEntry.GetSource2Mask(), GetOutputName(d, (instructionRawData >> 20) & 0x3F), extensionTableEntry.GetOutputMask());
                            break;
                        }
                    case 19:
                        {
                            DVLP.ExtensionTableEntry extensionTableEntry = Dvlp.ExtensionTableEntries[instructionRawData & 0x3F];
                            Console.WriteLine("    MOV - {0}.{1} -> {2}.{3}", GetInputName(d, (instructionRawData >> 12) & 0x3F), extensionTableEntry.GetSource1Mask(), GetOutputName(d, (instructionRawData >> 20) & 0x3F), extensionTableEntry.GetOutputMask());
                            break;
                        }
                    case 33:
                        Console.WriteLine("    END21");
                        break;
                    case 34:
                        Console.WriteLine("    END22");
                        break;
                    case 36:
                        {
                            uint calParamA = instructionRawData & 0x3FF;
                            uint calTargetLabelOffset = (instructionRawData >> 10) & 0x3FF;
                            string text = "";
                            labels = d.Labels;
                            foreach (DVLE.Label label in labels)
                            {
                                if (label.LabelProgramOffset == calTargetLabelOffset)
                                {
                                    text = d.SymbolTable[(int)label.SymbolOffset];
                                }
                            }
                            Console.WriteLine("    CAL - " + text);
                            break;
                        }
                    default:
                        if (opcode < 32)
                        {
                            DVLP.ExtensionTableEntry extensionTableEntry = Dvlp.ExtensionTableEntries[instructionRawData & 0x3F];
                            Console.WriteLine("    {0:X} - {1}.{2}, {3:X}.{4} -> {5}.{6}", opcode, GetInputName(d, (instructionRawData >> 12) & 0x3F), extensionTableEntry.GetSource1Mask(), GetInputName(d, (instructionRawData >> 6) & 0x3F), extensionTableEntry.GetSource2Mask(), GetOutputName(d, (instructionRawData >> 20) & 0x3F), extensionTableEntry.GetOutputMask());
                        }
                        else
                        {
                            Console.WriteLine("    {0:X}", opcode);
                        }
                        break;
                }
            }
            labels = d.Labels;
            foreach (DVLE.Label label in labels)
            {
                if (label.LabelProgramOffset == programBytePosition / 4)
                {
                    Console.WriteLine(d.SymbolTable[(int)label.SymbolOffset] + ":");
                }
            }
        }

        public void DumpProgram(int Index)
        {
            Dump(Dvle[Index], 0, Dvlp.ProgramLength);
        }

        public class DVLBIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Shaders;
            }

            public override string GetFileDescription()
            {
                return "DMP Vertex Linker Binary (DVLB)";
            }

            public override string GetFileFilter()
            {
                return "DMP Vertex Linker Binary (*.shbin)|*.shbin";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'D' && File.Data[1] == 'V' && File.Data[2] == 'L' && File.Data[3] == 'B') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}