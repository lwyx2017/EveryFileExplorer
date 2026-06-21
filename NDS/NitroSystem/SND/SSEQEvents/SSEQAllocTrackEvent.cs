using System.Windows.Forms;
using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQAllocTrackEvent : SSEQEvent
    {
    	public bool[] TrackUsed;

    	public ushort TracksUsed { get; private set; }

    	public SSEQAllocTrackEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		TracksUsed = er.ReadUInt16();
    		TrackUsed = new bool[16];
    		for (int i = 0; i < 16; i++)
    		{
    			TrackUsed[i] = ((TracksUsed >> i) & 1) == 1;
    		}
    	}

    	public SSEQAllocTrackEvent(int NrTracks)
    	{
    		base.EventID = 254;
    		TracksUsed = 0;
    		TrackUsed = new bool[16];
    		for (int i = 0; i < 16; i++)
    		{
    			if (i < NrTracks)
    			{
    				TrackUsed[i] = true;
    				TracksUsed |= (ushort)(1 << i);
    			}
    			else
    			{
    				TrackUsed[i] = false;
    			}
    		}
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}

    	public override string ToString()
    	{
    		return "sound.sseq.events.alloc";
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 2, 2);
    	}
    }
}