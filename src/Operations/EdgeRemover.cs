﻿using System;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover.Operations
{
    class EdgeRemover : IOperation
    {
        public void PerformTask()
        {
            OperationUtils.RemoveComponentUsingInstallWimTweak("Microsoft-Windows-Internet-Browser");
            UWPAppRemover.RemoveEdgeResidualFiles();
            Console.WriteLine("A system reboot is recommended.");
        }
    }
}
