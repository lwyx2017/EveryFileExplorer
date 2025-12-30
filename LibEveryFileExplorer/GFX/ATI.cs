namespace LibEveryFileExplorer.GFX
{
    public class ATI
    {
        public static byte[] DecodeATI1(byte R0, byte R1, ulong Data, bool IsSNORM = false)
        {
            if (IsSNORM)
            {
                R0 = (byte)System.Math.Round((((sbyte)R0) / 127f + 1f) * 127.5f);
                R1 = (byte)System.Math.Round((((sbyte)R1) / 127f + 1f) * 127.5f);
            }
            byte[] palette = new byte[8];
            palette[0] = R0;
            palette[1] = R1;
            if (R0 > R1)
            {
                palette[2] = (byte)((6 * R0 + R1 + 3) / 7);
                palette[3] = (byte)((5 * R0 + 2 * R1 + 3) / 7);
                palette[4] = (byte)((4 * R0 + 3 * R1 + 3) / 7);
                palette[5] = (byte)((3 * R0 + 4 * R1 + 3) / 7);
                palette[6] = (byte)((2 * R0 + 5 * R1 + 3) / 7);
                palette[7] = (byte)((R0 + 6 * R1 + 3) / 7);
            }
            else
            {
                palette[2] = (byte)((4 * R0 + R1 + 2) / 5);
                palette[3] = (byte)((3 * R0 + 2 * R1 + 2) / 5);
                palette[4] = (byte)((2 * R0 + 3 * R1 + 2) / 5);
                palette[5] = (byte)((R0 + 4 * R1 + 2) / 5);
                palette[6] = 0;
                palette[7] = 255;
            }
            byte[] result = new byte[16];
            int q = 0;
            for (int i = 0; i < 16; i++)
            {
                int index = (int)((Data >> q) & 7);
                result[i] = palette[index];
                if (IsSNORM) result[i] = (byte)System.Math.Round((((sbyte)result[i]) / 127f + 1f) * 127.5f);
                q += 3;
            }
            return result;
        }

        public static uint[] DecodeATI2(byte Lum0, byte Lum1, ulong LumData, byte Alpha0, byte Alpha1, ulong AlphaData, bool IsSNORM = false)
        {
            byte[] lumPixels = DecodeATI1(Lum0, Lum1, LumData, IsSNORM);
            byte[] alphaPixels = DecodeATI1(Alpha0, Alpha1, AlphaData, IsSNORM);
            uint[] result = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                byte lum = lumPixels[i];
                byte alpha = alphaPixels[i];
                result[i] = GFXUtil.ToColorFormat(alpha, lum, lum, lum, ColorFormat.ARGB8888);
            }
            return result;
        }
    }
}