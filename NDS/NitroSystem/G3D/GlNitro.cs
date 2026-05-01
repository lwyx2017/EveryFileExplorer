using LibEveryFileExplorer.IO;
using NDS.GPU;
using NDS.NitroSystem.G3D;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace NDS.NitroSystem.G3D
{
    public class GlNitro
    {
        public class Nitro3DContext
        {
            public int MatrixMode = 2;
            public MTX44[] MatrixStack = new MTX44[31];
            public Vector3[] LightVectors = new Vector3[4]
            {
                new Vector3(0f, -1f, -1f),
                new Vector3(0.998047f, -1f, 0f),
                new Vector3(0f, -1f, 0.998047f),
                new Vector3(-1f, -1f, 0f)
            };
            public Color[] LightColors = new Color[4]
            {
                Color.White,
                Color.White,
                Color.White,
                Color.White
            };
            public bool[] LightEnabled = new bool[4];
            public byte[] SpecularReflectionTable = new byte[128]
            {
                0, 2, 4, 6, 8, 10, 12, 14, 16, 18,
                20, 22, 24, 26, 28, 30, 32, 34, 36, 38,
                40, 42, 44, 46, 48, 50, 52, 54, 56, 58,
                60, 62, 64, 66, 68, 70, 72, 74, 76, 78,
                80, 82, 84, 86, 88, 90, 92, 94, 96, 98,
                100, 102, 104, 106, 108, 110, 112, 114, 116, 118,
                120, 122, 124, 126, 129, 131, 133, 135, 137, 139,
                141, 143, 145, 147, 149, 151, 153, 155, 157, 159,
                161, 163, 165, 167, 169, 171, 173, 175, 177, 179,
                181, 183, 185, 187, 189, 191, 193, 195, 197, 199,
                201, 203, 205, 207, 209, 211, 213, 215, 217, 219,
                221, 223, 225, 227, 229, 231, 233, 235, 237, 239,
                241, 243, 245, 247, 249, 251, 253, 255
            };
            public bool UseSpecularReflectionTable;
            public Color DiffuseColor;
            public Color AmbientColor;
            public Color SpecularColor;
            public Color EmissionColor;
            public int Alpha = 31;

            public Nitro3DContext()
            {
                for (int i = 0; i < 31; i++)
                {
                    MatrixStack[i] = new MTX44();
                }
            }
        }

        public class DisplayListEncoder
        {
            private List<byte> DisplayList = new List<byte>();
            private byte[] Commands = new byte[4];
            private int CommandId = 0;
            private List<byte> Args = new List<byte>();
            private Vector3 VtxState = new Vector3(float.NaN, float.NaN, float.NaN);

            public void Nop()
            {
                AddCommand(0);
            }

            public void Color(Color c)
            {
                AddCommand(32, (uint)Textures.EncodeColor(c.ToArgb()));
            }

            public void Normal(Vector3 Normal)
            {
                AddCommand(33, (uint)((((int)(Normal.Z * 512f) & 0x3FF) << 20) | (((int)(Normal.Y * 512f) & 0x3FF) << 10) | ((int)(Normal.X * 512f) & 0x3FF)));
            }

            public void TexCoord(Vector2 TexCoord)
            {
                AddCommand(34, (uint)((((int)(TexCoord.Y * 16f) & 0xFFFF) << 16) | ((int)(TexCoord.X * 16f) & 0xFFFF)));
            }

            public void BestVertex(Vector3 Position)
            {
                if (VtxState.X == Position.X)
                {
                    VertexYZ(Position);
                }
                else if (VtxState.Y == Position.Y)
                {
                    VertexXZ(Position);
                }
                else if (VtxState.Z == Position.Z)
                {
                    VertexXY(Position);
                }
                else if (((int)(Math.Abs(Position.X) * 4096f) & 0x3F) == 0 && ((int)(Math.Abs(Position.Y) * 4096f) & 0x3F) == 0 && ((int)(Math.Abs(Position.Z) * 4096f) & 0x3F) == 0)
                {
                    Vertex10(Position);
                }
                else
                {
                    Vertex(Position);
                }
                VtxState = Position;
            }

            public void Vertex(Vector3 Position)
            {
                AddCommand(35, (uint)((((int)(Position.Y * 4096f) & 0xFFFF) << 16) | ((int)(Position.X * 4096f) & 0xFFFF)), (uint)((int)(Position.Z * 4096f) & 0xFFFF));
            }

            public void Vertex10(Vector3 Position)
            {
                AddCommand(36, (uint)((((int)(Position.Z * 64f) & 0x3FF) << 20) | (((int)(Position.Y * 64f) & 0x3FF) << 10) | ((int)(Position.X * 64f) & 0x3FF)));
            }

            public void VertexXY(Vector3 Position)
            {
                AddCommand(37, (uint)((((int)(Position.Y * 4096f) & 0xFFFF) << 16) | ((int)(Position.X * 4096f) & 0xFFFF)));
            }

            public void VertexXZ(Vector3 Position)
            {
                AddCommand(38, (uint)((((int)(Position.Z * 4096f) & 0xFFFF) << 16) | ((int)(Position.X * 4096f) & 0xFFFF)));
            }

            public void VertexYZ(Vector3 Position)
            {
                AddCommand(39, (uint)((((int)(Position.Z * 4096f) & 0xFFFF) << 16) | ((int)(Position.Y * 4096f) & 0xFFFF)));
            }

            public void VertexDiff(Vector3 Position)
            {
                AddCommand(40, (uint)((((int)(Position.Z * 4096f) & 0x3FF) << 20) | (((int)(Position.Y * 4096f) & 0x3FF) << 10) | ((int)(Position.X * 4096f) & 0x3FF)));
            }

            public void Begin(byte VertexType)
            {
                AddCommand(64, VertexType);
            }

            public void End()
            {
                if (CommandId == 0)
                {
                    AddCommand(65);
                    Flush();
                    Flush();
                }
                else
                {
                    AddCommand(65);
                }
            }

            public void Flush()
            {
                for (int i = CommandId; i < 4; i++)
                {
                    Nop();
                }
            }

            public void AddCommand(byte Id)
            {
                AddCommand(Id, new uint[0]);
            }

            public void AddCommand(byte Id, params uint[] Params)
            {
                Commands[CommandId] = Id;
                CommandId++;
                foreach (uint value in Params)
                {
                    Args.AddRange(BitConverter.GetBytes(value));
                }
                if (CommandId == 4)
                {
                    DisplayList.AddRange(Commands);
                    DisplayList.AddRange(Args);
                    Commands = new byte[4];
                    Args.Clear();
                    CommandId = 0;
                }
            }

            public byte[] GetDisplayList()
            {
                if (CommandId != 0)
                {
                    Flush();
                }
                Flush();
                return DisplayList.ToArray();
            }
        }

        private class DisplayListRecoder
        {
            private Dictionary<byte, int> NrParams = new Dictionary<byte, int>
            {
                { 0, 0 }, { 16, 1 }, { 17, 0 }, { 18, 1 }, { 19, 1 }, { 20, 1 }, { 21, 0 }, { 22, 16 },
                { 23, 12 }, { 24, 16 }, { 25, 12 }, { 26, 9 }, { 27, 3 }, { 28, 3 }, { 32, 1 }, { 33, 1 },
                { 34, 1 }, { 35, 2 }, { 36, 1 }, { 37, 1 }, { 38, 1 }, { 39, 1 }, { 40, 1 }, { 41, 1 },
                { 42, 1 }, { 43, 1 }, { 48, 1 }, { 49, 1 }, { 50, 1 }, { 51, 1 }, { 52, 32 }, { 64, 1 },
                { 65, 0 }, { 80, 1 }, { 96, 1 }, { 112, 3 }, { 113, 2 }, { 114, 1 }, { 255, 0 }
            };

            private byte[] DL;

            public DisplayListRecoder(byte[] DL)
            {
                if (DL == null)
                {
                    throw new ArgumentNullException("The display list can not be null.");
                }
                this.DL = DL;
            }

            public byte[] GetDisplayList()
            {
                return DL;
            }

            public void RemoveNormals()
            {
                RemoveCommand(33);
            }

            private void RemoveCommand(byte Nr)
            {
                DisplayListEncoder dle = new DisplayListEncoder();
                int num = 0;
                int len = DL.Length;
                int[] cmds = new int[4];
                while (num < len)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (num >= len)
                        {
                            cmds[i] = 255;
                            continue;
                        }
                        cmds[i] = DL[num];
                        num++;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (num >= len) break;
                        if (cmds[i] != Nr)
                        {
                            IOUtil.ReadU32sLE(DL, num, NrParams[(byte)cmds[i]]);
                        }
                        num += NrParams[(byte)cmds[i]] * 4;
                    }
                }
                DL = dle.GetDisplayList();
            }
        }

        private const float SCALE_IV = 4096f;

        public static void glNitroTexImage2D(Bitmap b, int Nr, int WrapModeS, int WrapModeT, int FilterModeMin, int FilterModeMag)
        {
            Gl.glBindTexture(3553, Nr);
            Gl.glColor3f(1f, 1f, 1f);
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Gl.glTexImage2D(3553, 0, 32856, b.Width, b.Height, 0, 32993, 5121, bmpData.Scan0);
            b.UnlockBits(bmpData);
            Gl.glTexParameteri(3553, 10241, FilterModeMin);
            Gl.glTexParameteri(3553, 10240, FilterModeMag);
            Gl.glTexParameterf(3553, 10242, WrapModeS);
            Gl.glTexParameterf(3553, 10243, WrapModeT);
        }

        public static MTX44 glNitroPivot(float[] ab, int pv, int neg)
        {
            MTX44 mtx = new MTX44();
            mtx.LoadIdentity();
            float sign = 1f;
            float a = ab[0];
            float bVal = ab[1];
            float x = a;
            float y = bVal;

            switch (neg)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 9:
                case 11:
                case 13:
                case 15:
                    sign = -1f; break;
            }
            switch (neg)
            {
                case 2:
                case 3:
                case 6:
                case 7:
                case 10:
                case 11:
                case 14:
                case 15:
                    y = -y; break;
            }
            switch (neg)
            {
                case 4:
                case 5:
                case 6:
                case 7:
                case 12:
                case 13:
                case 14:
                case 15:
                    x = -x; break;
            }

            switch (pv)
            {
                case 0:
                    mtx[0, 0] = sign; mtx[1, 1] = x; mtx[2, 1] = y;
                    mtx[2, 0] = y; mtx[1, 2] = x; break;
                case 1:
                    mtx[0, 1] = sign; mtx[0, 0] = x; mtx[2, 0] = y;
                    mtx[2, 1] = y; mtx[0, 2] = x; break;
                case 2:
                    mtx[0, 2] = sign; mtx[0, 0] = x; mtx[1, 0] = y;
                    mtx[1, 1] = y; mtx[0, 1] = x; break;
            }
            return mtx;
        }

        private static int sign(int data, int size)
        {
            if ((data & (1 << size - 1)) != 0)
            {
                data |= -1 << size;
            }
            return data;
        }
    }
}