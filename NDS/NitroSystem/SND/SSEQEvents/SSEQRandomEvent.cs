using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQRandomEvent : SSEQEvent
    {
    	public new byte EventID { get; private set; }

    	public short Min { get; private set; }

    	public short Max { get; private set; }

    	public SSEQRandomEvent(byte EventID, EndianBinaryReader er)
    	{
    		this.EventID = EventID;
    		EventID = er.ReadByte();
    		Min = er.ReadInt16();
    		Max = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}