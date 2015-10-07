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

using System.Management.Automation;
using Microsoft.Azure.Commands.DataLakeStore.Models;
using Microsoft.Azure.Management.DataLake.StoreFileSystem.Models;

namespace Microsoft.Azure.Commands.DataLakeStore
{
    [Cmdlet(VerbsCommon.Remove, "AzureDataLakeStoreItem"), OutputType(typeof(bool))]
    public class RemoveAzureDataLakeStoreItem : DataLakeStoreFileSystemCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true, HelpMessage = "The DataLakeStore account to execute the filesystem operation in")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = true, HelpMessage = "The path in the specified dataLake account to remove the file or folder. " +
                                                                                           "In the format 'webhdfs://<accountName>.dataLakeaccountdogfood.net/folder/file.txt', " +
                                                                                           "where the first '/' after the DNS indicates the root of the file system.")]
        [ValidateNotNull]
        public DataLakeStorePathInstance[] Paths { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, Mandatory = false, HelpMessage = "Indicates the user wants a recursive delete of the folder.")]
        public SwitchParameter Recurse { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 3, Mandatory = false, HelpMessage = "Indicates the user wants to remove all of the contents of the folder, but not the folder itself")]
        public SwitchParameter Clean { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 4, Mandatory = false, HelpMessage = "Indicates the delete should be immediately performed with no confirmation or prompting. Use carefully.")]
        public SwitchParameter Force { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 5, Mandatory = false, HelpMessage = "Indicates the delete should be immediately performed with no confirmation or prompting. Use carefully.")]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            bool[] success = {true};
            foreach (var path in Paths)
            {
                FileType testClean;
                var pathExists = DataLakeStoreFileSystemClient.TestFileOrFolderExistence(path.Path,
                    AccountName, out testClean);
                
                if (!Force.IsPresent)
                {
                    ConfirmAction(
                        Force.IsPresent,
                        string.Format(Properties.Resources.RemovingDataLakeStoreItem, path.FullyQualifiedPath),
                        string.Format(Properties.Resources.RemoveDataLakeStoreItem, path.FullyQualifiedPath),
                        path.FullyQualifiedPath,
                        () =>
                            success[0] = success[0] && DataLakeStoreFileSystemClient.DeleteFileOrFolder(path.Path, AccountName,
                                Recurse));
                }
                else
                {
                    success[0] = success[0] && DataLakeStoreFileSystemClient.DeleteFileOrFolder(path.Path, AccountName, Recurse);
                }

                if (pathExists && testClean == FileType.Directory && Clean)
                {
                    // recreate the directory as an empty directory if clean was specified.
                    DataLakeStoreFileSystemClient.CreateDirectory(path.Path, AccountName);
                }
            }

            if (PassThru)
            {
                WriteObject(success[0]);
            }
        }
    }
}