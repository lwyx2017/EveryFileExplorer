using LibEveryFileExplorer.IO;
using System;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQRandVarEvent : SSEQEvent
    {
    	public byte VarID { get; private set; }

    	public short Value { get; private set; }

    	public SSEQRandVarEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		VarID = er.ReadByte();
    		Value = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Random random = new Random((int)DateTime.Now.Ticks);
    		if (Value < 0)
    		{
    			if (VarID > 15)
    			{
    				Result.GlobalVariables[VarID - 16] = (short)random.Next(Value, 0);
    			}
    			else
    			{
    				Result.LocalVariables[VarID] = (short)random.Next(Value, 0);
    			}
    		}
    		else if (VarID > 15)
    		{
    			Result.GlobalVariables[VarID - 16] = (short)random.Next(0, Value);
    		}
    		else
    		{
    			Result.LocalVariables[VarID] = (short)random.Next(0, Value);
    		}
    	}
    }
}