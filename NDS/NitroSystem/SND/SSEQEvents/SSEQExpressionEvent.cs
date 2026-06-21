using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQExpressionEvent : SSEQEvent
    {
    	public byte Expression { get; private set; }

    	public SSEQExpressionEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Expression = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}