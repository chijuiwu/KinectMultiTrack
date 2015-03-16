using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Kinect;
using KinectSerializer;
using System.Diagnostics;
using System.Windows.Threading;
using KinectMultiTrack.UI;

namespace KinectMultiTrack
{
    public class Server
    {
        private readonly TcpListener serverKinectTCPListener;
        private readonly Thread serverThread;

        private const uint SEC_IN_MILLISEC = 1000;
        private const uint FRAME_IN_SEC = 60;

        private const uint WRITE_LOG_INTERVAL = 1 / 4 * SEC_IN_MILLISEC;
        private const uint FLUSH_LOG_INTERVAL = 3 * SEC_IN_MILLISEC;
        private readonly Stopwatch writeLogStopwatch;
        private readonly Stopwatch flushLogStopwatch;

        private readonly Tracker tracker;
        private TrackingUI trackingUI;

        public event KinectCameraHandler OnAddedKinect;
        public event KinectCameraHandler OnRemovedKinect;
        public delegate void KinectCameraHandler(IPEndPoint kinectClientIP);

        public Server(int port)
        {
            this.serverKinectTCPListener = new TcpListener(IPAddress.Any, port);
            this.serverThread = new Thread(new ThreadStart(this.ServerWorkerThread));

            this.tracker = new Tracker();

            Thread trackingUIThread = new Thread(new ThreadStart(this.StartTrackingUIThread));
            trackingUIThread.SetApartmentState(ApartmentState.STA);
            trackingUIThread.IsBackground = true;
            trackingUIThread.Start();

            this.writeLogStopwatch = new Stopwatch();
            this.flushLogStopwatch = new Stopwatch();
        }

        private void StartStopServer(bool start)
        {
            if (start)
            {
                this.Run();
            }
            else
            {
                this.Stop();
            }
        }

        private void Run()
        {
            this.serverKinectTCPListener.Start();
            this.serverThread.Start();
            this.writeLogStopwatch.Start();
            Debug.WriteLine(KinectMultiTrack.Properties.Resources.SERVER_START + this.serverKinectTCPListener.LocalEndpoint);
        }

        private void Stop()
        {
            this.writeLogStopwatch.Stop();
            this.flushLogStopwatch.Stop();
        }

        private void StartTrackingUIThread()
        {
            this.trackingUI = new TrackingUI();
            this.trackingUI.Show();

            this.trackingUI.OnSetup += this.ConfigureServer;
            this.trackingUI.OnStartStop += this.StartStopServer;
            this.trackingUI.OnDisplayResult += this.Log;

            this.OnAddedKinect += this.trackingUI.Server_AddKinectCamera;
            this.OnRemovedKinect += this.trackingUI.Server_RemoveKinectCamera;
            this.tracker.OnWaitingKinects += this.trackingUI.Tracker_OnWaitingKinects;
            this.tracker.OnCalibration += this.trackingUI.Tracker_OnCalibration;
            this.tracker.OnRecalibration += this.trackingUI.Tracker_OnReCalibration;
            this.tracker.OnResult += this.trackingUI.Tracker_OnResult;

            Dispatcher.Run();
        }

        private void ConfigureServer(int kinectCount)
        {
            this.tracker.Configure(kinectCount);
        }

        // Accepts connections and for each thread spaw a new connection
        private void ServerWorkerThread()
        {
            while (true)
            {
                TcpClient kinectClient = this.serverKinectTCPListener.AcceptTcpClient();
                Thread kinectFrameThread = new Thread(() => this.ServerKinectFrameWorkerThread(kinectClient));
                kinectFrameThread.Start();
            }
        }

        private void ServerKinectFrameWorkerThread(object obj)
        {
            TcpClient client = obj as TcpClient;
            IPEndPoint clientIP = (IPEndPoint)client.Client.RemoteEndPoint;
            NetworkStream clientStream = client.GetStream();
            Debug.WriteLine(KinectMultiTrack.Properties.Resources.CONNECTION_START + clientIP);
            
            bool kinectCameraAdded = false;

            while (true)
            {
                if (!kinectCameraAdded && this.OnAddedKinect != null)
                {
                    Thread fireOnAddKinectCamera = new Thread(() => this.OnAddedKinect(clientIP));
                    fireOnAddKinectCamera.Start();
                    kinectCameraAdded = true;
                }
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    this.Track(clientIP, bodyFrame);

                    // Response content is trivial
                    byte[] response = Encoding.ASCII.GetBytes(Properties.Resources.SERVER_RESPONSE_OKAY);
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Flush();
                }
                catch (Exception)
                {
                    Debug.WriteLine(KinectMultiTrack.Properties.Resources.SERVER_EXCEPTION);
                    clientStream.Close();
                    client.Close();
                }
            }
            if (kinectCameraAdded)
            {
                this.tracker.RemoveClient(clientIP);
                Thread fireOnRemoveKinectCamera = new Thread(() => this.OnRemovedKinect(clientIP));
                fireOnRemoveKinectCamera.Start();
            }
            clientStream.Close();
            clientStream.Dispose();
            client.Close();
        }

        private void Track(IPEndPoint clientIP, SBodyFrame bodyFrame)
        {
            Thread track = new Thread(() => this.tracker.SynchronizeTracking(clientIP, bodyFrame));
            track.Start();
        }

        private void Log(int studyId, int kinectConfiguration, int userScenario, TrackerResult result)
        {
            Thread log = new Thread(() => Logger.SynchronizeLogging(studyId, kinectConfiguration, userScenario, result));
            log.Start();
        }
    }
}
