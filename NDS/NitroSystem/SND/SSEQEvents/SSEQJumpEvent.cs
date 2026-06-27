using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQJumpEvent : SSEQEvent
    {
    	public uint Offset { get; private set; }

    	public SSEQJumpEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Offset = (uint)IOUtil.ReadU24LE(er.ReadBytes(3), 0);
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new TextEvent("loopEnd", MetaEventType.Marker, Result.CurrentTime));
    	}

    	public override string ToString()
    	{
    		return "Jump";
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 7, 7);
    	}
    }
}
