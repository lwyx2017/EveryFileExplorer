using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using OpenTK;

namespace NDS.NitroSystem.G3D
{
    public class NSBCA : FileFormat<NSBCA.NSBCAIdentifier>//, IViewable
    {
        public NSBCA(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new NSBCAHeader(er);
                JointAnimationSet = new JointAnmSet(er);
            }
            finally
            {
                er.ClearMarkers();
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
        }

        public NSBCAHeader Header;
        public class NSBCAHeader
        {
            public NSBCAHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "BCA0") throw new SignatureNotCorrectException(Signature, "BCA0", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                Constant = er.ReadUInt16();
                Filesize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                Section = er.ReadUInt16();
                DataBlockOffsets = new UInt32[Section];
                for (int i = 0; i < Section; i++)
                {
                    DataBlockOffsets[i] = er.ReadUInt32();
                }
            }
            public string Signature;
            public UInt16 Endianness;
            public UInt16 Constant;
            public UInt32 Filesize;
            public UInt16 HeaderSize;
            public UInt16 Section;
            public UInt32[] DataBlockOffsets;
        }

        public JointAnmSet JointAnimationSet;
        public class JointAnmSet
        {
            public class JointAnmSetData : DictionaryData
            {
                public uint Offset;

                public override void Read(EndianBinaryReader er)
                {
                    Offset = er.ReadUInt32();
                }
            }
            public JointAnmSet(EndianBinaryReader er)
            {
                er.SetMarkerOnCurrentOffset("JointAnmSet");
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "JNT0") throw new SignatureNotCorrectException(Signature, "JNT0", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                dict = new Dictionary<JointAnmSetData>(er);
                jntAnm = new JointAnm[dict.numEntry];
                long position = er.BaseStream.Position;
                for (int i = 0; i < dict.numEntry; i++)
                {
                    er.BaseStream.Position = er.GetMarker("JointAnmSet") + dict[i].Value.Offset;
                    jntAnm[i] = new JointAnm(er);
                }
                er.BaseStream.Position = position;
            }
            public string Signature;
            public uint SectionSize;
            public Dictionary<JointAnmSetData> dict;
            public JointAnm[] jntAnm;
        }

        public class JointAnm
        {
            public JointAnm(EndianBinaryReader er)
            {
                er.SetMarkerOnCurrentOffset("JointAnm");
                anmHeader = new AnmHeader(er, AnmHeader.Category0.J, AnmHeader.Category1.AC);
                numFrame = er.ReadUInt16();
                numNode = er.ReadUInt16();
                annFlag = er.ReadUInt32();
                ofsRot3 = er.ReadUInt32();
                er.SetMarker("ofsRot3", ofsRot3 + er.GetMarker("JointAnm"));
                ofsRot5 = er.ReadUInt32();
                er.SetMarker("ofsRot5", ofsRot5 + er.GetMarker("JointAnm"));
                ofsTag = er.ReadUInt16s(numNode);
                er.ReadBytes(4);
                tagData = new TagData[numNode];
                long position = er.BaseStream.Position;
                for (int i = 0; i < numNode; i++)
                {
                    er.BaseStream.Position = ofsTag[i] + er.GetMarker("JointAnm");
                    tagData[i] = new TagData(er, numFrame);
                }
                er.BaseStream.Position = position;
                er.RemoveMarker("JointAnm");
                er.RemoveMarker("ofsRot3");
                er.RemoveMarker("ofsRot5");
            }
            public AnmHeader anmHeader;
            public ushort numFrame;
            public ushort numNode;
            public uint annFlag;
            public uint ofsRot3;
            public uint ofsRot5;
            public ushort[] ofsTag;
            public TagData[] tagData;
        }

