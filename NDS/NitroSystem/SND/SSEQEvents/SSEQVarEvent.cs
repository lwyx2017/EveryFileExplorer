using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQVarEvent : SSEQEvent
    {
    	public new byte EventID { get; private set; }

    	public byte VariableID1 { get; private set; }

    	public byte VariableID2 { get; private set; }

    	public SSEQVarEvent(byte EventID, EndianBinaryReader er)
    	{
    		this.EventID = EventID;
    		EventID = er.ReadByte();
    		VariableID1 = er.ReadByte();
    		if (EventID >= 176 && EventID <= 189)
    		{
    			VariableID2 = er.ReadByte();
    		}
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}