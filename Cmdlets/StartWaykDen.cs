using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Start", "WaykDen")]
    public class StartWaykDen : WaykDenConfigCmdlet
    {
        private Exception error;
        private DenServicesController denServicesController;
        private bool started = false;
        protected override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.Path, this.DenConfigController);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;
                Task<bool> start = this.denServicesController.StartWaykDen();
                this.started = true;

                while(!start.IsCompleted && !start.IsCanceled)
                {
                    this.mre.WaitOne();
                    lock(this.mutex)
                    {
                        if(this.record != null)
                        {
                            this.WriteProgress(this.record);
                            this.record = null;
                        }

                        if(this.error != null)
                        {
                            this.WriteError(new ErrorRecord(this.error, this.error.StackTrace, ErrorCategory.InvalidData, this.error.Data));
                            this.error = null;
                        }
                    }
                }

                Platforms p = string.Equals(this.denServicesController.DenConfig.DenDockerConfigObject.Platform, "Linux") ? Platforms.Linux : Platforms.Windows;
                this.CheckOverrideDockerImages(p, this.denServicesController.DenConfig.DenImageConfigObject);

            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        protected override void OnLog(string message)
        {
            ProgressRecord r = new ProgressRecord(1, "WaykDen", message);
            r.PercentComplete = this.denServicesController.RunningDenServices.Count * 100 / 6;

            lock(this.mutex)
            {
                this.record = r;
                this.mre.Set();
            }
        }

        protected override void OnError(Exception e)
        {
            this.error = e;

            if(!started)
            {
                this.WriteError(new ErrorRecord(e, e.StackTrace, ErrorCategory.InvalidData, e.Data));
            }

            lock(this.mutex)
            {
                this.mre.Set();
            }
        }

        private void CheckOverrideDockerImages(Platforms platform, DenImageConfigObject denImageConfig)
        {
            string originalLucid = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenLucidImage : DenImageConfigObject.WindowsDenLucidImage;
            string originalMongo = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenMongoImage : DenImageConfigObject.WindowsDenMongoImage;
            string originalPicky = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenPickyImage : DenImageConfigObject.WindowsDenPickyImage;
            string originalRouter = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenRouterImage : DenImageConfigObject.WindowsDenRouterImage;
            string originalServer = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenServerImage : DenImageConfigObject.WindowsDenServerImage;
            string originalTraefik = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenTraefikImage : DenImageConfigObject.WindowsDenTraefikImage;
            string originalJet = platform == Platforms.Linux ? DenImageConfigObject.LinuxDevolutionsJetImage : DenImageConfigObject.WindowsDevolutionsJetImage;

            if (denImageConfig.DenLucidImage != originalLucid)
            {
                ShowDockerImageIsOverride(denImageConfig.DenLucidImage, originalLucid);
            }
            if (denImageConfig.DenMongoImage != originalMongo)
            {
                ShowDockerImageIsOverride(denImageConfig.DenMongoImage, originalMongo);
            }
            if (denImageConfig.DenPickyImage != originalPicky)
            {
                ShowDockerImageIsOverride(denImageConfig.DenPickyImage, originalPicky);
            }
            if (denImageConfig.DenRouterImage != originalRouter)
            {
                ShowDockerImageIsOverride(denImageConfig.DenRouterImage, originalRouter);
            }
            if (denImageConfig.DenServerImage != originalServer)
            {
                ShowDockerImageIsOverride(denImageConfig.DenServerImage, originalServer);
            }
            if (denImageConfig.DenTraefikImage != originalTraefik)
            {
                ShowDockerImageIsOverride(denImageConfig.DenTraefikImage, originalTraefik);
            }
            if (denImageConfig.DevolutionsJetImage != originalJet)
            {
                ShowDockerImageIsOverride(denImageConfig.DevolutionsJetImage, originalJet);
            }
        }

        private void ShowDockerImageIsOverride(string stringOverride, string original)
        {
            this.WriteWarning($@"The original docker image: '{original}' is overridden by '{stringOverride}'");
        }
    }
}