        public class TagData
        {
            public TagData(EndianBinaryReader er, int nrFrames)
            {
                Flag = er.ReadUInt32();
                if ((Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY) == 0)
                {
                    if ((Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_T) == 0 && (Flag & NNS_G3D_JNTANM_SRTINFO_BASE_T) == 0)
                    {
                        Tx = new JointAnmTrans(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_TX) != 0, nrFrames);
                        Ty = new JointAnmTrans(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_TY) != 0, nrFrames);
                        Tz = new JointAnmTrans(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_TZ) != 0, nrFrames);
                    }
                    if ((Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_R) == 0 && (Flag & NNS_G3D_JNTANM_SRTINFO_BASE_R) == 0)
                    {
                        R = new JointAnmRot(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_R) != 0, nrFrames);
                    }
                    if ((Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_S) == 0 && (Flag & NNS_G3D_JNTANM_SRTINFO_BASE_S) == 0)
                    {
                        Sx = new JointAnmScale(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_SX) != 0, nrFrames);
                        Sy = new JointAnmScale(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_SY) != 0, nrFrames);
                        Sz = new JointAnmScale(er, (Flag & NNS_G3D_JNTANM_SRTINFO_CONST_SZ) != 0, nrFrames);
                    }
                }
            }
            public float[] GetMatrix(int frame, bool mayaScale, int posScale, MDL0.Model.NodeSet.NodeData mdlNode)
            {
                if ((Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY) != 0)
                {
                    return LoadIdentity();
                }
                float translateX = Tx != null ? Tx.GetValue(frame) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_T) == 0 ? mdlNode.Tx : 0;
                float translateY = Ty != null ? Ty.GetValue(frame) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_T) == 0 ? mdlNode.Ty : 0;
                float translateZ = Tz != null ? Tz.GetValue(frame) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_T) == 0 ? mdlNode.Tz : 0;
                float[] translationMatrix = Translate(LoadIdentity(), translateX / posScale, translateY / posScale, translateZ / posScale);
                float[] rotationMatrix = R != null ? MultMatrix(LoadIdentity(), R.GetMatrix(frame)) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_R) == 0 ? mdlNode.GetRotation() : LoadIdentity();
                float scaleX = Sx != null ? Sx.GetValue(frame) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_S) == 0 ? mdlNode.Sx : 1;
                float scaleY = Sy != null ? Sy.GetValue(frame) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_S) == 0 ? mdlNode.Sy : 1;
                float scaleZ = Sz != null ? Sz.GetValue(frame) : (Flag & NNS_G3D_JNTANM_SRTINFO_IDENTITY_S) == 0 ? mdlNode.Sz : 1;
                float[] scaleMatrix = Scale(LoadIdentity(), scaleX, scaleY, scaleZ);
                float[] resultMatrix = LoadIdentity();
                resultMatrix = MultMatrix(resultMatrix, translationMatrix);
                resultMatrix = MultMatrix(resultMatrix, rotationMatrix);
                resultMatrix = MultMatrix(resultMatrix, scaleMatrix);
                return resultMatrix;
            }

            private float[] Translate(float[] matrix, float x, float y, float z)
            {
                float[] translateMat = LoadIdentity();
                translateMat[12] = x;
                translateMat[13] = y;
                translateMat[14] = z;
                return MultMatrix(matrix, translateMat);
            }

            private float[] LoadIdentity()
            {
                return new float[16]
                {
                   1, 0, 0, 0,
                   0, 1, 0, 0,
                   0, 0, 1, 0,
                   0, 0, 0, 1
                };
            }

            private float[] MultMatrix(float[] a, float[] b)
            {
                float[] result = new float[16];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        result[(i << 2) + j] = 0;
                        for (int k = 0; k < 4; k++)
                        {
                            result[(i << 2) + j] += a[(k << 2) + j] * b[(i << 2) + k];
                        }
                    }
                }
                return result;
            }

            private float[] Scale(float[] matrix, float x, float y, float z)
            {
                float[] scaleMat = LoadIdentity();
                scaleMat[0] = x;
                scaleMat[5] = y;
                scaleMat[10] = z;
                return MultMatrix(matrix, scaleMat);
            }
            public uint Flag;
            public JointAnmTrans Tx;
            public JointAnmTrans Ty;
            public JointAnmTrans Tz;
            public JointAnmRot R;
            public JointAnmScale Sx;
            public JointAnmScale Sy;
            public JointAnmScale Sz;
            private const uint NNS_G3D_JNTANM_SRTINFO_IDENTITY = 1;
            private const uint NNS_G3D_JNTANM_SRTINFO_IDENTITY_T = 2;
            private const uint NNS_G3D_JNTANM_SRTINFO_BASE_T = 4;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_TX = 8;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_TY = 16;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_TZ = 32;
            private const uint NNS_G3D_JNTANM_SRTINFO_IDENTITY_R = 64;
            private const uint NNS_G3D_JNTANM_SRTINFO_BASE_R = 128;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_R = 256;
            private const uint NNS_G3D_JNTANM_SRTINFO_IDENTITY_S = 512;
            private const uint NNS_G3D_JNTANM_SRTINFO_BASE_S = 1024;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_SX = 2048;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_SY = 4096;
            private const uint NNS_G3D_JNTANM_SRTINFO_CONST_SZ = 8192;
            private const uint NNS_G3D_JNTANM_SRTINFO_NODE_MASK = 4278190080;
        }
        public class JointAnmTrans
        {
            public JointAnmTrans(EndianBinaryReader er, bool isConst, int nrFrames)
            {
                if (isConst)
                {
                    ConstTrans = er.ReadFx32();
                    return;
                }

                Info = er.ReadUInt32();
                Offset = er.ReadUInt32();

                int interpolationStep = (int)((Info & 0xC0000000u) >> 29);
                if (interpolationStep == 0)
                {
                    interpolationStep++;
                }

                long position = er.BaseStream.Position;
                er.BaseStream.Position = Offset + er.GetMarker("JointAnm");

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_TINFO_LAST_INTERP_MASK) >> 16);
                int frameCount = lastInterpolationFrame / interpolationStep + interpolationStep;

                if ((Info & NNS_G3D_JNTANM_TINFO_FX16ARRAY) != 0)
                {
                    Translation = new float[frameCount];
                    for (int i = 0; i < frameCount; i++)
                    {
                        Translation[i] = er.ReadFx16();
                    }
                }
                else
                {
                    Translation = er.ReadFx32s(frameCount);
                }

                er.BaseStream.Position = position;
            }

            public float GetValue(int frame)
            {
                if (Translation == null)
                {
                    return ConstTrans;
                }

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_TINFO_LAST_INTERP_MASK) >> 16);

                if ((Info & NNS_G3D_JNTANM_TINFO_STEP_MASK) == 0)
                {
                    return Translation[frame];
                }

                if ((Info & NNS_G3D_JNTANM_TINFO_STEP_2) != 0)
                {
                    if ((frame & 1) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            return Translation[(lastInterpolationFrame >> 1) + 1];
                        }
                        return Translation[frame >> 1] / 2 + Translation[(frame >> 1) + 1] / 2;
                    }
                    return Translation[frame >> 1];
                }

                if ((Info & NNS_G3D_JNTANM_TINFO_STEP_4) != 0)
                {
                    if ((frame & 3) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            return Translation[(lastInterpolationFrame >> 2) + (frame & 3)];
                        }

                        if ((frame & 1) != 0)
                        {
                            int currentIndex;
                            int nextIndex;

                            if ((frame & 2) != 0)
                            {
                                currentIndex = frame >> 2;
                                nextIndex = currentIndex + 1;
                            }
                            else
                            {
                                nextIndex = frame >> 2;
                                currentIndex = nextIndex + 1;
                            }

                            float currentValue = Translation[nextIndex];
                            float nextValue = Translation[currentIndex];
                            return (currentValue + currentValue + currentValue + nextValue) / 4;
                        }

                        return Translation[frame >> 2] / 2 + Translation[(frame >> 2) + 1] / 2;
                    }
                    return Translation[frame >> 2];
                }

                return 0;
            }
            public float ConstTrans;
            public uint Info;
            public uint Offset;
            public float[] Translation;
            private const uint NNS_G3D_JNTANM_TINFO_STEP_MASK = 3221225472;
            private const uint NNS_G3D_JNTANM_TINFO_STEP_2 = 1073741824;
            private const uint NNS_G3D_JNTANM_TINFO_STEP_4 = 2147483648;
            private const uint NNS_G3D_JNTANM_TINFO_FX16ARRAY = 536870912;
            private const uint NNS_G3D_JNTANM_TINFO_LAST_INTERP_MASK = 536805376;
        }

        public class JointAnmScale
        {
            public JointAnmScale(EndianBinaryReader er, bool isConst, int nrFrames)
            {
                if (isConst)
                {
                    ConstScale = er.ReadFx32();
                    ConstInvScale = er.ReadFx32();
                    return;
                }

                Info = er.ReadUInt32();
                Offset = er.ReadUInt32();

                int interpolationStep = (int)((Info & 0xC0000000u) >> 29);
                if (interpolationStep == 0)
                {
                    interpolationStep++;
                }

                long position = er.BaseStream.Position;
                er.BaseStream.Position = Offset + er.GetMarker("JointAnm");

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_SINFO_LAST_INTERP_MASK) >> 16);
                int frameCount = lastInterpolationFrame / interpolationStep + interpolationStep;

                if ((Info & NNS_G3D_JNTANM_SINFO_FX16ARRAY) != 0)
                {
                    Scale16 = new JointAnmScaleFx16[frameCount];
                    for (int i = 0; i < frameCount; i++)
                    {
                        Scale16[i] = new JointAnmScaleFx16(er);
                    }
                }
                else
                {
                    Scale32 = new JointAnmScaleFx32[frameCount];
                    for (int i = 0; i < frameCount; i++)
                    {
                        Scale32[i] = new JointAnmScaleFx32(er);
                    }
                }

                er.BaseStream.Position = position;
            }

            public float GetValue(int frame)
            {
                if (Scale16 == null && Scale32 == null)
                {
                    return ConstScale;
                }

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_SINFO_LAST_INTERP_MASK) >> 16);

                if ((Info & NNS_G3D_JNTANM_SINFO_STEP_MASK) == 0)
                {
                    if (Scale16 == null)
                    {
                        return Scale32[frame].Scale;
                    }
                    return Scale16[frame].Scale;
                }

                if ((Info & NNS_G3D_JNTANM_SINFO_STEP_2) != 0)
                {
                    if ((frame & 1) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            if (Scale16 == null)
                            {
                                return Scale32[(lastInterpolationFrame >> 1) + 1].Scale;
                            }
                            return Scale16[(lastInterpolationFrame >> 1) + 1].Scale;
                        }
                        if (Scale16 == null)
                        {
                            return (Scale32[frame >> 1].Scale + Scale32[(frame >> 1) + 1].Scale) / 2;
                        }
                        return (Scale16[frame >> 1].Scale + Scale16[(frame >> 1) + 1].Scale) / 2;
                    }
                    if (Scale16 == null)
                    {
                        return Scale32[frame >> 1].Scale;
                    }
                    return Scale16[frame >> 1].Scale;
                }

                if ((Info & NNS_G3D_JNTANM_SINFO_STEP_4) != 0)
                {
                    if ((frame & 3) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            if (Scale16 == null)
                            {
                                return Scale32[(lastInterpolationFrame >> 2) + (frame & 3)].Scale;
                            }
                            return Scale16[(lastInterpolationFrame >> 2) + (frame & 3)].Scale;
                        }

                        if ((frame & 1) != 0)
                        {
                            int currentIndex;
                            int nextIndex;

                            if ((frame & 2) != 0)
                            {
                                currentIndex = frame >> 2;
                                nextIndex = currentIndex + 1;
                            }
                            else
                            {
                                nextIndex = frame >> 2;
                                currentIndex = nextIndex + 1;
                            }

                            float currentValue;
                            float nextValue;
                            if (Scale16 != null)
                            {
                                currentValue = Scale16[nextIndex].Scale;
                                nextValue = Scale16[currentIndex].Scale;
                                return (currentValue + currentValue + currentValue + nextValue) / 4;
                            }
                            currentValue = Scale32[nextIndex].Scale;
                            nextValue = Scale32[currentIndex].Scale;
                            return (currentValue + currentValue + currentValue + nextValue) / 2;
                        }

                        if (Scale16 == null)
                        {
                            return Scale32[frame >> 2].Scale / 2 + Scale32[(frame >> 2) + 1].Scale / 2;
                        }
                        return (Scale16[frame >> 2].Scale + Scale16[(frame >> 2) + 1].Scale) / 2;
                    }
                    if (Scale16 == null)
                    {
                        return Scale32[frame >> 2].Scale;
                    }
                    return Scale16[frame >> 2].Scale;
                }

                return 1;
            }

            public float GetInvValue(int frame)
            {
                if (Scale16 == null && Scale32 == null)
                {
                    return ConstInvScale;
                }

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_SINFO_LAST_INTERP_MASK) >> 16);

                if ((Info & NNS_G3D_JNTANM_SINFO_STEP_MASK) == 0)
                {
                    if (Scale16 == null)
                    {
                        return Scale32[frame].InvScale;
                    }
                    return Scale16[frame].InvScale;
                }

                if ((Info & NNS_G3D_JNTANM_SINFO_STEP_2) != 0)
                {
                    if ((frame & 1) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            if (Scale16 == null)
                            {
                                return Scale32[lastInterpolationFrame >> 2].InvScale;
                            }
                            return Scale16[lastInterpolationFrame >> 2].InvScale;
                        }
                        if (Scale16 == null)
                        {
                            return Scale32[frame >> 1].InvScale / 2 + Scale32[(frame >> 1) + 1].InvScale / 2;
                        }
                        return (Scale16[frame >> 1].InvScale + Scale16[(frame >> 1) + 1].InvScale) / 2;
                    }
                    if (Scale16 == null)
                    {
                        return Scale32[frame >> 1].InvScale;
                    }
                    return Scale16[frame >> 1].InvScale;
                }

                if ((Info & NNS_G3D_JNTANM_SINFO_STEP_4) != 0)
                {
                    if ((frame & 3) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            if (Scale16 == null)
                            {
                                return Scale32[(lastInterpolationFrame >> 2) + (frame & 3)].InvScale;
                            }
                            return Scale16[(lastInterpolationFrame >> 2) + (frame & 3)].InvScale;
                        }

                        if ((frame & 1) != 0)
                        {
                            int currentIndex;
                            int nextIndex;

                            if ((frame & 2) != 0)
                            {
                                currentIndex = frame >> 2;
                                nextIndex = currentIndex + 1;
                            }
                            else
                            {
                                nextIndex = frame >> 2;
                                currentIndex = nextIndex + 1;
                            }

                            float currentValue;
                            float nextValue;
                            if (Scale16 != null)
                            {
                                currentValue = Scale16[nextIndex].InvScale;
                                nextValue = Scale16[currentIndex].InvScale;
                                return (currentValue + currentValue + currentValue + nextValue) / 4;
                            }
                            currentValue = Scale32[nextIndex].InvScale;
                            nextValue = Scale32[currentIndex].InvScale;
                            return (currentValue + currentValue + currentValue + nextValue) / 2;
                        }

                        if (Scale16 == null)
                        {
                            return Scale32[frame >> 2].InvScale / 2 + Scale32[(frame >> 2) + 1].InvScale / 2;
                        }
                        return (Scale16[frame >> 2].InvScale + Scale16[(frame >> 2) + 1].InvScale) / 2;
                    }
                    if (Scale16 == null)
                    {
                        return Scale32[frame >> 2].InvScale;
                    }
                    return Scale16[frame >> 2].InvScale;
                }

                return 1;
            }
            private const uint NNS_G3D_JNTANM_SINFO_STEP_MASK = 3221225472;
            private const uint NNS_G3D_JNTANM_SINFO_STEP_2 = 1073741824;
            private const uint NNS_G3D_JNTANM_SINFO_STEP_4 = 2147483648;
            private const uint NNS_G3D_JNTANM_SINFO_FX16ARRAY = 536870912;
            private const uint NNS_G3D_JNTANM_SINFO_LAST_INTERP_MASK = 536805376;
            public float ConstScale;
            public float ConstInvScale;
            public uint Info;
            public uint Offset;
            public JointAnmScaleFx16[] Scale16;
            public JointAnmScaleFx32[] Scale32;
        }

        public class JointAnmScaleFx16
        {
            public float Scale;
            public float InvScale;

            public JointAnmScaleFx16(EndianBinaryReader er)
            {
                Scale = er.ReadFx16();
                InvScale = er.ReadFx16();
            }
        }

        public class JointAnmScaleFx32
        {
            public float Scale;
            public float InvScale;

            public JointAnmScaleFx32(EndianBinaryReader er)
            {
                Scale = er.ReadFx32();
                InvScale = er.ReadFx32();
            }
        }

        public class JointAnmRot
        {
            public JointAnmRot(EndianBinaryReader er, bool isConst, int nrFrames)
            {
                long position;
                if (isConst)
                {
                    ConstOffset = er.ReadUInt32();
                    position = er.BaseStream.Position;
                    if (ConstOffset >> 15 == 1)
                    {
                        er.BaseStream.Position = er.GetMarker("ofsRot3") + 6 * (ConstOffset & 0x7FFF);
                        ConstRot3 = new JointAnmRot3(er);
                    }
                    else
                    {
                        er.BaseStream.Position = er.GetMarker("ofsRot5") + 10 * (ConstOffset & 0x7FFF);
                        ConstRot5 = new JointAnmRot5(er);
                    }
                    er.BaseStream.Position = position;
                    return;
                }

                Info = er.ReadUInt32();
                Offset = er.ReadUInt32();

                int interpolationStep = (int)((Info & 0xC0000000u) >> 29);
                if (interpolationStep == 0)
                {
                    interpolationStep++;
                }

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_RINFO_LAST_INTERP_MASK) >> 16);
                int frameCount = lastInterpolationFrame / interpolationStep + interpolationStep;

                position = er.BaseStream.Position;
                Rot3 = new JointAnmRot3[frameCount];
                Rot5 = new JointAnmRot5[frameCount];

                for (int i = 0; i < frameCount; i++)
                {
                    er.BaseStream.Position = Offset + er.GetMarker("JointAnm") + i * 2;
                    ushort index = er.ReadUInt16();
                    if (index >> 15 == 1)
                    {
                        er.BaseStream.Position = er.GetMarker("ofsRot3") + 6 * (index & 0x7FFF);
                        Rot3[i] = new JointAnmRot3(er);
                    }
                    else
                    {
                        er.BaseStream.Position = er.GetMarker("ofsRot5") + 10 * (index & 0x7FFF);
                        Rot5[i] = new JointAnmRot5(er);
                    }
                }

                er.BaseStream.Position = position;
            }

            public MTX44 GetMatrix(int frame)
            {
                if (Rot5 == null && Rot3 == null)
                {
                    return (ConstRot3 == null) ? ConstRot5.GetMatrix() : ConstRot3.GetMatrix();
                }

                int lastInterpolationFrame = (int)((Info & NNS_G3D_JNTANM_RINFO_LAST_INTERP_MASK) >> 16);

                if ((Info & NNS_G3D_JNTANM_RINFO_STEP_MASK) == 0)
                {
                    return (Rot3[frame] == null) ? Rot5[frame].GetMatrix() : Rot3[frame].GetMatrix();
                }

                if ((Info & NNS_G3D_JNTANM_RINFO_STEP_2) != 0)
                {
                    if ((frame & 1) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            return (Rot3[(lastInterpolationFrame >> 1) + 1] == null) ? Rot5[(lastInterpolationFrame >> 1) + 1].GetMatrix() : Rot3[(lastInterpolationFrame >> 1) + 1].GetMatrix();
                        }
                        return InterpolateMTX44_50_50((Rot3[frame >> 1] == null) ? Rot5[frame >> 1].GetMatrix() : Rot3[frame >> 1].GetMatrix(), (Rot3[(frame >> 1) + 1] == null) ? Rot5[(frame >> 1) + 1].GetMatrix() : Rot3[(frame >> 1) + 1].GetMatrix());
                    }
                    return (Rot3[frame >> 1] == null) ? Rot5[frame >> 1].GetMatrix() : Rot3[frame >> 1].GetMatrix();
                }

                if ((Info & NNS_G3D_JNTANM_RINFO_STEP_4) != 0)
                {
                    if ((frame & 3) != 0)
                    {
                        if (frame > lastInterpolationFrame)
                        {
                            return (Rot3[(lastInterpolationFrame >> 2) + (frame & 3)] == null) ? Rot5[(lastInterpolationFrame >> 2) + (frame & 3)].GetMatrix() : Rot3[(lastInterpolationFrame >> 2) + (frame & 3)].GetMatrix();
                        }

                        if ((frame & 1) != 0)
                        {
                            int currentIndex;
                            int nextIndex;

                            if ((frame & 2) != 0)
                            {
                                currentIndex = frame >> 2;
                                nextIndex = currentIndex + 1;
                            }
                            else
                            {
                                nextIndex = frame >> 2;
                                currentIndex = nextIndex + 1;
                            }

                            MTX44 mtx = ((Rot3[nextIndex] == null) ? Rot5[nextIndex].GetMatrix() : Rot3[nextIndex].GetMatrix());
                            MTX44 mtx2 = ((Rot3[currentIndex] == null) ? Rot5[currentIndex].GetMatrix() : Rot3[currentIndex].GetMatrix());

                            mtx[0, 0] = mtx[0, 0] * 3 + mtx2[0, 0];
                            mtx[1, 0] = mtx[1, 0] * 3 + mtx2[1, 0];
                            mtx[2, 0] = mtx[2, 0] * 3 + mtx2[2, 0];
                            mtx[0, 1] = mtx[0, 1] * 3 + mtx2[0, 1];
                            mtx[1, 1] = mtx[1, 1] * 3 + mtx2[1, 1];
                            mtx[2, 1] = mtx[2, 1] * 3 + mtx2[2, 1];

                            Vector3 vector = Vector3.Normalize(new Vector3(mtx[0, 0], mtx[1, 0], mtx[2, 0]));
                            mtx[0, 0] = vector.X;
                            mtx[1, 0] = vector.Y;
                            mtx[2, 0] = vector.Z;

                            vector = Vector3.Normalize(new Vector3(mtx[0, 1], mtx[1, 1], mtx[2, 1]));
                            mtx[0, 1] = vector.X;
                            mtx[1, 1] = vector.Y;
                            mtx[2, 1] = vector.Z;

                            mtx[0, 2] = mtx[0, 2] * 3 + mtx2[0, 2];
                            mtx[1, 2] = mtx[1, 2] * 3 + mtx2[1, 2];
                            mtx[2, 2] = mtx[2, 2] * 3 + mtx2[2, 2];

                            vector = Vector3.Normalize(new Vector3(mtx[0, 2], mtx[1, 2], mtx[2, 2]));
                            mtx[0, 2] = vector.X;
                            mtx[1, 2] = vector.Y;
                            mtx[2, 2] = vector.Z;

                            return mtx;
                        }

                        return InterpolateMTX44_50_50((Rot3[frame >> 2] == null) ? Rot5[frame >> 2].GetMatrix() : Rot3[frame >> 2].GetMatrix(), (Rot3[(frame >> 2) + 1] == null) ? Rot5[(frame >> 2) + 1].GetMatrix() : Rot3[(frame >> 2) + 1].GetMatrix());
                    }
                    return (Rot3[frame >> 2] == null) ? Rot5[frame >> 2].GetMatrix() : Rot3[frame >> 2].GetMatrix();
                }

                return new MTX44();
            }

            private MTX44 InterpolateMTX44_50_50(MTX44 a, MTX44 b)
            {
                a[0, 0] += b[0, 0];
                a[1, 0] += b[1, 0];
                a[2, 0] += b[2, 0];
                a[0, 1] += b[0, 1];
                a[1, 1] += b[1, 1];
                a[2, 1] += b[2, 1];

                Vector3 vector = Vector3.Normalize(new Vector3(a[0, 0], a[1, 0], a[2, 0]));
                a[0, 0] = vector.X;
                a[1, 0] = vector.Y;
                a[2, 0] = vector.Z;

                vector = Vector3.Normalize(new Vector3(a[0, 1], a[1, 1], a[2, 1]));
                a[0, 1] = vector.X;
                a[1, 1] = vector.Y;
                a[2, 1] = vector.Z;

                a[0, 2] += b[0, 2];
                a[1, 2] += b[1, 2];
                a[2, 2] += b[2, 2];

                vector = Vector3.Normalize(new Vector3(a[0, 2], a[1, 2], a[2, 2]));
                a[0, 2] = vector.X;
                a[1, 2] = vector.Y;
                a[2, 2] = vector.Z;

                return a;
            }
            private const uint NNS_G3D_JNTANM_RINFO_STEP_MASK = 3221225472;
            private const uint NNS_G3D_JNTANM_RINFO_STEP_2 = 1073741824;
            private const uint NNS_G3D_JNTANM_RINFO_STEP_4 = 2147483648;
            private const uint NNS_G3D_JNTANM_RINFO_LAST_INTERP_MASK = 536805376;
            public uint ConstOffset;
            public JointAnmRot3 ConstRot3;
            public JointAnmRot5 ConstRot5;
            public uint Info;
            public uint Offset;
            public JointAnmRot3[] Rot3;
            public JointAnmRot5[] Rot5;
        }

        public class JointAnmRot3
        {
            public ushort Info;
            public float A;
            public float B;

            public JointAnmRot3(EndianBinaryReader er)
            {
                Info = er.ReadUInt16();
                A = er.ReadFx16();
                B = er.ReadFx16();
            }

            public MTX44 GetMatrix()
            {
                MTX44 mtx = new MTX44();
                return GlNitro.glNitroPivot(new float[2] { A, B }, Info & 0xF, (Info >> 4) & 0xF);
            }
        }

        public class JointAnmRot5
        {
            public short[] Data;

            public JointAnmRot5(EndianBinaryReader er)
            {
                Data = er.ReadInt16s(5);
            }

            public MTX44 GetMatrix()
            {
                short packedZ = (short)(Data[4] & 7);
                short m11 = (short)(Data[4] >> 3);
                packedZ = (short)((packedZ << 3) | (Data[0] & 7));
                short m00 = (short)(Data[0] >> 3);
                packedZ = (short)((packedZ << 3) | (Data[1] & 7));
                short m10 = (short)(Data[1] >> 3);
                packedZ = (short)((packedZ << 3) | (Data[2] & 7));
                short m20 = (short)(Data[2] >> 3);
                packedZ = (short)((packedZ << 3) | (Data[3] & 7));
                short m01 = (short)(Data[3] >> 3);
                packedZ = (short)((short)(packedZ << 3) >> 3);
                MTX44 mtx = new MTX44();
                mtx[0, 0] = m00;
                mtx[1, 0] = m10;
                mtx[2, 0] = m20;
                mtx[0, 1] = m01;
                mtx[1, 1] = m11;
                mtx[2, 1] = packedZ;
                Vector3 vector = VecCross(new Vector3(mtx[0, 0], mtx[1, 0], mtx[2, 0]), new Vector3(mtx[0, 1], mtx[1, 1], mtx[2, 1]));
                mtx[0, 2] = vector.X;
                mtx[1, 2] = vector.Y;
                mtx[2, 2] = vector.Z;
                mtx[0, 0] /= 4096;
                mtx[1, 0] /= 4096;
                mtx[2, 0] /= 4096;
                mtx[0, 1] /= 4096;
                mtx[1, 1] /= 4096;
                mtx[2, 1] /= 4096;
                mtx[0, 2] /= 4096;
                mtx[1, 2] /= 4096;
                mtx[2, 2] /= 4096;
                return mtx;
            }

            private Vector3 VecCross(Vector3 a, Vector3 b)
            {
                float x = (a.Y * b.Z - a.Z * b.Y) / 4096;
                float y = (a.Z * b.X - a.X * b.Z) / 4096;
                float z = (a.X * b.Y - a.Y * b.X) / 4096;
                return new Vector3(x, y, z);
            }
        }

        public class NSBCAIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Animations;
            }

            public override string GetFileDescription()
            {
                return "Nitro System Binary Character Animation (NSBCA)";
            }

            public override string GetFileFilter()
            {
                return "Nitro System Binary Character Animation (*.nsbca)|*.nsbca";
            }

            public override Bitmap GetIcon()
            {
                return Resource.lollypopanim;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'C' && File.Data[2] == 'A' && File.Data[3] == '0')
                    return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}