using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
/**
 * GameState.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: Central Class that manages game data access,
 * calculates interactions and reads+writes to player streams 
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobSurvival
{
    class GameState
    {
        //this is manual right now. unfortunatly
        const string ServerIp = "127.0.0.1";

        private readonly NetworkStream _blobStream;
        private readonly NetworkStream _moveStream;

        private readonly object _blobLock = new object();
        private List<Blob> _blobs = new List<Blob>();
        private Blob _playerBlob;

        private readonly object _moveLock = new object();
        private double _xM;
        private double _yM;
        private double _zM;

        //conect to the server and start threads to write to and read from the network streams
        public GameState(int portNo)
        {
            var movePortNo = portNo + 1;
            var client = new TcpClient(ServerIp, portNo);
            _blobStream = client.GetStream();

            var client2 = new TcpClient(ServerIp, movePortNo);
            _moveStream = client2.GetStream();


            var readerThread = new Thread(ReadBlobs) {Name = "BlobReader Thread"};
            readerThread.Start();

            var writerThread = new Thread(WriteMotion) {Name = "MotionWriter Thread"};
            writerThread.Start();
        }

        //periodicaly called by tread, clears buffer of motion requests and sends to server
        private void WriteMotion()
        {
            while (true)
            {
                double xm, ym, zm;
                lock (_moveLock)
                {
                    xm = _xM;
                    ym = _yM;
                    zm = _zM;
                    _xM = 0;
                    _yM = 0;
                    _zM = 0;
                }
        
                SendMotion(xm, ym, zm);
                Thread.Sleep(100);
            }
        }

        //writes bytes to networkstream inorder to comunicate requested motion to the server
        private void SendMotion(double xm, double ym, double zm)
        {
            var moveBytes = new byte[8*3];
            Array.Copy(BitConverter.GetBytes(xm), 0, moveBytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(ym), 0, moveBytes, 8, 8);
            Array.Copy(BitConverter.GetBytes(zm), 0, moveBytes, 16, 8);

            _moveStream.Write(moveBytes,0,8*3);
  
        }

        // periodicaly called by thread to get the new locatons and sizes of everything form the server.
        private void ReadBlobs()
        {
            while (true)
            {
                Blob playerBlob;
                var blobs = GetBlobs(out playerBlob);

                lock (_blobLock)
                {
                    _playerBlob = playerBlob;
                    _blobs = blobs;
                }

                Thread.Sleep(100);
            }
        }

        //interperates bytes from the network stream into list of blobs
        private List<Blob> GetBlobs(out Blob player)
        {

                    var startBytes = new byte[8];
                    _blobStream.Read(startBytes, 0, 8);
                    var playerIndex = BitConverter.ToInt32(startBytes, 0);
                    var lenght = BitConverter.ToInt32(startBytes, 4);

                    var blobList = new byte[36*lenght];
                    _blobStream.Read(blobList, 0, blobList.Length);
          
                    byte[] ping = {0xAE};
                    _blobStream.Write(ping,0,ping.Length);

                    var blobs = new List<Blob>();
                    for (var i = 0; i < lenght; i++)
                    {
                        var bl = ReadBytes(blobList, i*36);
                        blobs.Add(bl);
                    }
                    player = blobs[playerIndex];
                    return blobs;
        }

        //reads bytes to a blob
        private static Blob ReadBytes(byte[] bytes, int startIndex)
        {
            var x = BitConverter.ToDouble(bytes, startIndex);
            var y = BitConverter.ToDouble(bytes, startIndex + 8);
            var z = BitConverter.ToDouble(bytes, startIndex + 16);
            var r = BitConverter.ToDouble(bytes, startIndex + 24);
            var color = BitConverter.ToInt32(bytes, startIndex + 32);
            return new Blob(x, y, z, r, Color.FromArgb(color));
        }

        //locks onto the blob list and returs it
        public List<Blob> GetBlobs()
        {
            lock (_blobLock)
            {
                return _blobs;
            }      
        }

        //locks onto the blob list and returns the player blob
        public Blob GetPlayer()
        {
            lock (_blobLock)
            {
                return _playerBlob;
            }

        }
        //adds to the buffer of requested motion
        public void MovePlayer(double xm, double ym, double zm)
        {
            lock (_moveLock)
            {
                _xM += xm;
                _yM += ym;
                _zM += zm;
            }
        }
    }
}
