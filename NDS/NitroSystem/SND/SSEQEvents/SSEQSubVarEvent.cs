using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQSubVarEvent : SSEQEvent
    {
    	public byte VarID { get; private set; }

    	public short Value { get; private set; }

    	public SSEQSubVarEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		VarID = er.ReadByte();
    		Value = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		if (VarID > 15)
    		{
    			Result.GlobalVariables[VarID - 16] -= Value;
    		}
    		else
    		{
    			Result.LocalVariables[VarID] -= Value;
    		}
    	}
    }
}