using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQTempoEvent : SSEQEvent
    {
    	public short Tempo { get; private set; }

    	public SSEQTempoEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Tempo = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new TempoEvent((int)(60000000.0 / (double)Tempo), Result.CurrentTime));
    	}

    	public override string ToString()
    	{
    		return ("Tempo") + " (" + Tempo + " bpm)";
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 0, 0);
    	}
    }
}