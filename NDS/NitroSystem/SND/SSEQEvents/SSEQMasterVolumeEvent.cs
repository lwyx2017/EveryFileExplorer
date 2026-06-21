using System.Collections.Generic;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQMasterVolumeEvent : SSEQEvent
    {
    	public byte MasterVolume { get; private set; }

    	public SSEQMasterVolumeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		MasterVolume = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		IList<MidiEvent> midiTrack = Result.MidiTrack;
    		byte[] array = new byte[8] { 240, 127, 127, 4, 1, 0, 0, 247 };
    		array[6] = MasterVolume;
    		midiTrack.Add(new SequencerSpecificEvent(array, Result.CurrentTime));
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 8, 8);
    	}
    }
}