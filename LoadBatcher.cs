﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace UnitsConverter
{
    [Serializable]
    [Transaction(TransactionMode.Manual)]
    public class BatchConverter : IExternalCommand
    {
        Result IExternalCommand.Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var UIApp = commandData.Application;
            UIApp.DialogBoxShowing += OnDialogShowing;
            UIApp.Application.FailuresProcessing += OnFailuresProcessing;
            var form = new ConverterForm(commandData);
            form.ShowDialog();
            UIApp.Application.FailuresProcessing -= OnFailuresProcessing;
            UIApp.DialogBoxShowing -= OnDialogShowing;
            return Result.Succeeded;
        }

        private static void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var failuresAccessor = e.GetFailuresAccessor();
            IEnumerable<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            foreach (var failureMessage in failureMessages)
                if (failureMessage.GetSeverity() == FailureSeverity.Warning)
                    failuresAccessor.DeleteWarning(failureMessage);
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        private static void OnDialogShowing(object o, DialogBoxShowingEventArgs e)
        {
            if (e.Cancellable) e.Cancel();
            //worry about this later - 1002 = cancel
            if (e.DialogId == "TaskDialog_Unresolved_References") e.OverrideResult(1002);
            //Don't sync newly created files. 1003 = close
            if (e.DialogId == "TaskDialog_Local_Changes_Not_Synchronized_With_Central") e.OverrideResult(1003);
            if (e.DialogId == "TaskDialog_Save_Changes_To_Local_File")
                //Relinquish unmodified elements and worksets
                e.OverrideResult(1001);
        }
    }
}