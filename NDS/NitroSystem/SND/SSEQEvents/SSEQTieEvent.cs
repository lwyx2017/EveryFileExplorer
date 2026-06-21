using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQTieEvent : SSEQEvent
    {
    	public byte Tie { get; private set; }

    	public SSEQTieEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Tie = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.Tie = Tie == 1;
    	}
    }
}