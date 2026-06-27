using System.IO;
using DirectMidi;

namespace NDS.NitroSystem.SND
{
    public class MusicPlayer
    {
    	private CDirectMusic CDMusic = new CDirectMusic();

    	private CDLSLoader CLoader = new CDLSLoader();

    	private COutputPort COutPort = new COutputPort();

    	private CAPathPerformance CAPathPerformance = new CAPathPerformance();

    	private CCollection CCollectionA = new CCollection();

    	private CSegment CSegment1 = new CSegment();

    	private INFOPORT PortInfo;

    	private bool UseDLS = false;

    	private string DLS = "";

    	private string Midi = "";

    	private bool useLoop = false;

    	private int LoopStart;

    	private int LoopEnd;

    	private int times;

    	public MusicPlayer()
    	{
    		CDMusic.Initialize();
    		CAPathPerformance.Initialize(CDMusic, null, null, DMUS_APATH.DYNAMIC_STEREO, 128);
    		CLoader.Initialize();
    		COutPort.Initialize(CDMusic);
    	}

    	public void SetDLS(string Path)
    	{
    		if (DLS != "")
    		{
    			File.Delete(DLS);
    			DLS = "";
    		}
    		DLS = Path;
    		UseDLS = true;
    	}

    	public void UnloadDLS()
    	{
    		UseDLS = false;
    	}

    	public void SetMidi(string Path)
    	{
    		Midi = Path;
    	}

    	public int GetLength()
    	{
    		if (CAPathPerformance != null)
    		{
    			CAPathPerformance.StopAll();
    		}
    		CSegment1.ReleaseSegment();
    		CSegment1.UnloadAllPerformances();
    		CLoader.UnloadCollection(CCollectionA);
    		CLoader.LoadSegment(Midi, out CSegment1, bIsMIDIFile: true);
    		if (UseDLS)
    		{
    			CLoader.LoadDLS(DLS, out CCollectionA);
    			CSegment1.ConnectToDLS(CCollectionA);
    		}
    		CSegment1.Download(CAPathPerformance);
    		CSegment1.GetLength(out var pmtLength);
    		return pmtLength;
    	}

    	public void Play()
    	{
    		if (CAPathPerformance != null)
    		{
    			CAPathPerformance.StopAll();
    		}
    		CSegment1.ReleaseSegment();
    		CSegment1.UnloadAllPerformances();
    		CLoader.UnloadCollection(CCollectionA);
    		CLoader.LoadSegment(Midi, out CSegment1, bIsMIDIFile: true);
    		if (UseDLS)
    		{
    			CLoader.LoadDLS(DLS, out CCollectionA);
    			CSegment1.ConnectToDLS(CCollectionA);
    			CLoader.UnloadCollection(CCollectionA);
    			CCollectionA.Dispose();
    			CCollectionA = new CCollection();
    		}
    		CSegment1.Download(CAPathPerformance);
    		if (useLoop)
    		{
    			CSegment1.SetLoopPoints(LoopStart, LoopEnd);
    			if (times != -1)
    			{
    				CSegment1.SetRepeats((uint)times);
    			}
    			else
    			{
    				CSegment1.SetRepeats(DMUS_SEG.REPEAT_INFINITE);
    			}
    		}
    		CAPathPerformance.PlaySegment(CSegment1, null);
    	}

    	public void SetLoop(int Start, int End, int times)
    	{
    		useLoop = true;
    		LoopEnd = End;
    		this.times = times;
    		LoopStart = Start;
    	}

    	public void Unload()
    	{
    		useLoop = false;
    		Midi = "";
    		times = -1;
    		if (CAPathPerformance != null)
    		{
    			CAPathPerformance.StopAll();
    		}
    		CSegment1.ReleaseSegment();
    		CSegment1.UnloadAllPerformances();
    		CSegment1.Dispose();
    		CLoader.Dispose();
    		COutPort.ReleasePort();
    		CSegment1 = null;
    		CLoader = null;
    		CSegment1 = new CSegment();
    		CLoader = new CDLSLoader();
    		CLoader.Initialize();
    		UseDLS = false;
    	}

    	public void Stop()
    	{
    		CAPathPerformance.StopAll();
    	}
    }
}