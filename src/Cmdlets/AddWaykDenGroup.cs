using System;
using System.Management.Automation;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "WaykDenGroup")]
    public class AddWaykDenGroup : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen group name.")]
        public string GroupName { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string data = JsonConvert.SerializeObject(new ByNameObject { name = this.GroupName });
                string post = this.DenRestAPIController.PostCreateGroup(data);
                if (!string.IsNullOrEmpty(post))
                {
                    this.WriteObject(new GroupObject { ID = post, Name = this.GroupName});
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
