﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Kinect;
using KinectSerializer;
using System.Diagnostics;
using System.Windows.Threading;

namespace Tiny
{
    class KinectServer
    {
        private TcpListener tcpListener;
        private Thread listenForConnectionThread;

        private UserTracker userTracker;
        private CombinedBodyViewer combinedBodyViewer;
        private TrackingBodyViewer trackingBodyViewer;

        public event KinectBodyStreamHandler CombinedStreamUpdate;
        public delegate void KinectBodyStreamHandler(IEnumerable<SerializableBodyFrame> bodyFrames);
        
        public event WorldBodyStreamHandler TrackingAlgorithmUpdate;
        public delegate void WorldBodyStreamHandler(IEnumerable<WorldView> bodyFrames);
        
        public event KinectCalibrationHandler KinectCalibrationUpdate;
        public delegate void KinectCalibrationHandler(bool completed);


        public KinectServer(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenForConnectionThread = new Thread(new ThreadStart(this.ListenForKinectStream));
            this.userTracker = new UserTracker();

            Thread combinedBodyViewerThread = new Thread(new ThreadStart(this.StartCombinedBodyViewerThread));
            combinedBodyViewerThread.SetApartmentState(ApartmentState.STA);
            combinedBodyViewerThread.Start();

            Thread trackingBodyViewerThread = new Thread(new ThreadStart(this.StartTrackingBodyViewerThread));
            trackingBodyViewerThread.SetApartmentState(ApartmentState.STA);
            trackingBodyViewerThread.Start();
        }

        private void StartCombinedBodyViewerThread()
        {
            this.combinedBodyViewer = new CombinedBodyViewer();
            this.combinedBodyViewer.Show();
            this.CombinedStreamUpdate += this.combinedBodyViewer.UpdateBodyStreamDisplay;
            Dispatcher.Run();
        }

        private void StartTrackingBodyViewerThread()
        {
            this.trackingBodyViewer = new TrackingBodyViewer();
            this.trackingBodyViewer.Show();
            this.TrackingAlgorithmUpdate += this.trackingBodyViewer.UpdateTrackingDisplay;
            this.KinectCalibrationUpdate += this.trackingBodyViewer.UpdateCalibrationStatus;
            Dispatcher.Run();
        }

        public void Start()
        {
            this.listenForConnectionThread.Start();
            Debug.WriteLine("Kinect Server: Starting @ " + this.tcpListener.LocalEndpoint);
        }

        private void ListenForKinectStream()
        {
            this.tcpListener.Start();
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(this.HandleKinectStream));
                clientThread.Start(client);
            }
        }

        private void HandleKinectStream(object obj)
        {
            TcpClient client = (TcpClient)obj;
            IPEndPoint clientIP = (IPEndPoint)client.Client.RemoteEndPoint;
            NetworkStream clientStream = client.GetStream();

            Debug.WriteLine("Handling connection from: " + clientIP);

            while (true)
            {
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SerializableBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    this.userTracker.AddOrUpdateBodyFrame(clientIP, bodyFrame);
                    Thread visualUpdateThread = new Thread(new ThreadStart(this.StartVisualUpdateThread));
                    visualUpdateThread.Start();

                    byte[] response = Encoding.ASCII.GetBytes(Properties.Resources.SERVER_RESPONSE_OKAY);
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Flush();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Kinect Server: Exception");
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    clientStream.Close();
                    client.Close();
                }
            }

            this.userTracker.RemoveClient(clientIP);
            clientStream.Close();
            clientStream.Dispose();
            client.Close();
        }

        private void StartVisualUpdateThread()
        {
            this.userTracker.SynchronizeFrames();
            if (this.userTracker.CalibrationCompleted)
            {
                this.KinectCalibrationUpdate(true);
            }
            else
            {
                this.KinectCalibrationUpdate(false);
            }
            IEnumerable<SerializableBodyFrame> userLastKinectFrames = this.userTracker.UserLastKinectFrames;
            Debug.WriteLine("size: " + userLastKinectFrames.Count());
            IEnumerable<WorldView> userLastWorldViews = this.userTracker.UserLastWorldViews;
            this.CombinedStreamUpdate(userLastKinectFrames);
            this.TrackingAlgorithmUpdate(userLastWorldViews);
        }
    }
}
