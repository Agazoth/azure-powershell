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
using System.CodeDom;
using System.IO;
using System.Management.Automation;
using Hyak.Common;
using Microsoft.Azure.Commands.DataLakeAnalytics.Models;
using Microsoft.Azure.Management.DataLake.AnalyticsJob.Models;

namespace Microsoft.Azure.Commands.DataLakeAnalytics
{
    [Cmdlet(VerbsLifecycle.Submit, "AzureDataLakeAnalyticsJob"), OutputType(typeof(JobInformation))]
    public class SubmitAzureDataLakeAnalyticsJob : DataLakeAnalyticsCmdletBase
    {
        // internal const string HiveJobWithScriptPath = "Submit job with script path for Hive";
        internal const string USqlJobWithScriptPath = "Submit job with script path for SQL-IP";
        internal const string USqlJobParameterSetName = "Submit SQL-IP Job";
        // internal const string HiveJobParameterSetName = "Submit Hive Job";

        // TODO: Remove this once hive jobs are enabled
        private SwitchParameter sqlip = true;

        private int _degreeOfParallelism = 1;
        private int _priority = 1000;

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 0, Mandatory = true, HelpMessage = "Name of bigAnalytics account under which the job will be submitted.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobWithScriptPath, Mandatory = true, HelpMessage = "Name of bigAnalytics account under which the job will be submitted.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 0, Mandatory = true, HelpMessage = "Name of bigAnalytics account under which the job will be submitted.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobParameterSetName, Mandatory = true, HelpMessage = "Name of bigAnalytics account under which the job will be submitted.")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 1, Mandatory = true, HelpMessage = "The friendly name of the job to submit.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobWithScriptPath, Mandatory = true, HelpMessage = "The friendly name of the job to submit.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 1, Mandatory = true, HelpMessage = "The friendly name of the job to submit.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobParameterSetName, Mandatory = true, HelpMessage = "The friendly name of the job to submit.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobWithScriptPath, Mandatory = true, HelpMessage = "Path to the script file to submit.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 2, Mandatory = true, HelpMessage = "Path to the script file to submit.")]
        [ValidateNotNullOrEmpty]
        public string ScriptPath { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, Position = 2, ParameterSetName = USqlJobParameterSetName, Mandatory = true, HelpMessage = "Script to execute (written inline).")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = HiveJobParameterSetName, Mandatory = true, HelpMessage = "Script to execute (written inline).")]
        [ValidateNotNullOrEmpty]
        public string Script { get; set; }

        // TODO: Uncomment this out when hive is enabled
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Mandatory = true, HelpMessage = "Indicates that a SQL-IP job is being submitted.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Mandatory = true, HelpMessage = "Indicates that a SQL-IP job is being submitted.")]
        public SwitchParameter USql
        {
            get { return sqlip; }
            set { sqlip = value; }
        }

        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobWithScriptPath, Mandatory = true, HelpMessage = "Indicates that a Hive job is being submitted.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobParameterSetName, Mandatory = true, HelpMessage = "Indicates that a Hive job is being submitted.")]
        public SwitchParameter Hive { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 3, Mandatory = false, HelpMessage = "Optionally set the version of the runtime to use for the job. If left unset, the default runtime is used.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 3, Mandatory = false, HelpMessage = "Optionally set the version of the runtime to use for the job. If left unset, the default runtime is used.")]
        [ValidateNotNullOrEmpty]
        public string Runtime { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 4, Mandatory = false, HelpMessage = "The type of compilation to be done on this job. Valid values are: 'Semantic' (Only erforms semantic checks and necessary sanity checks), 'Full' (full compilation) and 'SingleBox' (Full compilation performed locally).")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 4, Mandatory = false, HelpMessage = "The type of compilation to be done on this job. Valid values are: 'Semantic' (Only erforms semantic checks and necessary sanity checks), 'Full' (full compilation) and 'SingleBox' (Full compilation performed locally)")]
        [ValidateSet("Semantic", "Full", "SingleBox")]
        public string CompileMode { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 5, Mandatory = false, HelpMessage = "Indicates that the submission should only build the job and not execute if set to true.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 5, Mandatory = false, HelpMessage = "Indicates that the submission should only build the job and not execute if set to true.")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter CompileOnly { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 6, Mandatory = false, HelpMessage = "The degree of parallelism to use for this job. Typically, a higher degree of parallelism dedicated to a script results in faster script execution time. Valid range is between 1 and 50, inclusive.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 6, Mandatory = false, HelpMessage = "The degree of parallelism to use for this job. Typically, a higher degree of parallelism dedicated to a script results in faster script execution time. Valid range is between 1 and 50, inclusive.")]
        [ValidateRange(1,50)]
        public int DegreeOfParallelism
        {
            get
            {
                return _degreeOfParallelism;
            }
            set
            {
                _degreeOfParallelism = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 7, Mandatory = false, HelpMessage = "The priority for this job with a range from 1 to 1000, where 1000 is the lowest priority and 1 is the highest.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 7, Mandatory = false, HelpMessage = "The priority for this job with a range from 1 to 1000, where 1000 is the lowest priority and 1 is the highest.")]
        [ValidateRange(1, int.MaxValue)]
        public int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobWithScriptPath, Position = 8, Mandatory = false, HelpMessage = "Name of resource group under which the job will be submitted.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobWithScriptPath, Mandatory = false, HelpMessage = "Name of resource group under which the job will be submitted.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = USqlJobParameterSetName, Position = 8, Mandatory = false, HelpMessage = "Name of resource group under which the job will be submitted.")]
        // [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = HiveJobParameterSetName, Mandatory = false, HelpMessage = "Name of resource group under which the job will be submitted.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        protected override void ProcessRecord()
        {
            // error handling for not passing or passing both script and script path
            if ((string.IsNullOrEmpty(Script) && string.IsNullOrEmpty(ScriptPath)) || (!string.IsNullOrEmpty(Script) && !string.IsNullOrEmpty(ScriptPath)))
            {
                throw new CloudException(Properties.Resources.AmbiguousScriptParameter);
            }

            // get the script
            if(string.IsNullOrEmpty(Script))
            {
                var powerShellDestinationPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(ScriptPath);
                if(!File.Exists(powerShellDestinationPath))
                {
                    throw new CloudException(string.Format(Properties.Resources.ScriptFilePathDoesNotExist, powerShellDestinationPath));
                }

                Script = File.ReadAllText(powerShellDestinationPath);
            }

            string jobType;
            JobProperties properties;
            if(USql)
            {
                jobType = JobType.USql;
                var sqlIpProperties = new USqlProperties
                {
                    Type = jobType,
                    Script = Script
                };

                if(!string.IsNullOrEmpty(CompileMode))
                {
                    sqlIpProperties.CompileMode = CompileMode;
                }

                if(!string.IsNullOrEmpty(Runtime))
                {
                    sqlIpProperties.RuntimeVersion = Runtime;
                }

                properties = sqlIpProperties;

            }
            else if(Hive)
            {
                jobType = JobType.Hive;
                properties = new HiveProperties
                {
                    Script = Script,
                    Type = jobType
                };
            }
            else
            {
                throw new CloudException(Properties.Resources.InvalidJobType);
            }

            var jobInfo = new JobInformation
            {
                JobId = Guid.NewGuid(),
                Name = Name,
                Properties = properties,
                Type = jobType,
                DegreeOfParallelism = DegreeOfParallelism,
                Priority = Priority
            };

            WriteObject(CompileOnly
                ? DataLakeAnalyticsClient.BuildJob(ResourceGroupName, AccountName, jobInfo)
                : DataLakeAnalyticsClient.SubmitJob(ResourceGroupName, AccountName, jobInfo));
        }
    }
}