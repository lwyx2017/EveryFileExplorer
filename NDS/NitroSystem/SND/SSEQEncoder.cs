using System.Collections.Generic;
using NDS.NitroSystem.SND.SSEQEvents;
using NAudio.Midi;

namespace NDS.NitroSystem.SND
{
    public class SSEQEncoder
    {
    	private List<SSEQEvent> Events;

    	public SSEQEncoder(string MidiPath)
    	{
    		MidiFile midiFile = new MidiFile(MidiPath);
    		Events = new List<SSEQEvent>();
    		Events.Add(new SSEQAllocTrackEvent(midiFile.Tracks));
    		for (int i = 0; i < midiFile.Tracks - 1; i++)
    		{
    			Events.Add(new SSEQTrackEvent(i + 1));
    		}
    		foreach (MidiEventCollection @event in midiFile.Events)
    		{
    			List<SSEQEvent> list = new List<SSEQEvent>();
    			foreach (MidiEvent item in @event)
    			{
    				if (item is NoteOnEvent)
    				{
    					list.Add(new SSEQNoteEvent((NoteOnEvent)item));
    				}
    			}
    			Events.AddRange(list);
    		}
    	}
    }
}