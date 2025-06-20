using BepInEx;
using BepInEx.Configuration;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.HotdogSpeech", "Hotdog Speech", "1.0.0")]
    public class HotdogSpeech : BaseUnityPlugin
    {
        public static HotdogSpeech Instance;

        public static ConfigEntry<int> MaximumNumberOfTTSWorkers;
        public static ConfigEntry<float> WorkerTimeout;

        private static string _sharpTalkPath;

        private static readonly Dictionary<Process, TTSWorkerData> _workers = new();

        private static readonly List<TTSWorkRequest> _workRequests = new();

        private const string SHARP_TALK_PATH = "SharpTalk";
        private const string SHARP_TALK_EXE = "Speak.exe";

        public class TTSWorkerData
        {
            // Request Data
            public string Text;
            public TtsVoice Voice = TtsVoice.Paul;
            public ETypeOfWork TypeOfWork;
            public AudioSource AudioSource;
            public Vector3 SoundPosition;
            public Vector2? VolumeRange;
            public Vector2? PitchRange;
            public Vector2? DurationRange;

            // Internal Information
            public Process WorkerProcess;
            public bool IsBusy;
            public bool IsResetting;
            public StreamWriter ConsoleInput;
            public AudioClip AudioClipReceived;
            public string WorkerName;

            // Data Receive
            public int BytesRemaining = int.MaxValue;
            public List<byte[]> ReceivedChunks = new();
            public float TimeSinceLastChunk = 0f;

            public void ClearRequestData()
            {
                Text = string.Empty;
                Voice = TtsVoice.Paul;
                TypeOfWork = ETypeOfWork.None;
                AudioSource = null;
                SoundPosition = Vector3.zero;
                VolumeRange = null;
                PitchRange = null;
                DurationRange = null;

                IsBusy = false;
                AudioClipReceived = null;

                TimeSinceLastChunk = 0f;
                BytesRemaining = int.MaxValue;
                ReceivedChunks.Clear();
            }

            public void SendRequest(TTSWorkRequest workRequest)
            {
                IsBusy = true;
                TypeOfWork = workRequest.TypeOfWork;

                Text = workRequest.Text;
                AudioSource = workRequest.AudioSource;
                Voice = workRequest.Voice;
                SoundPosition = workRequest.SoundPosition;
                VolumeRange = workRequest.VolumeRange;
                PitchRange = workRequest.PitchRange;
                DurationRange = workRequest.DurationRange;

                Instance.Logger.LogInfo($"Sending {Text},{Voice} to worker {WorkerName}!");
                SendDataToTTS();
            }

            private void SendDataToTTS()
            {
                //string textModified = Text + "\n";
                //string voiceModified = Voice + "\n";

                //byte[] textData = Encoding.UTF8.GetBytes( textModified );
                //byte[] voiceData = Encoding.UTF8.GetBytes( voiceModified );

                //ConsoleInput.BaseStream.Write(textData, 0, textData.Length);
                //ConsoleInput.BaseStream.Write(voiceData, 0, voiceData.Length);

                ConsoleInput.WriteLine(Text.Replace("\n", "").Replace("\r",""));
                ConsoleInput.WriteLine(Voice);
            }

            public void ReceiveData(object sender, DataReceivedEventArgs e)
            {
                // Receive data from external executable
                
                if (!string.IsNullOrEmpty(e.Data))
                {
                    string data = e.Data.TrimEnd('\r');
                    if (sender is Process process && _workers.TryGetValue(process, out TTSWorkerData workerData))
                    {
                        if (workerData != this) Instance.Logger.LogError($"Received data from different worker!");
                        //Instance.Logger.LogInfo($"Data for text {workerData.Text} received from worker {workerData.WorkerName}!");
                        // Console.WriteLine($"Data received from worker {workerData.WorkerName}!");

                        if (data.StartsWith("Error:"))
                        {
                            Instance.Logger.LogError(data);
                        }
                        else if (data == "Yes")
                        {
                            Instance.Logger.LogError($"{workerData.WorkerName} still alive but didn't send all data!");
                        }
                        else if (int.TryParse(data, out int bytesRemaining))
                        {
                            if (bytesRemaining > workerData.BytesRemaining)
                            {
                                Instance.Logger.LogError($"Lost chunk receiving audio data from worker {workerData.WorkerName}");
                            }
                            workerData.BytesRemaining = bytesRemaining;

                            if (bytesRemaining == 0)
                            {
                                Instance.Logger.LogInfo($"Transmission completed for {workerData.WorkerName}!");
                                workerData.AudioClipReceived = ConvertChunksToAudioClip(workerData.ReceivedChunks);
                                Instance.StartCoroutine(ProcessFinishedWorkerData());
                            }
                            else
                            {
                                // Instance.Logger.LogInfo($"{workerData.BytesRemaining} bytes remainining for {workerData.WorkerName}!");
                            }
                        }
                        else
                        {
                            byte[] chunk = Convert.FromBase64String(data);
                            workerData.ReceivedChunks.Add(chunk);
                            // Instance.Logger.LogInfo($"Received {workerData.ReceivedChunks.Count} chunk for {workerData.WorkerName}!");

                            workerData.TimeSinceLastChunk = 0f;
                        }
                    }
                    else if (sender is not Process)
                    {
                        Instance.Logger.LogError($"Received message from {sender}, but it is not a process... somehow...");
                    }
                    else if (!_workers.ContainsKey(sender as Process))
                    {
                        Instance.Logger.LogError($"Process {sender} not registered as active worker but sent results!!!");

                        Console.WriteLine(data);
                    }
                }
                else
                {
                    Instance.Logger.LogWarning($"Received empty message from {sender}");
                }
            }



            private void SayWokerDataOnSource()
            {
                AudioSource.clip = AudioClipReceived; ;
                AudioSource.Play();
                ClearRequestData();
            }

            private void SayWorkerDataAsGenericSound()
            {
                AudioEvent audioEvent = new()
                {
                    VolumeRange = VolumeRange ?? new Vector2(0.4f, 0.4f),
                    PitchRange = PitchRange ?? Vector2.one,
                    ClipLengthRange = DurationRange ?? Vector2.one
                };
                audioEvent.Clips.Add(AudioClipReceived);

                SM.PlayGenericSound(audioEvent, SoundPosition);
                ClearRequestData();
            }

            private IEnumerator ProcessFinishedWorkerData()
            {
                yield return null;

                switch (TypeOfWork)
                {
                    case ETypeOfWork.GenericSound:
                        SayWorkerDataAsGenericSound();
                        break;
                    case ETypeOfWork.AudioSource:
                        SayWokerDataOnSource();
                        break;
                    //case ETypeOfWork.AudioClip:
                    //    break;
                    default:
                        Instance.Logger.LogInfo($"Couldn't process worker data from worker {WorkerName}");
                        break;
                }
            }

            public void ResetWorker()
            {
                // ConsoleInput.WriteLine("Still Alive?");
                IsResetting = true;
                WorkerProcess.OutputDataReceived -= ReceiveData;
                WorkerProcess.OutputDataReceived += ReceiveLastDataBit;
                if (!WorkerProcess.HasExited) WorkerProcess.Kill();
                else Instance.Logger.LogWarning($"Worker {WorkerName} crashed prior to reset!");

                Instance.StartCoroutine(WaitForLastBitRetrieval());
            }

            public void ReceiveLastDataBit(object sender, DataReceivedEventArgs e)
            {
                // Receive last bit of data from external executable after forcefully closing process

                if (!string.IsNullOrEmpty(e.Data))
                {
                    string data = e.Data.TrimEnd('\r');

                    byte[] chunk = Convert.FromBase64String(data);
                    ReceivedChunks.Add(chunk);
                    Instance.Logger.LogInfo($"Received {ReceivedChunks.Count} chunk for {WorkerName}! Last chunk retrieval successful!");

                    TimeSinceLastChunk = 0f;

                    AudioClipReceived = ConvertChunksToAudioClip(ReceivedChunks);
                    Instance.StartCoroutine(ProcessFinishedWorkerData());
                }
            }

            public IEnumerator WaitForLastBitRetrieval()
            {
                yield return new WaitForSeconds(1f);
                bool lastBitFailed = false;
                if (IsBusy)
                {
                    Instance.Logger.LogWarning($"Worker {WorkerName} last bit failed to transmit!");
                    lastBitFailed = true;
                }

                _workers.Remove(WorkerProcess);
                TTSWorkRequest workRequest = new(this);

                ClearRequestData();
                WorkerProcess.OutputDataReceived -= ReceiveLastDataBit;
                workRequest.Text += " Test";
                Instance.StartCoroutine(WaitForProcessToCloseAfterReset(workRequest, lastBitFailed));
            }

            private IEnumerator WaitForProcessToCloseAfterReset(TTSWorkRequest workRequest, bool restartRequest)
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = _sharpTalkPath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                WorkerProcess = new()
                {
                    StartInfo = startInfo
                };

                WorkerProcess.Start();
                WorkerProcess.OutputDataReceived += ReceiveData;
                WorkerProcess.BeginOutputReadLine();
                ConsoleInput = WorkerProcess.StandardInput;

                _workers.Add(WorkerProcess, this);

                yield return new WaitForSeconds(0.1f);
                ConsoleInput.WriteLine("Start");
                yield return null;
                if (restartRequest) SendRequest(workRequest);
                else ClearRequestData();
                IsResetting = false;
            }
        }

        public class TTSWorkRequest
        {
            public string Text;
            public TtsVoice Voice = TtsVoice.Paul;
            public ETypeOfWork TypeOfWork;
            public AudioSource AudioSource;
            public Vector3 SoundPosition;
            public Vector2? VolumeRange;
            public Vector2? PitchRange;
            public Vector2? DurationRange;

            public TTSWorkRequest() {}

            public TTSWorkRequest(TTSWorkerData workerData)
            {
                Text = workerData.Text;
                Voice = workerData.Voice;
                TypeOfWork = workerData.TypeOfWork;
                AudioSource = workerData.AudioSource;
                SoundPosition = workerData.SoundPosition;
                VolumeRange = workerData.VolumeRange;
                PitchRange = workerData.PitchRange;
                DurationRange = workerData.DurationRange;
            }
        }

        public enum ETypeOfWork
        {
            None,
            GenericSound,
            AudioSource,
            AudioClip
        }

        public void Awake()
        {
            MaximumNumberOfTTSWorkers = Config.Bind("Hotdog Speech", "Maximum Number of TTS workers", 4);
            WorkerTimeout = Config.Bind("Hotdog Speech", "Worker Timeout", 0.25f, "Time to wait after not receiving the last chunk of data before resetting the worker.");

            _sharpTalkPath = Info.Location;
            _sharpTalkPath = Path.GetDirectoryName(_sharpTalkPath);
            _sharpTalkPath = Path.Combine(_sharpTalkPath, SHARP_TALK_PATH);
            _sharpTalkPath = Path.Combine(_sharpTalkPath, SHARP_TALK_EXE);

            Instance = this;

            StartCoroutine(StartWorkers());

            On.FistVR.FVRPhysicalObject.BeginInteraction += FVRPhysicalObject_BeginInteraction;
            On.FistVR.FVRPhysicalObject.UpdateInteraction += FVRPhysicalObject_UpdateInteraction;
        }

        private void FVRPhysicalObject_UpdateInteraction(On.FistVR.FVRPhysicalObject.orig_UpdateInteraction orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            orig(self, hand);

            float random = UnityEngine.Random.Range(0f, 1f);

            if (random < 0.001f) SayAsGenericSound("Aeiou.", self.transform.position, TtsVoice.Paul, new Vector2(0.2f,0.2f));
            else if (random > 0.999f) SayAsGenericSound("John Madden!", self.transform.position, TtsVoice.Paul, new Vector2(0.2f, 0.2f));
        }

        private void FVRPhysicalObject_BeginInteraction(On.FistVR.FVRPhysicalObject.orig_BeginInteraction orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            orig(self, hand);
            string text;
            switch (self)
            {
                case FVRFireArm:
                    text = $"Do you think you can handle the {self.ObjectWrapper.DisplayName}? It's a {self.ObjectWrapper.TagFirearmRoundPower} {self.ObjectWrapper.TagFirearmAction} weapon!";
                    SayAsGenericSound(text, self.transform.position);
                    break;
                case FVRFireArmMagazine m:
                    text = $"This bad boy can fit so many rounds! {m.m_capacity} rounds, to be exact!";
                    SayAsGenericSound(text, self.transform.position);
                    break;
                default:
                    if (self.ObjectWrapper != null) text = $"I'm not entirely sure what this is. I think it's called a {self.ObjectWrapper.DisplayName}?";
                    else text = $"I'm not entirely sure what this is. I think it's called a {self.gameObject.name.Replace(" (Clone)","")}?";
                    SayAsGenericSound(text, self.transform.position);
                    break;
            }
        }

        private IEnumerator StartWorkers()
        {
            for (int i = 0; i < MaximumNumberOfTTSWorkers.Value; i++)
            {
                StartNewSharpTalkWorker(i);
                Logger.LogInfo("Worker " + i + " started!");
                yield return new WaitForSeconds(1f);
            }
        }

        public void OnDestroy()
        {
            // Close the input stream and wait for the process to exit
            foreach (var worker in _workers)
            {
                worker.Key.Kill();
                worker.Key.Dispose();
            }
        }

        public void Update()
        {
            //List<TTSWorkRequest> completedRequests = new();
            //foreach (var workRequest in _workRequests)
            //{
            //    bool noMoreFreeWorkers = true;
            //    for (int i = 0; i < _workers.Count; i++)
            //    {
            //        TTSWorkerData workerData = _workers.ElementAt(i).Value;
            //        if (!workerData.IsBusy && !workerData.IsResetting)
            //        {
            //            workerData.SendRequest(workRequest);
            //            completedRequests.Add(workRequest);
            //            noMoreFreeWorkers = false;
            //            break;
            //        }
            //    }
            //    if (noMoreFreeWorkers) break;
            //}

            //_workRequests.RemoveAll(r => completedRequests.Contains(r));

            if (_workRequests.Count > 0)
            {
                TTSWorkRequest workRequest = _workRequests.First();
                for (int i = 0; i < _workers.Count; i++)
                {
                    TTSWorkerData workerData = _workers.ElementAt(i).Value;
                    if (!workerData.IsBusy && !workerData.IsResetting)
                    {
                        workerData.SendRequest(workRequest);
                        break;
                    }
                }
                _workRequests.Remove(workRequest);
            }

            List<TTSWorkerData> stuckWorkers = new();
            for (int i = 0; i < _workers.Count; i++)
            {
                TTSWorkerData busyWorker = _workers.ElementAt(i).Value;
                if (busyWorker.IsBusy && !busyWorker.IsResetting)
                {
                    busyWorker.TimeSinceLastChunk += Time.deltaTime;

                    if (busyWorker.TimeSinceLastChunk > WorkerTimeout.Value)
                    {
                        Logger.LogWarning($"Worker {busyWorker.WorkerName} failed to send entire audio! Resetting worker!");
                        stuckWorkers.Add(busyWorker);
                    }
                }
            }

            foreach (var stuckWorker in stuckWorkers)
            {
                stuckWorker.ResetWorker();
            }
        }

        public static void SayOnSource(string text, AudioSource source, TtsVoice voice = TtsVoice.Paul)
        {
            TTSWorkRequest workRequest = new()
            {
                Text = text,
                TypeOfWork = ETypeOfWork.AudioSource,
                Voice = voice,
                AudioSource = source
            };

            _workRequests.Add(workRequest);
        }

        public static void SayAsGenericSound(string text, Vector3 pos, TtsVoice voice = TtsVoice.Paul, Vector2? volumeRange = null, Vector2? pitchRange = null, Vector2? durationRange = null)
        {
            TTSWorkRequest workRequest = new()
            {
                Text = text,
                TypeOfWork = ETypeOfWork.GenericSound,
                Voice = voice,
                SoundPosition = pos,
                VolumeRange = volumeRange,
                PitchRange = pitchRange,
                DurationRange = durationRange
            };

            _workRequests.Add(workRequest);
        }


        private void StartNewSharpTalkWorker(int index)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = _sharpTalkPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process newSharpTalkProcess = new()
            {
                StartInfo = startInfo
            };

            newSharpTalkProcess.Start();
            TTSWorkerData workerData = new()
            {
                WorkerProcess = newSharpTalkProcess,
                ConsoleInput = newSharpTalkProcess.StandardInput,
                WorkerName = "Worker_" + index
            };

            // Read from standard output asynchronously
            newSharpTalkProcess.OutputDataReceived += workerData.ReceiveData;
            newSharpTalkProcess.BeginOutputReadLine();

            _workers.Add(newSharpTalkProcess, workerData);

            StartCoroutine(WaitForProcessStart(newSharpTalkProcess, workerData.ConsoleInput));
        }

        private IEnumerator WaitForProcessStart(Process process, StreamWriter console)
        {
            yield return new WaitForSeconds(1f);

            //string startString = "Start\n";
            //byte[] stringData = Encoding.UTF8.GetBytes(startString);
            //console.BaseStream.Write(stringData, 0, stringData.Length);

            yield return new WaitForSeconds(1f);
            process.StandardInput.WriteLine("Still Alive?");
        }

        private static AudioClip ConvertStringToAudioClip(string audioDataString, string name = "TTS", int sampleRate = 11025)
        {
            byte[] audioDataBytes = Convert.FromBase64String(audioDataString);

            // Convert the byte array to a NativeArray<float>
            NativeArray<float> audioDataFloats = new(audioDataBytes.Length / 2, Allocator.Persistent);

            // Convert 16-bit PCM data to floats (assuming little-endian byte order)
            for (int i = 0; i < audioDataFloats.Length; i++)
            {
                short sample = (short)(audioDataBytes[i * 2] | (audioDataBytes[i * 2 + 1] << 8));
                audioDataFloats[i] = sample / 32768f; // Normalize to range -1.0 to 1.0
            }

            // Create the AudioClip
            AudioClip audioClip = AudioClip.Create(name + "_AudioClip", audioDataFloats.Length, 1, sampleRate, false);
            audioClip.SetData(audioDataFloats.ToArray(), 0);

            // Release the NativeArray memory
            audioDataFloats.Dispose();

            return audioClip;
        }

        private static AudioClip ConvertChunksToAudioClip(List<byte[]> chunks, string name = "TTS", int sampleRate = 11025)
        {
            byte[] audioDataBytes = chunks.SelectMany(bytes => bytes).ToArray();

            // Convert the byte array to a NativeArray<float>
            NativeArray<float> audioDataFloats = new(audioDataBytes.Length / 2, Allocator.Persistent);

            // Convert 16-bit PCM data to floats (assuming little-endian byte order)
            for (int i = 0; i < audioDataFloats.Length; i++)
            {
                short sample = (short)(audioDataBytes[i * 2] | (audioDataBytes[i * 2 + 1] << 8));
                audioDataFloats[i] = sample / 32768f; // Normalize to range -1.0 to 1.0
            }

            // Create the AudioClip
            AudioClip audioClip = AudioClip.Create(name + "_AudioClip", audioDataFloats.Length, 1, sampleRate, false);
            audioClip.SetData(audioDataFloats.ToArray(), 0);

            // Release the NativeArray memory
            audioDataFloats.Dispose();

            return audioClip;
        }

        /// <summary>
        /// Enumerates the available voices for DECtalk.
        /// </summary>
        public enum TtsVoice : uint
        {
            /// <summary>
            /// Default (male) voice.
            /// </summary>
            Paul = 0,
            /// <summary>
            /// Full female voice.
            /// </summary>
            Betty = 1,
            /// <summary>
            /// Full male voice.
            /// </summary>
            Harry = 2,
            /// <summary>
            /// Aged male voice.
            /// </summary>
            Frank = 3,
            /// <summary>
            /// Male voice.
            /// </summary>
            Dennis = 4,
            /// <summary>
            /// Child's voice.
            /// </summary>
            Kit = 5,
            /// <summary>
            /// Aged female voice.
            /// </summary>
            Ursula = 6,
            /// <summary>
            /// Female voice.
            /// </summary>
            Rita = 7,
            /// <summary>
            /// Whispering female voice.
            /// </summary>
            Wendy = 8
        }

#if !DEBUG
#endif
    }
}
