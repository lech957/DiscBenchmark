using System;
using System.Diagnostics;
using System.IO;

namespace DiscBenchmark{


public delegate void LogHandler(string msg);

public class StreamStuff{

    public event LogHandler Logging;

    public string FilePath;

    public long FileSize{get;set;}
    public int RewriteCount{get;set;}
    public long ChunkSize{get;set;}

    private int _bufferSize=4096;

    public int LogEveryXMb = 50;

    public long _LogSplit ;

    private byte[][] _writeBucket = new byte[100][];

    private Random rand = new Random();
    private bool inited =false;


    public StreamStuff(string filepath, long filesize, long chunkSize,int rewriteCount){
        FileSize=filesize;
        ChunkSize=chunkSize;
        RewriteCount=rewriteCount;
        FilePath=filepath;
        _LogSplit=(10*1024*1024)/_bufferSize;
    }


    private void Init(){

        Stopwatch sp = Stopwatch.StartNew();
        Log($"init write bucket {_writeBucket.Length}x{_bufferSize}Byte...");
        for (int i=0;i<_writeBucket.Length;i++)
        {
            _writeBucket[i]= new byte[_bufferSize];
            rand.NextBytes(_writeBucket[i]);
        }
        sp.Stop();
        Log($"Finished init write bucket in {sp.Elapsed}");

    }


    private void Log(string msg){
        Logging?.Invoke(msg);
    }    


    public void StartCopy(){
        if (! inited)
            Init();

        Stopwatch sp_all = Stopwatch.StartNew();
        Stopwatch sp = new Stopwatch();
        long logChunk = FileSize / _LogSplit;
        using (FileStream f = new FileStream(FilePath,FileMode.Create,FileAccess.ReadWrite)){
            byte[] buffer = new byte[_bufferSize];
            sp.Restart();
            while (f.Length < FileSize){
                if (f.Length > 0 && f.Length % _LogSplit ==0){
                    f.Flush();
                    sp.Stop();
                    if (sp.ElapsedMilliseconds >0)
                    {
                        decimal speedInMBperSecond = ((1000m*_LogSplit) / (1024*1024)) / (sp.ElapsedMilliseconds) ;
                        Log($"Speed {speedInMBperSecond:2}MB/s");
                    }
                    else{
                        Log($"Too fast, took {sp.Elapsed} for {LogEveryXMb}MB");
                    }
                    sp.Restart();
                }
                buffer= _writeBucket[rand.Next(_writeBucket.Length)];
                f.Write(buffer,0,buffer.Length);

            }
            f.Flush();
            sp.Stop();
        }
        sp_all.Stop();
        double sec = sp_all.ElapsedMilliseconds/1000d;
        double rate = (FileSize/1024/1024 )/sec;
        Log($"Done in {sp_all.Elapsed} rate: {rate}MB/s");

    }


    public void StartRewrite(){

    }

}

}