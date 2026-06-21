using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQVolumeEvent : SSEQEvent
    {
    	public byte Volume { get; private set; }

    	public SSEQVolumeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Volume = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new ControlChangeEvent(Result.CurrentTime, Result.TrackID + 1, MidiController.MainVolume, Volume));
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 8, 8);
    	}
    }
}