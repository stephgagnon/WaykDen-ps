using System;
using System.Management.Automation;
using WaykDen.Utils;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenGroup")]
    public class GetWaykDenGroup : RestApiCmdlet
    {
        [Parameter(HelpMessage = "Wayk Den group ID.")]
        public string GroupID { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string parameter = this.GroupID;
                string result = this.DenRestAPIController.GetGroups(parameter).Result;

                if (!string.IsNullOrEmpty(result))
                {
                    Group[] groups = { };
                    if (JsonUtils.IsArrayJsonObject(result))
                    {
                        groups = this.DenRestAPIController.DeserializeString<Group[]>(result);
                    }
                    else if (JsonUtils.IsSingleJsonObject(result))
                    {
                        groups = new Group[] { this.DenRestAPIController.DeserializeString<Group>(result) };
                    }

                    foreach (Group group in groups)
                    {
                        this.WriteObject(group.ToGroupObject, true);
                    }
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
