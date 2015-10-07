﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Management.Automation;
using Microsoft.Azure.Commands.DataLakeAnalytics.Models;

namespace Microsoft.Azure.Commands.DataLakeAnalytics
{
    [Cmdlet(VerbsLifecycle.Stop, "AzureDataLakeAnalyticsJob", SupportsShouldProcess = true)]
    public class StopAzureDataLakeAnalyticsJobInfo : DataLakeAnalyticsCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true, HelpMessage = "Name of the bigAnalytics account name under which want to stop the job.")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, ValueFromPipeline = true, Mandatory = true, HelpMessage = "Name of the specific job to stop.")]
        [ValidateNotNullOrEmpty]
        public Guid JobId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = false, HelpMessage = "Name of resource group under which want to stop the job.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3, Mandatory = false, HelpMessage = "Indicates that the job should be forcibly stopped.")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Force { get; set; }

        [Parameter(Position = 4, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            if (!Force.IsPresent)
            {
                ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.StoppingDataLakeAnalyticsJob, JobId),
                string.Format(Properties.Resources.StopDataLakeAnalyticsJob, JobId),
                JobId.ToString(),
                () => DataLakeAnalyticsClient.CancelJob(ResourceGroupName, AccountName, JobId));
            }
            else
            {
                DataLakeAnalyticsClient.CancelJob(ResourceGroupName, AccountName, JobId);
            }

            if (PassThru)
            {
                WriteObject(true);
            }
        }
    }
}