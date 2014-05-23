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
using System.Threading.Tasks;

namespace DataModel.QueueMessage
{
    /// <summary>
    /// Models a RMS command that is transmitted in serialized format via Azure Queue from web role to worker role
    /// </summary>
    public class RmsCommand
    {
        private const char Delimiter = '/';

        public enum Command
        {
            GetTemplate,
            PublishTemplate
        }

        public Command RmsOperationCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Parameters to command
        /// </summary>
        public object[] Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Makes an instance of RmsCommand from string
        /// </summary>
        /// <param name="stringfiedMessaged"></param>
        public RmsCommand(string stringfiedMessaged)
        {
            //get command
            string remainingString = stringfiedMessaged;
            int firstIndexOfDelimiter = remainingString.IndexOf(Delimiter);
            if (firstIndexOfDelimiter == -1)
            {
                this.RmsOperationCommand = remainingString.ConverToEnum<Command>();
                return;
            }

            this.RmsOperationCommand = remainingString.Substring(0, firstIndexOfDelimiter).ConverToEnum<Command>();
            remainingString = remainingString.Substring(firstIndexOfDelimiter).TrimStart(Delimiter);


            //get parameters
            List<object> parameters = new List<object>();
            while (!string.IsNullOrWhiteSpace(remainingString))
            {
                firstIndexOfDelimiter = remainingString.IndexOf(Delimiter);
                if (firstIndexOfDelimiter == -1)
                {
                    parameters.Add(remainingString);
                    break;
                }
                parameters.Add(remainingString.Substring(0, firstIndexOfDelimiter));
                remainingString = remainingString.Substring(firstIndexOfDelimiter).TrimStart(Delimiter);
            }
            Parameters = new object[parameters.Count];
            parameters.CopyTo(Parameters);
        }

        public RmsCommand(Command command, params object[] parameters)
        {
            RmsOperationCommand = command;
            Parameters = parameters;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(RmsOperationCommand.ConvertToString());

            foreach (object parameter in Parameters)
            {
                stringBuilder.AppendFormat("{0}{1}", Delimiter, parameter.ToString());
            }
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Extension methods for enum to string and vice versa operations
    /// </summary>
    public static class EnumExtensions
    {
        public static string ConvertToString(this Enum en)
        {
            return Enum.GetName(en.GetType(), en);
        }

        public static EnumType ConverToEnum<EnumType>(this String eValue)
        {
            return (EnumType)Enum.Parse(typeof(EnumType), eValue);
        }
    }
}
