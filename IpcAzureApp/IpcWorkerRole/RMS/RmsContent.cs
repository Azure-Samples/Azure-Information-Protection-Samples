//
// Copyright © Microsoft Corporation, All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
// ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A
// PARTICULAR PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache License, Version 2.0 for the specific language
// governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.InformationProtectionAndControl;

namespace IpcWorkerRole.RMS
{

    /// <summary>
    /// Protected content (file or a stream) model
    /// </summary>
    internal class RmsContent
    {
        /// <summary>
        /// Constructor for RmsContent
        /// </summary>
        /// <param name="sourceStream">input stream of original file contents</param>
        /// <param name="sinkStream">output stream of protected file contents</param>
        public RmsContent(Stream sourceStream, Stream sinkStream)
        {
            this.SourceStream = sourceStream;
            this.SinkStream = sinkStream;
        }

        public Stream SourceStream { get; private set; }

        public Stream SinkStream { get; set; }

        public string OriginalFileNameWithExtension { get; set; }

        public string PublishedFileNameWithExtension { get; set; }

        public RmsContentState RmsContentState { get; set; }

        public string RmsTemplateId { get; set; }
    }


    internal enum RmsContentState
    {
        Original = 0,
        Protected
    }
}
