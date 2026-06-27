using LibEveryFileExplorer.IO;
using NAudio.Midi;
using System;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQModulationDelayEvent : SSEQEvent
    {
    	public short ModulationDelay { get; private set; }

    	public SSEQModulationDelayEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		ModulationDelay = er.ReadInt16();
    	}

        public override void AddMidiEvents(ref SSEQMidiResult Result)
        {
            try
            {
                int rawCcValue = 64 + ModulationDelay / 2;
                int safeCcValue = rawCcValue;
                if (safeCcValue < 0) safeCcValue = 0;
                if (safeCcValue > 127) safeCcValue = 127;
                MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(78, safeCcValue, Result.TrackID + 1).RawData);
                midiEvent.AbsoluteTime = Result.CurrentTime;
                Result.MidiTrack.Add(midiEvent);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
    }
}