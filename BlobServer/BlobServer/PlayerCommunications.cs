using System.Net.Sockets;
using System.Threading;
/**
 * PlayerCommunications.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: Stores info about client.
 * periodicaly sends and recives data.
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobServer
{
    class PlayerCommunications
    {       
        private readonly NetworkStream _blobStream;
        private readonly NetworkStream _moveStream;
        private readonly Blob _blob;
        private readonly GameState _gameState;

        //initalizes class variables and starts threads to listen to and write to client
        public PlayerCommunications(NetworkStream move, NetworkStream blobs, GameState gs)
        {
            _blobStream = blobs;
            _moveStream = move;
            _gameState = gs;
            _blob = _gameState.AddBlob();

            var readerThread = new Thread(ReadMotions) {Name = "MotionReader Thread"};
            readerThread.Start();

            var writerThread = new Thread(WriteBlobs) {Name = "BlobWriter Thread"};
            writerThread.Start();
        }

        //periodiclay moves a blob, until the end of time.. . . . .  .   .    .    . I need to write a dispose method
        private void ReadMotions()
        {
            while (true)
            {
                _gameState.MoveBlob(_moveStream,_blob);
                Thread.Sleep(100);
            }
        }

        //periodiclay sends blob data to the client, until the end of time.. . . . .  .   .    .    . I still need to write a dispose method
        private void WriteBlobs()
        {
            while (true)
            {
                _gameState.WriteState(_blobStream,_blob);
                var ping = new byte[1];
                _blobStream.Read(ping, 0, ping.Length);
                _blobStream.Flush();
                Thread.Sleep(100);
            }
        }



    }
}
