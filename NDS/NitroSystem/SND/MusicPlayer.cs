using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NDS.NitroSystem.SND
{
    public class MusicPlayer
    {
        private const int FluidFailed = -1;
        private const int FluidPlayerDone = 2;

        private readonly object syncRoot = new object();

        private IntPtr settings;
        private IntPtr synth;
        private IntPtr audioDriver;
        private IntPtr player;
        private int soundFontId = -1;
        private int fallbackSoundFontId = -1;

        private Timer loopTimer;
        private bool looping;
        private int completedRepeats;

        private bool useDLS;
        private string dls = "";
        private string midi = "";
        private bool useLoop;
        private int loopStart;
        private int loopEnd;
        private int times = -1;

        public void SetDLS(string Path)
        {
            if (dls != "")
            {
                File.Delete(dls);
                dls = "";
            }
            dls = Path;
            useDLS = true;
        }

        public void UnloadDLS()
        {
            useDLS = false;
        }

        public void SetMidi(string Path)
        {
            midi = Path;
        }

        public int GetLength()
        {
            lock (syncRoot)
            {
                StopPlaybackLocked();
                EnsureInitializedLocked();
                if (String.IsNullOrEmpty(midi) || !File.Exists(midi) || !LoadMidiLocked())
                {
                    return 0;
                }
                return NativeMethods.fluid_player_get_total_ticks(player);
            }
        }

        public void Play()
        {
            lock (syncRoot)
            {
                StopPlaybackLocked();
                EnsureInitializedLocked();
                if (String.IsNullOrEmpty(midi) || !File.Exists(midi))
                {
                    return;
                }
                if (!LoadMidiLocked())
                {
                    throw new InvalidOperationException("FluidSynth could not load the MIDI file: " + midi);
                }

                LoadSoundFontLocked();
                completedRepeats = 0;
                looping = useLoop;
                if (NativeMethods.fluid_player_play(player) == FluidFailed)
                {
                    throw new InvalidOperationException("FluidSynth could not start MIDI playback.");
                }
                if (looping)
                {
                    loopTimer = new Timer(CheckLoop, null, 0, 10);
                }
            }
        }

        public void SetLoop(int Start, int End, int times)
        {
            useLoop = true;
            loopEnd = End;
            this.times = times;
            loopStart = Start;
        }

        public void Unload()
        {
            lock (syncRoot)
            {
                StopTimerLocked();
                DeletePlayerLocked();
                if (audioDriver != IntPtr.Zero)
                {
                    NativeMethods.delete_fluid_audio_driver(audioDriver);
                    audioDriver = IntPtr.Zero;
                }
                if (synth != IntPtr.Zero)
                {
                    NativeMethods.delete_fluid_synth(synth);
                    synth = IntPtr.Zero;
                }
                if (settings != IntPtr.Zero)
                {
                    NativeMethods.delete_fluid_settings(settings);
                    settings = IntPtr.Zero;
                }
                soundFontId = -1;
                fallbackSoundFontId = -1;
                useLoop = false;
                looping = false;
                midi = "";
                times = -1;
                useDLS = false;
            }
        }

        public void Stop()
        {
            lock (syncRoot)
            {
                StopPlaybackLocked();
            }
        }

        private void EnsureInitializedLocked()
        {
            if (settings != IntPtr.Zero)
            {
                return;
            }

            try
            {
                NativeMethods.EnsureLibraryLoaded();
                settings = NativeMethods.new_fluid_settings();
            }
            catch (DllNotFoundException ex)
            {
                throw new DllNotFoundException("Unable to load libfluidsynth-3.dll from the Plugins directory. Make sure libfluidsynth-3.dll, SDL3.dll and sndfile.dll are placed next to NDS.dll.", ex);
            }
            if (settings == IntPtr.Zero)
            {
                throw new InvalidOperationException("FluidSynth could not create the settings object.");
            }

            NativeMethods.fluid_settings_setnum(settings, "synth.sample-rate", 44100.0);
            NativeMethods.fluid_settings_setint(settings, "synth.polyphony", 128);
            synth = NativeMethods.new_fluid_synth(settings);
            if (synth == IntPtr.Zero)
            {
                NativeMethods.delete_fluid_settings(settings);
                settings = IntPtr.Zero;
                throw new InvalidOperationException("FluidSynth could not create the synthesizer.");
            }

            audioDriver = NativeMethods.new_fluid_audio_driver(settings, synth);
            if (audioDriver == IntPtr.Zero)
            {
                NativeMethods.delete_fluid_synth(synth);
                NativeMethods.delete_fluid_settings(settings);
                synth = IntPtr.Zero;
                settings = IntPtr.Zero;
                throw new InvalidOperationException("FluidSynth could not create the audio output device.");
            }
        }

        private bool LoadMidiLocked()
        {
            DeletePlayerLocked();
            player = NativeMethods.new_fluid_player(synth);
            if (player == IntPtr.Zero)
            {
                return false;
            }
            if (NativeMethods.fluid_player_add(player, ToUtf8(midi)) == FluidFailed)
            {
                DeletePlayerLocked();
                return false;
            }
            return true;
        }

        private void LoadSoundFontLocked()
        {
            if (soundFontId >= 0)
            {
                NativeMethods.fluid_synth_sfunload(synth, soundFontId, 1);
                soundFontId = -1;
            }
            if (fallbackSoundFontId >= 0)
            {
                NativeMethods.fluid_synth_sfunload(synth, fallbackSoundFontId, 1);
                fallbackSoundFontId = -1;
            }
            if (!useDLS)
            {
                LoadFallbackSoundBankLocked();
                return;
            }
            if (String.IsNullOrEmpty(dls) || !File.Exists(dls))
            {
                throw new FileNotFoundException("The DLS sound bank file was not found.", dls);
            }

            soundFontId = NativeMethods.fluid_synth_sfload(synth, ToUtf8(dls), 1);
            if (soundFontId == FluidFailed)
            {
                throw new InvalidOperationException("FluidSynth could not load the DLS sound bank. Make sure the current libfluidsynth build includes libinstpatch/DLS support: " + dls);
            }
        }

        private void LoadFallbackSoundBankLocked()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "gm.dls");
            if (!File.Exists(path))
            {
                return;
            }
            fallbackSoundFontId = NativeMethods.fluid_synth_sfload(synth, ToUtf8(path), 1);
            if (fallbackSoundFontId == FluidFailed)
            {
                fallbackSoundFontId = -1;
            }
        }

        private void StopPlaybackLocked()
        {
            StopTimerLocked();
            looping = false;
            if (player != IntPtr.Zero)
            {
                NativeMethods.fluid_player_stop(player);
            }
            if (synth != IntPtr.Zero)
            {
                NativeMethods.fluid_synth_all_sounds_off(synth, -1);
            }
        }

        private void DeletePlayerLocked()
        {
            if (player != IntPtr.Zero)
            {
                NativeMethods.fluid_player_stop(player);
                NativeMethods.delete_fluid_player(player);
                player = IntPtr.Zero;
            }
        }

        private void CheckLoop(object state)
        {
            lock (syncRoot)
            {
                if (player == IntPtr.Zero || !looping)
                {
                    return;
                }
                if (NativeMethods.fluid_player_get_status(player) == FluidPlayerDone)
                {
                    StopTimerLocked();
                    looping = false;
                    return;
                }
                if (NativeMethods.fluid_player_get_current_tick(player) < loopEnd)
                {
                    return;
                }

                if (times == -1 || completedRepeats < times)
                {
                    NativeMethods.fluid_player_seek(player, loopStart);
                    completedRepeats++;
                }
                else
                {
                    looping = false;
                }
            }
        }

        private void StopTimerLocked()
        {
            if (loopTimer != null)
            {
                loopTimer.Dispose();
                loopTimer = null;
            }
        }

        private static byte[] ToUtf8(string value)
        {
            return Encoding.UTF8.GetBytes(value + "\0");
        }

        private static class NativeMethods
        {
            private const string LibraryName = "libfluidsynth-3.dll";
            private const uint LoadWithAlteredSearchPath = 0x00000008;

            private static bool libraryLoaded;

            internal static void EnsureLibraryLoaded()
            {
                if (libraryLoaded)
                {
                    return;
                }

                lock (typeof(NativeMethods))
                {
                    if (libraryLoaded)
                    {
                        return;
                    }

                    string pluginDirectory = Path.GetDirectoryName(typeof(MusicPlayer).Assembly.Location);
                    string libraryPath = Path.Combine(pluginDirectory, LibraryName);
                    if (!File.Exists(libraryPath))
                    {
                        throw new DllNotFoundException("libfluidsynth-3.dll was not found in the Plugins directory: " + pluginDirectory);
                    }

                    if (LoadLibraryEx(libraryPath, IntPtr.Zero, LoadWithAlteredSearchPath) == IntPtr.Zero)
                    {
                        throw new DllNotFoundException("libfluidsynth-3.dll exists but could not be loaded. Verify that it is the x64 build and that its dependencies are in the same directory: " + libraryPath);
                    }

                    libraryLoaded = true;
                }
            }

            [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern IntPtr LoadLibraryEx(string fileName, IntPtr file, uint flags);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr new_fluid_settings();

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            internal static extern int fluid_settings_setnum(IntPtr settings, string name, double value);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            internal static extern int fluid_settings_setint(IntPtr settings, string name, int value);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void delete_fluid_settings(IntPtr settings);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr new_fluid_synth(IntPtr settings);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void delete_fluid_synth(IntPtr synth);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr new_fluid_audio_driver(IntPtr settings, IntPtr synth);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void delete_fluid_audio_driver(IntPtr driver);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_synth_sfload(IntPtr synth, byte[] filename, int resetPresets);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_synth_sfunload(IntPtr synth, int id, int resetPresets);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_synth_all_sounds_off(IntPtr synth, int channel);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr new_fluid_player(IntPtr synth);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void delete_fluid_player(IntPtr player);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_add(IntPtr player, byte[] midiFile);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_play(IntPtr player);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_stop(IntPtr player);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_get_status(IntPtr player);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_get_current_tick(IntPtr player);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_get_total_ticks(IntPtr player);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int fluid_player_seek(IntPtr player, int ticks);
        }
    }
}