using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQShiftVarEvent : SSEQEvent
    {
    	public byte VarID { get; private set; }

    	public short Value { get; private set; }

    	public SSEQShiftVarEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		VarID = er.ReadByte();
    		Value = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		if (Value < 0)
    		{
    			if (VarID > 15)
    			{
    				ref short reference = ref Result.GlobalVariables[VarID - 16];
    				reference = (short)(reference >> -Value);
    			}
    			else
    			{
    				ref short reference2 = ref Result.LocalVariables[VarID];
    				reference2 = (short)(reference2 >> -Value);
    			}
    		}
    		else if (VarID > 15)
    		{
    			Result.GlobalVariables[VarID - 16] <<= (int)Value;
    		}
    		else
    		{
    			Result.LocalVariables[VarID] <<= (int)Value;
    		}
    	}
    }
}