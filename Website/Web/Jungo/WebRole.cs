//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  05/22/2013  EHornbostel     Created

using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            Trace.TraceInformation("Executing rolestart.ps1");

            var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = @"..\setup\rolestart.ps1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };

            var writer = new StreamWriter("psout.txt");

            var process = Process.Start(startInfo);
            process.WaitForExit();
            var stdoutMsg = process.StandardOutput.ReadToEnd();
            writer.Write(stdoutMsg);
            writer.Close();

            Trace.TraceInformation("Executed rolestart.ps1");

            return base.OnStart();
        }
    }
}