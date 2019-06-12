

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace CustomBA
{
    public class ProgressViewModel : PropertyNotifyBase
    {
        private RootViewModel root;
        private Dictionary<string, int> executingPackageOrderIndex;

        private int progressPhases;
        private int progress;
        private int cacheProgress;
        private int executeProgress;
        private string message;

        public ProgressViewModel(RootViewModel root)
        {
            this.root = root;
            this.executingPackageOrderIndex = new Dictionary<string, int>();

            this.root.PropertyChanged += this.RootPropertyChanged;

            WixBA.Model.Bootstrapper.ExecuteMsiMessage += this.ExecuteMsiMessage;
            WixBA.Model.Bootstrapper.ExecuteProgress += this.ApplyExecuteProgress;

            WixBA.Model.Bootstrapper.ExecuteComplete += Bootstrapper_ExecuteComplete;

            WixBA.Model.Bootstrapper.PlanBegin += this.PlanBegin;
            WixBA.Model.Bootstrapper.PlanPackageComplete += this.PlanPackageComplete;
            WixBA.Model.Bootstrapper.ApplyPhaseCount += this.ApplyPhaseCount;
            WixBA.Model.Bootstrapper.Progress += this.ApplyProgress;
            WixBA.Model.Bootstrapper.CacheAcquireProgress += this.CacheAcquireProgress;
            WixBA.Model.Bootstrapper.CacheComplete += this.CacheComplete;
        }

        private void Bootstrapper_ExecuteComplete(object sender, ExecuteCompleteEventArgs e)
        {
            if (Hresult.Succeeded(e.Status))
            {
                Message = "Complete successful!";
            }
            else
            {
                Message = "Complete failed!";
            }
        }

        public bool ProgressEnabled
        {
            get { return this.root.InstallState == InstallationState.Applying; }
        }

        public int Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                if (this.progress != value)
                {
                    this.progress = value;
                    base.OnPropertyChanged("Progress");
                }
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message != value)
                {
                    this.message = value;
                    base.OnPropertyChanged("Message");
                }
            }
        }

        void RootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("InstallState" == e.PropertyName)
            {
                base.OnPropertyChanged("ProgressEnabled");
            }
        }

        private void PlanBegin(object sender, PlanBeginEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"PlanBegin: PackageCount= {e.PackageCount}, Result = {e.Result}");

            lock (this)
            {
                this.executingPackageOrderIndex.Clear();
            }
        }

        private void PlanPackageComplete(object sender, PlanPackageCompleteEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"PlanPackageComplete: PackageId= {e.PackageId}, Requested = {e.Requested}, Execute = {e.Execute}, Rollback = {e.Rollback}, State = {e.State}");

            if (ActionState.None != e.Execute)
            {
                lock (this)
                {
                    Debug.Assert(!this.executingPackageOrderIndex.ContainsKey(e.PackageId));
                    this.executingPackageOrderIndex.Add(e.PackageId, this.executingPackageOrderIndex.Count);
                }
            }
        }

        private void ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"ExecuteMsiMessage: PackageId= {e.PackageId}, e.Message={e.Message}, Data = {string.Join(",",e.Data) }, DisplayParameters = {e.DisplayParameters}, MessageType = {e.MessageType}");

            lock (this)
            {
                if (e.MessageType == InstallMessage.ActionStart)
                {
                    this.Message = string.Format(e.Message, e.Data);
                }

                e.Result = this.root.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void ApplyPhaseCount(object sender, ApplyPhaseCountArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"ApplyPhaseCount: PhaseCount= {e.PhaseCount}");

            this.progressPhases = e.PhaseCount;
        }

        private void ApplyProgress(object sender, ProgressEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"ApplyProgress: OverallPercentage= {e.OverallPercentage},ProgressPercentage= {e.ProgressPercentage},");
            lock (this)
            {
                e.Result = this.root.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"CacheAcquireProgress: PayloadId= {e.PayloadId}, Progress = {e.Progress}, OverallPercentage = {e.OverallPercentage}, Result = {e.Result}");


            lock (this)
            {
                this.cacheProgress = e.OverallPercentage;
                this.Progress = (this.cacheProgress + this.executeProgress) / this.progressPhases;
                e.Result = this.root.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void CacheComplete(object sender, CacheCompleteEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"CacheComplete: Status= {e.Status}");

            lock (this)
            {
                this.cacheProgress = 100;
                this.Progress = (this.cacheProgress + this.executeProgress) / this.progressPhases;
            }
        }

        private void ApplyExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            WixBA.Model.Engine.Log(LogLevel.Verbose, $"ApplyExecuteProgress: PackageId= {e.PackageId}, ProgressPercentage = {e.ProgressPercentage}, OverallPercentage = {e.OverallPercentage}, Result = {e.Result}");

            lock (this)
            {

                this.executeProgress = e.OverallPercentage;
                this.Progress = (this.cacheProgress + this.executeProgress) / this.progressPhases;

                WixBA.Model.Engine.Log(LogLevel.Verbose, $"ApplyExecuteProgress: Progress= {Progress}");


                if (WixBA.Model.Command.Display == Display.Embedded)
                {
                    WixBA.Model.Engine.SendEmbeddedProgress(e.ProgressPercentage, this.Progress);
                }

                e.Result = this.root.Canceled ? Result.Cancel : Result.Ok;
            }
        }
    }
}
