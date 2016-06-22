using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
/**
 * GameState.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: Central Class that manages game data access,
 * calculates interactions and reads+writes to player streams 
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobServer
{
    class GameState
    {
        private List<Blob> _blobs;

        private const double Area = 20;
        private static readonly Color[] BlobColors = { Color.Aqua, Color.Blue, Color.Green, Color.Yellow, Color.Orange, Color.Red, Color.Purple };

        public GameState()
        {
            _blobs = new List<Blob>();
            var r = new Random();
            for (int i = 0; i < 20; i++) {
                var b = new Blob(r.NextDouble() * Area, r.NextDouble() * Area, r.NextDouble() * Area, 0.2, Color.White);
                lock (_blobs) {
                    _blobs.Add(b);
                }
            }

        }
        //adds a new random player blbo to _blobs and returns a pointer
        public Blob AddBlob()
        {
            var r = new Random();
            var b = new Blob(Area * r.NextDouble(), Area * r.NextDouble(), Area * r.NextDouble(), 2, BlobColors[r.Next(BlobColors.Length)]);
            lock (_blobs) {
                _blobs.Add(b);
            }
            return b;
        }

        //reads data from a clitent stream and moves a blob accordingly
        public void MoveBlob(NetworkStream moveStream, Blob b)
        {

            var moveBytes = new byte[8 * 3];
            moveStream.Read(moveBytes, 0, moveBytes.Length);
            var xm = BitConverter.ToDouble(moveBytes, 0);
            var ym = BitConverter.ToDouble(moveBytes, 8);
            var zm = BitConverter.ToDouble(moveBytes, 16);
            double nx, ny, nz;
            lock (_blobs) {
                nx = b.X + xm;
                ny = b.Y + ym;
                nz = b.Z + zm;
            }
            if (nx < Area && nx > 0 && ny < Area && ny > 0 && nz < Area && nz > 0) {
                b.X = nx;
                b.Y = ny;
                b.Z = nz;
            }

        }

        //writes the infromation contained in _blobs to a client stream
        public void WriteState(NetworkStream blobStream, Blob b)
        {
            byte[] buffer = new byte[8 + 36 * _blobs.Count];

            lock (_blobs) {
                Array.Copy(BitConverter.GetBytes(_blobs.IndexOf(b)), 0, buffer, 0, 4);
                Array.Copy(BitConverter.GetBytes(_blobs.Count), 0, buffer, 4, 4);
                for (var i = 0; i < _blobs.Count; i++) {
                    var blobBytes = ToCereal(_blobs[i]);
                    Array.Copy(blobBytes, 0, buffer, 8 + i * 36, 36);
                }
            }
            blobStream.Write(buffer, 0, buffer.Length);
        }

        //Checks to see if blobs have eaten eachother and changes sizes accordingly
        public void RunGame()
        {
            var blobsCopy = new List<Blob>();
            lock (_blobs) {
                blobsCopy.AddRange(_blobs);
            }
            List<Blob> blobsAfter = new List<Blob>();
            foreach (var blob1 in blobsCopy) {
                var stillAlive = true;
                foreach (var blob2 in blobsCopy) {
                    if (blob1 != blob2) {
                        var distance =
                            Math.Sqrt(Math.Pow(blob1.X - blob2.X, 2) +
                                      Math.Pow(blob1.Y - blob2.Y, 2) +
                                      Math.Pow(blob1.Z - blob2.Z, 2));
                        if (distance < blob1.Radius + blob2.Radius) {
                            if (blob1.Radius > blob2.Radius) {
                                blob1.Radius += blob2.Radius;
                            }
                            else if (blob1.Radius < blob2.Radius) {
                                stillAlive = false;
                                break;
                            }
                        }
                    }
                }
                if (stillAlive) {
                    blobsAfter.Add(blob1);
                }
            }
            lock (_blobs) {
                _blobs = blobsAfter;
            }
        }

        //returns a byte array with the infromation from a blob
        public byte[] ToCereal(Blob b)
        {
            var bytes = new byte[36];
            var xBytes = BitConverter.GetBytes(b.X);
            var yBytes = BitConverter.GetBytes(b.Y);
            var zBytes = BitConverter.GetBytes(b.Z);
            var rBytes = BitConverter.GetBytes(b.Radius);
            var cBytes = BitConverter.GetBytes(b.Color.ToArgb());
            Array.Copy(xBytes, 0, bytes, 0, 8);
            Array.Copy(yBytes, 0, bytes, 8, 8);
            Array.Copy(zBytes, 0, bytes, 16, 8);
            Array.Copy(rBytes, 0, bytes, 24, 8);
            Array.Copy(cBytes, 0, bytes, 32, 4);
            return bytes;
        }
    }
}
