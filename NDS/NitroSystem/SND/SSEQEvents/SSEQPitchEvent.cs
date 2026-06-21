using System;
using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPitchEvent : SSEQEvent
    {
        public sbyte PitchOffset { get; private set; }

        public SSEQPitchEvent(byte EventID, EndianBinaryReader endianReader)
        {
            base.EventID = EventID;
            PitchOffset = endianReader.ReadSByte();
        }

        public override void AddMidiEvents(ref SSEQMidiResult midiConvertResult)
        {
            short pitchWheelValue = (short)((PitchOffset + 128) * 64);
            if (pitchWheelValue >= 0 && pitchWheelValue <= 16384)
            {
                midiConvertResult.MidiTrack.Add(new PitchWheelChangeEvent(midiConvertResult.CurrentTime, midiConvertResult.TrackID + 1, pitchWheelValue));
                return;
            }
            throw new Exception("Pitch isn't in a range between 0 and 0x4000.");
        }
    }
}