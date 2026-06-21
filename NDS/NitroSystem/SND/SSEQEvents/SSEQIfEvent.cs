using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQIfEvent : SSEQEvent
    {
    	public SSEQIfEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.If = true;
    	}

    	public override string ToString()
    	{
    		return "sound.sseq.events.if";
    	}
    }
}