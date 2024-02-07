using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace UnitsConverter
{
    public partial class ConverterForm : Form
    {
        private readonly ExternalCommandData cmdData;
        private readonly ArrayList fileTypes = new ArrayList();
        private StreamWriter writer;
        private readonly IList<FileInfo> files = new List<FileInfo>();
        private readonly IList<string> failures = new List<string>();
        private int success;
        private int failed;

        // Variable to store if user cancels the process

        private bool cancelled;
        private bool addInfo;

        // Container for previous opened document

        private UIDocument previousDocument;

        public ConverterForm(ExternalCommandData commandData)
        {
            InitializeComponent();

            // Keep a local copy of the command data

            cmdData = commandData;

            previousDocument = null;
        }

        // Handler for Source folder browse button
        private void btnSource_Click(object sender, EventArgs e)
        {
            // Open the folder browser dialog

            var dlg = new FolderBrowserDialog();

            // Disable New Folder button since it is source location

            dlg.ShowNewFolderButton = false;

            // Provide description 

            dlg.Description = "Select the Source folder :";

            // Show the folder browse dialog

            dlg.ShowDialog();

            // Populate the source path text box

            txtSrcPath.Text = dlg.SelectedPath;
        }

        // Handler for the Destination folder browse button
        private void btnDestination_Click(object sender, EventArgs e)
        {
            // Open the folder browser dialog

            var dlgDest = new FolderBrowserDialog();

            // Enable the New folder button since users should have
            // ability to create destination folder incase it did 
            // not pre-exist

            dlgDest.ShowNewFolderButton = true;

            // Provide description

            dlgDest.Description = "Select the Destination folder : ";

            // Show the folder browse dialog

            dlgDest.ShowDialog();

            // Populate the destination path text box

            txtDestPath.Text = dlgDest.SelectedPath;
        }

        // Handler for the Cancel button
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Set the cancelled variable to true

            cancelled = true;
            Close();
        }

        public void TraverseAll(DirectoryInfo source,
            DirectoryInfo target)
        {
            try
            {
                // Check for user input events

                Application.DoEvents();

                // If destination directory does not exist, 
                // create new directory

                if (!Directory.Exists(target.FullName)) Directory.CreateDirectory(target.FullName);

                foreach (var fi in source.GetFiles())
                {
                    // Check for user input events

                    Application.DoEvents();
                    if (!cancelled)
                    {
                        var sec =
                            fi.GetAccessControl();
                        if (!sec.AreAccessRulesProtected)
                        {
                            // Proceed only if it is not a back up file

                            if (IsNotBackupFile(fi))
                            {
                                // Check if the file already exists, if not proceed

                                if (!AlreadyExists(target, fi))
                                {
                                    // The method contains the code to upgrade the file

                                    Upgrade(fi, target.FullName);
                                }
                                else
                                {
                                    // Print that the file already exists

                                    var msg = " already exists!";
                                    writer.WriteLine("------------------------------");
                                    writer.WriteLine("Error: "
                                                     + target.FullName + "\\" + fi.Name + " " + msg);
                                    writer.WriteLine("------------------------------");
                                    writer.Flush();

                                    lstBxUpdates.Items.Add(
                                        "-------------------------------");
                                    lstBxUpdates.Items.Add("Error: "
                                                           + target.FullName + "\\" + fi.Name + " " + msg);
                                    lstBxUpdates.Items.Add(
                                        "-------------------------------");
                                    lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;
                                }
                            }
                        }
                        else
                        {
                            var msg = " is not accessible or read-only!";
                            writer.WriteLine("-------------------------------");
                            writer.WriteLine("Error: " + fi.FullName + msg);
                            writer.WriteLine("-------------------------------");
                            writer.Flush();

                            lstBxUpdates.Items.Add(
                                "------------------------------");
                            lstBxUpdates.Items.Add("Error: " + fi.FullName + msg);
                            lstBxUpdates.Items.Add(
                                "------------------------------");
                            lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;
                        }
                    }
                }

                // Check for user input events

                Application.DoEvents();

                // RFT resave creates backup files 
                // Delete these backup files created
                foreach (var backupFile in target.GetFiles())
                    if (!IsNotBackupFile(backupFile))
                        File.Delete(backupFile.FullName);

                // Using recursion to work with sub-directories

                foreach (var sourceSubDir in
                         source.GetDirectories())
                {
                    var nextTargetSubDir =
                        target.CreateSubdirectory(sourceSubDir.Name);
                    TraverseAll(sourceSubDir, nextTargetSubDir);

                    // Delete the empty folders - this is created when
                    // none of the files in them meet our upgrade criteria

                    if (nextTargetSubDir.GetFiles().Count() == 0 &&
                        nextTargetSubDir.GetDirectories().Count() == 0)
                        Directory.Delete(nextTargetSubDir.FullName);
                }
            }
            catch
            {
            }
        }

        // Helper method to check if file already exists in target folder
        private bool AlreadyExists(DirectoryInfo target, FileInfo file)
        {
            foreach (var infoTarget in target.GetFiles())
                if (infoTarget.Name.Equals(file.Name))
                    return true;
            return false;
        }

        // Helps determine if the source file is back up file or not
        // Backup files are determined by the format : 
        // <project_name>.<nnnn>.rvt
        // This utility ignores backup files
        private bool IsNotBackupFile(FileInfo rootFile)
        {
            // Check if the file is a backup file

            if (rootFile.Name.Length < 9) return true;

            if (rootFile.Name.Substring(rootFile.Name.Length - 9)
                    .Length > 0)
            {
                var backUpFileName = rootFile.Name.Substring(
                    rootFile.Name.Length - 9);
                long result = 0;

                // Check each char in the file name if it follows 
                // the back up file naming convention

                if (
                    backUpFileName[0].ToString().Equals(".")
                    && long.TryParse(backUpFileName[1].ToString(), out result)
                    && long.TryParse(backUpFileName[2].ToString(), out result)
                    && long.TryParse(backUpFileName[3].ToString(), out result)
                    && long.TryParse(backUpFileName[4].ToString(), out result)
                )
                    return false;
            }

            return true;
        }

        // Searches the directory and creates an internal list of files
        // to be upgraded
        private void SearchDir(DirectoryInfo sDir, bool first)
        {
            try
            {
                // If at root level, true for first call to this method

                if (first)
                    foreach (var rootFile in sDir.GetFiles())
                        // Create internal list of files to be upgraded
                        // This will help with Progress bar
                        // Proceed only if it is not a back up file
                        if (IsNotBackupFile(rootFile))
                            // Keep adding files to the internal list of files
                            if (fileTypes.Contains(rootFile.Extension)
                                || rootFile.Extension.Equals(".txt"))
                            {
                                if (rootFile.Extension.Equals(".txt"))
                                {
                                    if (fileTypes.Contains(".rfa"))
                                        foreach (var rft in sDir.GetFiles())
                                            if (
                                                rft.Name.Remove(rft.Name.Length - 4, 4)
                                                    .Equals(
                                                        rootFile.Name.Remove(
                                                            rootFile.Name.Length - 4, 4)
                                                    ) &&
                                                !rft.Extension.Equals(rootFile.Extension)
                                            )
                                            {
                                                files.Add(rootFile);
                                                break;
                                            }
                                }
                                else
                                {
                                    files.Add(rootFile);
                                }
                            }

                // Get access to each sub-directory in the root directory

                foreach (var direct in sDir.GetDirectories())
                {
                    var sec =
                        direct.GetAccessControl();
                    if (!sec.AreAccessRulesProtected)
                    {
                        foreach (var fInfo in direct.GetFiles())
                            // Proceed only if it is not a back up file
                            if (IsNotBackupFile(fInfo))
                                // Keep adding files to the internal list of files
                                if (fileTypes.Contains(fInfo.Extension)
                                    || fInfo.Extension.Equals(".txt"))
                                {
                                    if (fInfo.Extension.Equals(".txt"))
                                    {
                                        if (fileTypes.Contains(".rfa"))
                                            foreach (var rft in direct.GetFiles())
                                                if (
                                                    rft.Name.Remove(
                                                        rft.Name.Length - 4, 4).Equals(
                                                        fInfo.Name.Remove(fInfo.Name.Length - 4, 4)
                                                    )
                                                    && !rft.Extension.Equals(fInfo.Extension)
                                                )
                                                {
                                                    files.Add(fInfo);
                                                    break;
                                                }
                                    }
                                    else
                                    {
                                        files.Add(fInfo);
                                    }
                                }

                        // Use recursion to drill down further into 
                        // directory structure

                        SearchDir(direct, false);
                    }
                    else
                    {
                        var msg = " is not accessible or read-only!";
                        writer.WriteLine("------------------------------------");
                        writer.WriteLine("Error: " + direct.FullName + msg);
                        writer.WriteLine("------------------------------------");
                        writer.Flush();

                        lstBxUpdates.Items.Add("------------------------------");
                        lstBxUpdates.Items.Add("Error: " + direct.FullName + msg);
                        lstBxUpdates.Items.Add("------------------------------");
                        lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;
                    }
                }
            }
            catch (Exception excpt)
            {
                writer.WriteLine("-------------------------------------");
                writer.WriteLine("Error :" + excpt.Message);
                writer.WriteLine("-------------------------------------");
                writer.Flush();
            }
        }

        // Handler code for the Upgrade button click event
        private void btnUpgrade_Click(object sender, EventArgs e)
        {
            // Initialize the count for success and failed files

            success = 0;
            failed = 0;
            fileTypes.Clear();


            // add rfa files only

            fileTypes.Add(".rfa");

            // Error handling with file types

            if (fileTypes.Count == 0)
            {
                TaskDialog.Show("No File Types",
                    "Please select at least one file type!");
                return;
            }

            // Ensure all path information is filled in 

            if (txtSrcPath.Text.Length > 0
                && txtDestPath.Text.Length > 0)
            {
                // Perform checks to see if all the paths are valid

                var dir = new DirectoryInfo(txtSrcPath.Text);
                var dirDest = new DirectoryInfo(txtDestPath.Text);

                if (!dir.Exists)
                {
                    txtSrcPath.Text = string.Empty;
                    return;
                }

                if (!dirDest.Exists)
                {
                    txtDestPath.Text = string.Empty;
                    return;
                }

                // Ensure destination folder is not inside the source folder
                var dirs = from nestedDirs in dir.EnumerateDirectories("*")
                    where dirDest.FullName.Contains(nestedDirs.FullName)
                    select nestedDirs;
                if (dirs.Count() > 0)
                {
                    TaskDialog.Show(
                        "Abort Conversion",
                        "Selected Destination folder, " + dirDest.Name +
                        ", is contained in the Source folder. Please select a" +
                        " Destination folder outside the Source folder.");
                    txtDestPath.Text = string.Empty;
                    return;
                }

                // If paths are valid
                // Create log and initialize it

                writer = File.CreateText(
                    txtDestPath.Text + "\\" + "ConversionLog.txt"
                );

                // Clear list box 

                lstBxUpdates.Items.Clear();
                files.Clear();

                // Progress bar initialization

                bar.Minimum = 1;

                // Search the directory and create the 
                // list of files to be upgraded

                SearchDir(dir, true);

                // Set Progress bar base values for progression

                bar.Maximum = files.Count;
                bar.Value = 1;
                bar.Step = 1;

                // Traverse through source directory and upgrade
                // files which match the type criteria

                TraverseAll(
                    new DirectoryInfo(txtSrcPath.Text),
                    new DirectoryInfo(txtDestPath.Text));

                // In case no files were found to match 
                // the required criteria

                if (failed.Equals(0) && success.Equals(0))
                {
                    var msg = "No relevant files found for conversion!";
                    TaskDialog.Show("Incomplete", msg);
                    writer.WriteLine(msg);
                    writer.Flush();
                }
                else
                {
                    if (failures.Count > 0)
                    {
                        var msg = "-------------"
                                  + "List of files that "
                                  + "failed to be upgraded"
                                  + "--------------------";

                        // Log failed files information

                        writer.WriteLine("\n");
                        writer.WriteLine(msg);
                        writer.WriteLine("\n");
                        writer.Flush();

                        // Display the failed files information

                        lstBxUpdates.Items.Add("\n");
                        lstBxUpdates.Items.Add(msg);
                        lstBxUpdates.Items.Add("\n");
                        lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;
                        foreach (var str in failures)
                        {
                            writer.WriteLine(str);
                            lstBxUpdates.Items.Add("\n" + str);
                            lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;
                        }

                        failures.Clear();
                        writer.Flush();
                    }

                    // Display final completion dialog 
                    // with success rate

                    TaskDialog.Show("Completed",
                        success + "/" + (success + failed)
                        + " files have been successfully upgraded! "
                        + "\n\nA log file has been created at :\n"
                        + txtDestPath.Text);
                }
                // Reset the Progress bar

                bar.Value = 1;

                // Close the Writer object

                writer.Close();
            }
        }

        // Method which upgrades each file
        private void Upgrade(FileInfo file, string destPath)
        {
            addInfo = false;

            // Check if file type is what is expected to be upgraded
            // or is a text file which is for files which contain 
            // type information for certain family files

            if (fileTypes.Contains(file.Extension)
                || file.Extension.Equals(".txt"))
                try
                {
                    // If it is a text file
                    if (file.Extension.Equals(".txt"))
                    {
                        if (fileTypes.Contains(".rfa"))
                        {
                            var copy = false;

                            // Check each file from the list to see 
                            // if the text file has the same name as 
                            // any of the family files or if it is 
                            // just a standalone text file. In case 
                            // of standalone text file, ignore.
                            foreach (var rft in files)
                                if (
                                    rft.Name.Remove(rft.Name.Length - 4, 4).Equals(
                                        file.Name.Remove(file.Name.Length - 4, 4))
                                    && !rft.Extension.Equals(file.Extension)
                                )
                                {
                                    copy = true;
                                    break;
                                }

                            if (copy)
                            {
                                // Copy the text file into target 
                                // destination
                                File.Copy(file.DirectoryName +
                                          "\\" + file.Name, destPath +
                                                            "\\" + file.Name, true);
                                addInfo = true;
                            }
                        }
                    }

                    // For other file types other than text file
                    else
                    {
                        // This is the main function that opens and save  
                        // a given file. 
                        {
                            // Open a Revit file as an active document. 
                            var UIApp = cmdData.Application;
                            var UIDoc = UIApp.OpenAndActivateDocument(file.FullName);

                            var doc = UIDoc.Document;

                            // Try closing the previously opened document after 
                            // another one is opened. We are doing this because we 
                            // cannot explicitely close an active document
                            //  at a moment.  
                            if (previousDocument != null) previousDocument.SaveAndClose();

                            if (radioButtonMetric.Checked) BatchMetric(doc);

                            if (radioButtonImperial.Checked) BatchImperial(doc);

                            // Save the Revit file to the target destination.
                            // Since we are opening a file as an active document, 
                            // it takes care of preview. 
                            var destinationFile = destPath + "\\" + file.Name;
                            doc.SaveAs(destinationFile);

                            // Saving the current document to close it later.   
                            // If we had a method to close an active document, 
                            // we want to close it here. However, since we opened 
                            // it as an active document, we cannot do so.
                            // We'll close it after the next file is opened.
                            previousDocument = UIDoc;

                            // Set variable to know if upgrade 
                            // was successful - for status updates
                            addInfo = true;
                        }
                    }


                    var msgUnits = "";

                    if (radioButtonMetric.Checked)
                        msgUnits = "metric";

                    else
                        msgUnits = "imperial";

                    if (addInfo)
                    {
                        var msg = " has been converted to " + msgUnits;

                        // Log file and user interface updates
                        lstBxUpdates.Items.Add("\n" + file.Name + msg);
                        lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;
                        writer.WriteLine(file.FullName + msg);
                        writer.Flush();
                        bar.PerformStep();
                        ++success;
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(file.FullName
                                 + " could not be upgraded: "
                                 + ex.Message);

                    bar.PerformStep();

                    ++failed;
                }
        }

        private void UpgraderForm_Load(object sender, EventArgs e)
        {
        }

        private void BatchMetric(Document doc)
        {
            //get the units in the document
            var units = doc.GetUnits();

            //UTLength
            var foUTLength = units.GetFormatOptions(SpecTypeId.Length);
            foUTLength.Accuracy = 1;
            foUTLength.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.Length, foUTLength);
            //UTArea
            var foUTArea = units.GetFormatOptions(SpecTypeId.Area);
            foUTArea.Accuracy = 0.01;
            foUTArea.SetUnitTypeId(UnitTypeId.SquareMeters);
            units.SetFormatOptions(SpecTypeId.Area, foUTArea);
            //UTVolume
            var foUTVolume = units.GetFormatOptions(SpecTypeId.Volume);
            foUTVolume.Accuracy = 0.01;
            foUTVolume.SetUnitTypeId(UnitTypeId.CubicMeters);
            units.SetFormatOptions(SpecTypeId.Volume, foUTVolume);
            //UTAngle
            var foUTAngle = units.GetFormatOptions(SpecTypeId.Angle);
            foUTAngle.Accuracy = 0.01;
            foUTAngle.SetUnitTypeId(UnitTypeId.Degrees);
            units.SetFormatOptions(SpecTypeId.Angle, foUTAngle);


            //hvac
            //UTHVACDensity
            var foUTHVACDensity = units.GetFormatOptions(SpecTypeId.HvacDensity);
            foUTHVACDensity.Accuracy = 0.0001;
            foUTHVACDensity.SetUnitTypeId(UnitTypeId.KilogramsPerCubicMeter);
            units.SetFormatOptions(SpecTypeId.HvacDensity, foUTHVACDensity);
            //UTHVACEnergy
            var foUTHVACEnergy = units.GetFormatOptions(SpecTypeId.HvacEnergy);
            foUTHVACEnergy.Accuracy = 1;
            foUTHVACEnergy.SetUnitTypeId(UnitTypeId.Joules);
            units.SetFormatOptions(SpecTypeId.HvacEnergy, foUTHVACEnergy);
            //UTHVACFriction
            var foUTHVACFriction = units.GetFormatOptions(SpecTypeId.HvacFriction);
            foUTHVACFriction.Accuracy = 0.01;
            foUTHVACFriction.SetUnitTypeId(UnitTypeId.PascalsPerMeter);
            units.SetFormatOptions(SpecTypeId.HvacFriction, foUTHVACFriction);
            //UTHVACPower
            var foUTHVACPower = units.GetFormatOptions(SpecTypeId.HvacPower);
            foUTHVACPower.Accuracy = 1;
            foUTHVACPower.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.HvacPower, foUTHVACPower);
            //UTHVACPowerDensity
            var foUTHVACPowerDensity = units.GetFormatOptions(SpecTypeId.HvacPowerDensity);
            foUTHVACPowerDensity.Accuracy = 0.01;
            foUTHVACPowerDensity.SetUnitTypeId(UnitTypeId.WattsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.HvacPowerDensity, foUTHVACPowerDensity);

            //UTHVACPressure
            var foUTHVACPressure = units.GetFormatOptions(SpecTypeId.HvacPressure);
            foUTHVACPressure.Accuracy = 0.1;
            foUTHVACPressure.SetUnitTypeId(UnitTypeId.Pascals);
            units.SetFormatOptions(SpecTypeId.HvacPressure, foUTHVACPressure);
            //UTHVACTemperature
            var foUTHVACTemperature = units.GetFormatOptions(SpecTypeId.HvacTemperature);
            foUTHVACTemperature.Accuracy = 1;
            foUTHVACTemperature.SetUnitTypeId(UnitTypeId.Celsius);
            units.SetFormatOptions(SpecTypeId.HvacTemperature, foUTHVACTemperature);
            //UTHVACVelocity
            var foUTHVACVelocity = units.GetFormatOptions(SpecTypeId.HvacVelocity);
            foUTHVACVelocity.Accuracy = 0.1;
            foUTHVACVelocity.SetUnitTypeId(UnitTypeId.MetersPerSecond);
            units.SetFormatOptions(SpecTypeId.HvacVelocity, foUTHVACVelocity);
            //UTHVACAirFlow
            var foUTHVACAirFlow = units.GetFormatOptions(SpecTypeId.AirFlow);
            foUTHVACAirFlow.Accuracy = 0.1;
            foUTHVACAirFlow.SetUnitTypeId(UnitTypeId.LitersPerSecond);
            units.SetFormatOptions(SpecTypeId.AirFlow, foUTHVACAirFlow);
            //UTHVACDuctSize
            var foUTHVACDuctSize = units.GetFormatOptions(SpecTypeId.DuctSize);
            foUTHVACDuctSize.Accuracy = 1;
            foUTHVACDuctSize.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.DuctSize, foUTHVACDuctSize);
            //UTHVACCrossSection
            var foUTHVACCrossSection = units.GetFormatOptions(SpecTypeId.CrossSection);
            foUTHVACCrossSection.Accuracy = 1;
            foUTHVACCrossSection.SetUnitTypeId(UnitTypeId.SquareMillimeters);
            units.SetFormatOptions(SpecTypeId.CrossSection, foUTHVACCrossSection);
            //UTHVACHeatGain
            var foUTHVACHeatGain = units.GetFormatOptions(SpecTypeId.HeatGain);
            foUTHVACHeatGain.Accuracy = 1;
            foUTHVACHeatGain.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.HeatGain, foUTHVACHeatGain);

            //electrical
            //UTElectricalCurrent
            var foUTElectricalCurrent = units.GetFormatOptions(SpecTypeId.Current);
            foUTElectricalCurrent.Accuracy = 1;
            foUTElectricalCurrent.SetUnitTypeId(UnitTypeId.Amperes);
            units.SetFormatOptions(SpecTypeId.Current, foUTElectricalCurrent);
            //UTElectricalPotential
            var foUTElectricalPotential = units.GetFormatOptions(SpecTypeId.ElectricalPotential);
            foUTElectricalPotential.Accuracy = 1;
            foUTElectricalPotential.SetUnitTypeId(UnitTypeId.Volts);
            units.SetFormatOptions(SpecTypeId.ElectricalPotential, foUTElectricalPotential);
            //UTElectricalFrequency
            var foUTElectricalFrequency = units.GetFormatOptions(SpecTypeId.ElectricalFrequency);
            foUTElectricalFrequency.Accuracy = 1;
            foUTElectricalFrequency.SetUnitTypeId(UnitTypeId.Hertz);
            units.SetFormatOptions(SpecTypeId.ElectricalFrequency, foUTElectricalFrequency);
            //UTElectricalIlluminance
            var foUTElectricalIlluminance = units.GetFormatOptions(SpecTypeId.Illuminance);
            foUTElectricalIlluminance.Accuracy = 1;
            foUTElectricalIlluminance.SetUnitTypeId(UnitTypeId.Lux);
            units.SetFormatOptions(SpecTypeId.Illuminance, foUTElectricalIlluminance);
            //UTElectricalLuminance
            var foUTElectricalLuminance = units.GetFormatOptions(SpecTypeId.Luminance);
            foUTElectricalLuminance.Accuracy = 1;
            foUTElectricalLuminance.SetUnitTypeId(UnitTypeId.CandelasPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.Luminance, foUTElectricalLuminance);
            //UTElectricalLuminousFlux
            var foUTElectricalLuminousFlux = units.GetFormatOptions(SpecTypeId.LuminousFlux);
            foUTElectricalLuminousFlux.Accuracy = 1;
            foUTElectricalLuminousFlux.SetUnitTypeId(UnitTypeId.Lumens);
            units.SetFormatOptions(SpecTypeId.LuminousFlux, foUTElectricalLuminousFlux);
            //UTElectricalLuminousIntensity
            var foUTElectricalLuminousIntensity = units.GetFormatOptions(SpecTypeId.LuminousIntensity);
            foUTElectricalLuminousIntensity.Accuracy = 1;
            foUTElectricalLuminousIntensity.SetUnitTypeId(UnitTypeId.Candelas);
            units.SetFormatOptions(SpecTypeId.LuminousIntensity, foUTElectricalLuminousIntensity);
            //UTElectricalEfficacy
            var foUTElectricalEfficacy = units.GetFormatOptions(SpecTypeId.Efficacy);
            foUTElectricalEfficacy.Accuracy = 1;
            foUTElectricalEfficacy.SetUnitTypeId(UnitTypeId.LumensPerWatt);
            units.SetFormatOptions(SpecTypeId.Efficacy, foUTElectricalEfficacy);
            //UTElectricalWattage
            var foUTElectricalWattage = units.GetFormatOptions(SpecTypeId.Wattage);
            foUTElectricalWattage.Accuracy = 1;
            foUTElectricalWattage.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.Wattage, foUTElectricalWattage);
            //UTColorTemperature
            var foUTColorTemperature = units.GetFormatOptions(SpecTypeId.ColorTemperature);
            foUTColorTemperature.Accuracy = 1;
            foUTColorTemperature.SetUnitTypeId(UnitTypeId.Kelvin);
            units.SetFormatOptions(SpecTypeId.ColorTemperature, foUTColorTemperature);
            //UTElectricalPower
            var foUTElectricalPower = units.GetFormatOptions(SpecTypeId.ElectricalPower);
            foUTElectricalPower.Accuracy = 1;
            foUTElectricalPower.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.ElectricalPower, foUTElectricalPower);


            //piping
            //UTHVACRoughness
            var foUTHVACRoughness = units.GetFormatOptions(SpecTypeId.HvacRoughness);
            foUTHVACRoughness.Accuracy = 0.01;
            foUTHVACRoughness.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.HvacRoughness, foUTHVACRoughness);
            //UTElectricalApparentPower
            var foUTElectricalApparentPower = units.GetFormatOptions(SpecTypeId.ApparentPower);
            foUTElectricalApparentPower.Accuracy = 1;
            foUTElectricalApparentPower.SetUnitTypeId(UnitTypeId.VoltAmperes);
            units.SetFormatOptions(SpecTypeId.ApparentPower, foUTElectricalApparentPower);
            //UTElectricalPowerDensity
            var foUTElectricalPowerDensity = units.GetFormatOptions(SpecTypeId.ElectricalPowerDensity);
            foUTElectricalPowerDensity.Accuracy = 0.01;
            foUTElectricalPowerDensity.SetUnitTypeId(UnitTypeId.WattsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.ElectricalPowerDensity, foUTElectricalPowerDensity);
            //UTPipingDensity
            var foUTPipingDensity = units.GetFormatOptions(SpecTypeId.PipingDensity);
            foUTPipingDensity.Accuracy = 0.0001;
            foUTPipingDensity.SetUnitTypeId(UnitTypeId.KilogramsPerCubicMeter);
            units.SetFormatOptions(SpecTypeId.PipingDensity, foUTPipingDensity);
            //UTPipingFlow
            var foUTPipingFlow = units.GetFormatOptions(SpecTypeId.Flow);
            foUTPipingFlow.Accuracy = 0.1;
            foUTPipingFlow.SetUnitTypeId(UnitTypeId.LitersPerSecond);
            units.SetFormatOptions(SpecTypeId.Flow, foUTPipingFlow);
            //UTPipingFriction
            var foUTPipingFriction = units.GetFormatOptions(SpecTypeId.PipingFriction);
            foUTPipingFriction.Accuracy = 0.01;
            foUTPipingFriction.SetUnitTypeId(UnitTypeId.PascalsPerMeter);
            units.SetFormatOptions(SpecTypeId.PipingFriction, foUTPipingFriction);
            //UTPipingPressure
            var foUTPipingPressure = units.GetFormatOptions(SpecTypeId.PipingPressure);
            foUTPipingPressure.Accuracy = 0.1;
            foUTPipingPressure.SetUnitTypeId(UnitTypeId.Pascals);
            units.SetFormatOptions(SpecTypeId.PipingPressure, foUTPipingPressure);
            //UTPipingTemperature
            var foUTPipingTemperature = units.GetFormatOptions(SpecTypeId.PipingTemperature);
            foUTPipingTemperature.Accuracy = 1;
            foUTPipingTemperature.SetUnitTypeId(UnitTypeId.Celsius);
            units.SetFormatOptions(SpecTypeId.PipingTemperature, foUTPipingTemperature);
            //UTPipingVelocity
            var foUTPipingVelocity = units.GetFormatOptions(SpecTypeId.PipingVelocity);
            foUTPipingVelocity.Accuracy = 0.1;
            foUTPipingVelocity.SetUnitTypeId(UnitTypeId.MetersPerSecond);
            units.SetFormatOptions(SpecTypeId.PipingVelocity, foUTPipingVelocity);
            //UTPipingViscosity
            var foUTPipingViscosity = units.GetFormatOptions(SpecTypeId.PipingViscosity);
            foUTPipingViscosity.Accuracy = 0.1;
            foUTPipingViscosity.SetUnitTypeId(UnitTypeId.PascalSeconds);
            units.SetFormatOptions(SpecTypeId.PipingViscosity, foUTPipingViscosity);
            //UTPipeSize
            var foUTPipeSize = units.GetFormatOptions(SpecTypeId.PipeSize);
            foUTPipeSize.Accuracy = 1;
            foUTPipeSize.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.PipeSize, foUTPipeSize);
            //UTPipingRoughness
            var foUTPipingRoughness = units.GetFormatOptions(SpecTypeId.PipingRoughness);
            foUTPipingRoughness.Accuracy = 0.001;
            foUTPipingRoughness.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.PipingRoughness, foUTPipingRoughness);
            //UTPipingVolume
            var foUTPipingVolume = units.GetFormatOptions(SpecTypeId.PipingVolume);
            foUTPipingVolume.Accuracy = 0.1;
            foUTPipingVolume.SetUnitTypeId(UnitTypeId.Liters);
            units.SetFormatOptions(SpecTypeId.PipingVolume, foUTPipingVolume);
            //UTHVACViscosity
            var foUTHVACViscosity = units.GetFormatOptions(SpecTypeId.HvacViscosity);
            foUTHVACViscosity.Accuracy = 0.1;
            foUTHVACViscosity.SetUnitTypeId(UnitTypeId.PascalSeconds);
            units.SetFormatOptions(SpecTypeId.HvacViscosity, foUTHVACViscosity);


            //thermals

            //UTHVACCoefficientOfHeatTransfer
            var foUTHVACCoefficientOfHeatTransfer = units.GetFormatOptions(SpecTypeId.HeatTransferCoefficient);
            foUTHVACCoefficientOfHeatTransfer.Accuracy = 0.0001;
            foUTHVACCoefficientOfHeatTransfer.SetUnitTypeId(UnitTypeId.WattsPerSquareMeterKelvin);
            units.SetFormatOptions(SpecTypeId.HeatTransferCoefficient, foUTHVACCoefficientOfHeatTransfer);

            //UTHVACThermalResistance
            var foUTHVACThermalResistance = units.GetFormatOptions(SpecTypeId.ThermalResistance);
            foUTHVACThermalResistance.Accuracy = 0.0001;
            foUTHVACThermalResistance.SetUnitTypeId(UnitTypeId.SquareMeterKelvinsPerWatt);
            units.SetFormatOptions(SpecTypeId.ThermalResistance, foUTHVACThermalResistance);

            //UTHVACThermalMass
            var foUTHVACThermalMass = units.GetFormatOptions(SpecTypeId.ThermalMass);
            foUTHVACThermalMass.Accuracy = 0.01;
            foUTHVACThermalMass.SetUnitTypeId(UnitTypeId.KilojoulesPerKelvin);
            units.SetFormatOptions(SpecTypeId.ThermalMass, foUTHVACThermalMass);

            //UTHVACThermalConductivity
            var foUTHVACThermalConductivity = units.GetFormatOptions(SpecTypeId.ThermalConductivity);
            foUTHVACThermalConductivity.Accuracy = 0.0001;
            foUTHVACThermalConductivity.SetUnitTypeId(UnitTypeId.WattsPerMeterKelvin);
            units.SetFormatOptions(SpecTypeId.ThermalConductivity, foUTHVACThermalConductivity);

            //UTHVACSpecificHeat
            var foUTHVACSpecificHeat = units.GetFormatOptions(SpecTypeId.SpecificHeat);
            foUTHVACSpecificHeat.Accuracy = 0.0001;
            foUTHVACSpecificHeat.SetUnitTypeId(UnitTypeId.JoulesPerGramDegreeCelsius);
            units.SetFormatOptions(SpecTypeId.SpecificHeat, foUTHVACSpecificHeat);

            //UTHVACSpecificHeatOfVaporization
            var foUTHVACSpecificHeatOfVaporization =
                units.GetFormatOptions(SpecTypeId.SpecificHeatOfVaporization);
            foUTHVACSpecificHeatOfVaporization.Accuracy = 0.0001;
            foUTHVACSpecificHeatOfVaporization.SetUnitTypeId(UnitTypeId.JoulesPerGram);
            units.SetFormatOptions(SpecTypeId.SpecificHeatOfVaporization, foUTHVACSpecificHeatOfVaporization);

            //UTHVACPermeability
            var foUTHVACPermeability = units.GetFormatOptions(SpecTypeId.Permeability);
            foUTHVACPermeability.Accuracy = 0.0001;
            foUTHVACPermeability.SetUnitTypeId(UnitTypeId.NanogramsPerPascalSecondSquareMeter);
            units.SetFormatOptions(SpecTypeId.Permeability, foUTHVACPermeability);


            //loads and shiz

            //UTElectricalResistivity
            var foUTElectricalResistivity = units.GetFormatOptions(SpecTypeId.ElectricalResistivity);
            foUTElectricalResistivity.Accuracy = 0.0001;
            foUTElectricalResistivity.SetUnitTypeId(UnitTypeId.OhmMeters);
            units.SetFormatOptions(SpecTypeId.ElectricalResistivity, foUTElectricalResistivity);
            //UTHVACAirFlowDensity
            var foUTHVACAirFlowDensity = units.GetFormatOptions(SpecTypeId.AirFlowDensity);
            foUTHVACAirFlowDensity.Accuracy = 0.0001;
            foUTHVACAirFlowDensity.SetUnitTypeId(UnitTypeId.LitersPerSecondSquareMeter);
            units.SetFormatOptions(SpecTypeId.AirFlowDensity, foUTHVACAirFlowDensity);
            //UTSlope
            var foUTSlope = units.GetFormatOptions(SpecTypeId.Slope);
            foUTSlope.Accuracy = 0.01;
            foUTSlope.SetUnitTypeId(UnitTypeId.SlopeDegrees);
            units.SetFormatOptions(SpecTypeId.Slope, foUTSlope);
            //UTHVACCoolingLoad
            var foUTHVACCoolingLoad = units.GetFormatOptions(SpecTypeId.CoolingLoad);
            foUTHVACCoolingLoad.Accuracy = 1;
            foUTHVACCoolingLoad.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.CoolingLoad, foUTHVACCoolingLoad);
            //UTHVACHeatingLoad
            var foUTHVACHeatingLoad = units.GetFormatOptions(SpecTypeId.HeatingLoad);
            foUTHVACHeatingLoad.Accuracy = 1;
            foUTHVACHeatingLoad.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.HeatingLoad, foUTHVACHeatingLoad);
            //UTHVACCoolingLoadDividedByArea
            var foUTHVACCoolingLoadDividedByArea =
                units.GetFormatOptions(SpecTypeId.CoolingLoadDividedByArea);
            foUTHVACCoolingLoadDividedByArea.Accuracy = 0.01;
            foUTHVACCoolingLoadDividedByArea.SetUnitTypeId(UnitTypeId.WattsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.CoolingLoadDividedByArea, foUTHVACCoolingLoadDividedByArea);
            //UTHVACHeatingLoadDividedByArea
            var foUTHVACHeatingLoadDividedByArea =
                units.GetFormatOptions(SpecTypeId.HeatingLoadDividedByArea);
            foUTHVACHeatingLoadDividedByArea.Accuracy = 0.01;
            foUTHVACHeatingLoadDividedByArea.SetUnitTypeId(UnitTypeId.WattsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.HeatingLoadDividedByArea, foUTHVACHeatingLoadDividedByArea);
            //UTHVACCoolingLoadDividedByVolume
            var foUTHVACCoolingLoadDividedByVolume =
                units.GetFormatOptions(SpecTypeId.CoolingLoadDividedByVolume);
            foUTHVACCoolingLoadDividedByVolume.Accuracy = 0.01;
            foUTHVACCoolingLoadDividedByVolume.SetUnitTypeId(UnitTypeId.WattsPerCubicMeter);
            units.SetFormatOptions(SpecTypeId.CoolingLoadDividedByVolume, foUTHVACCoolingLoadDividedByVolume);
            //UTHVACHeatingLoadDividedByVolume
            var foUTHVACHeatingLoadDividedByVolume =
                units.GetFormatOptions(SpecTypeId.HeatingLoadDividedByVolume);
            foUTHVACHeatingLoadDividedByVolume.Accuracy = 0.01;
            foUTHVACHeatingLoadDividedByVolume.SetUnitTypeId(UnitTypeId.WattsPerCubicMeter);
            units.SetFormatOptions(SpecTypeId.HeatingLoadDividedByVolume, foUTHVACHeatingLoadDividedByVolume);
            //UTHVACAirFlowDividedByVolume
            var foUTHVACAirFlowDividedByVolume = units.GetFormatOptions(SpecTypeId.AirFlowDividedByVolume);
            foUTHVACAirFlowDividedByVolume.Accuracy = 0.01;
            foUTHVACAirFlowDividedByVolume.SetUnitTypeId(UnitTypeId.LitersPerSecondCubicMeter);
            units.SetFormatOptions(SpecTypeId.AirFlowDividedByVolume, foUTHVACAirFlowDividedByVolume);
            //UTHVACAirFlowDividedByCoolingLoad
            var foUTHVACAirFlowDividedByCoolingLoad =
                units.GetFormatOptions(SpecTypeId.AirFlowDividedByCoolingLoad);
            foUTHVACAirFlowDividedByCoolingLoad.Accuracy = 0.01;
            foUTHVACAirFlowDividedByCoolingLoad.SetUnitTypeId(UnitTypeId.LitersPerSecondKilowatt);
            units.SetFormatOptions(SpecTypeId.AirFlowDividedByCoolingLoad, foUTHVACAirFlowDividedByCoolingLoad);
            //UTHVACAreaDividedByCoolingLoad
            var foUTHVACAreaDividedByCoolingLoad =
                units.GetFormatOptions(SpecTypeId.AreaDividedByCoolingLoad);
            foUTHVACAreaDividedByCoolingLoad.Accuracy = 0.01;
            foUTHVACAreaDividedByCoolingLoad.SetUnitTypeId(UnitTypeId.SquareMetersPerKilowatt);
            units.SetFormatOptions(SpecTypeId.AreaDividedByCoolingLoad, foUTHVACAreaDividedByCoolingLoad);
            //UTHVACAreaDividedByHeatingLoad
            var foUTHVACAreaDividedByHeatingLoad =
                units.GetFormatOptions(SpecTypeId.AreaDividedByHeatingLoad);
            foUTHVACAreaDividedByHeatingLoad.Accuracy = 0.0001;
            foUTHVACAreaDividedByHeatingLoad.SetUnitTypeId(UnitTypeId.SquareMetersPerKilowatt);
            units.SetFormatOptions(SpecTypeId.AreaDividedByHeatingLoad, foUTHVACAreaDividedByHeatingLoad);


            //pipe insulation, cables and other shiz
            //UTWireSize
            var foUTWireSize = units.GetFormatOptions(SpecTypeId.WireDiameter);
            foUTWireSize.Accuracy = 0.01;
            foUTWireSize.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.WireDiameter, foUTWireSize);
            //UTHVACSlope
            var foUTHVACSlope = units.GetFormatOptions(SpecTypeId.HvacSlope);
            foUTHVACSlope.Accuracy = 0.01;
            foUTHVACSlope.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.HvacSlope, foUTHVACSlope);
            //UTPipingSlope
            var foUTPipingSlope = units.GetFormatOptions(SpecTypeId.PipingSlope);
            foUTPipingSlope.Accuracy = 0.01;
            foUTPipingSlope.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.PipingSlope, foUTPipingSlope);
            //UTCurrency
            var foUTCurrency = units.GetFormatOptions(SpecTypeId.Currency);
            foUTCurrency.Accuracy = 0.01;
            foUTCurrency.SetUnitTypeId(UnitTypeId.Currency);
            units.SetFormatOptions(SpecTypeId.Currency, foUTCurrency);
            //UTMassDensity
            var foUTMassDensity = units.GetFormatOptions(SpecTypeId.MassDensity);
            foUTMassDensity.Accuracy = 0.01;
            foUTMassDensity.SetUnitTypeId(UnitTypeId.KilogramsPerCubicMeter);
            units.SetFormatOptions(SpecTypeId.MassDensity, foUTMassDensity);
            //UTHVACFactor
            var foUTHVACFactor = units.GetFormatOptions(SpecTypeId.Factor);
            foUTHVACFactor.Accuracy = 0.01;
            foUTHVACFactor.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.Factor, foUTHVACFactor);
            //UTElectricalTemperature
            var foUTElectricalTemperature = units.GetFormatOptions(SpecTypeId.ElectricalTemperature);
            foUTElectricalTemperature.Accuracy = 1;
            foUTElectricalTemperature.SetUnitTypeId(UnitTypeId.Celsius);
            units.SetFormatOptions(SpecTypeId.ElectricalTemperature, foUTElectricalTemperature);
            //UTElectricalCableTraySize
            var foUTElectricalCableTraySize = units.GetFormatOptions(SpecTypeId.CableTraySize);
            foUTElectricalCableTraySize.Accuracy = 1;
            foUTElectricalCableTraySize.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.CableTraySize, foUTElectricalCableTraySize);
            //UTElectricalConduitSize
            var foUTElectricalConduitSize = units.GetFormatOptions(SpecTypeId.ConduitSize);
            foUTElectricalConduitSize.Accuracy = 1;
            foUTElectricalConduitSize.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.ConduitSize, foUTElectricalConduitSize);
            //UTElectricalDemandFactor
            var foUTElectricalDemandFactor = units.GetFormatOptions(SpecTypeId.DemandFactor);
            foUTElectricalDemandFactor.Accuracy = 0.01;
            foUTElectricalDemandFactor.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.DemandFactor, foUTElectricalDemandFactor);
            //UTHVACDuctInsulationThickness
            var foUTHVACDuctInsulationThickness = units.GetFormatOptions(SpecTypeId.DuctInsulationThickness);
            foUTHVACDuctInsulationThickness.Accuracy = 1;
            foUTHVACDuctInsulationThickness.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.DuctInsulationThickness, foUTHVACDuctInsulationThickness);
            //UTHVACDuctLiningThickness
            var foUTHVACDuctLiningThickness = units.GetFormatOptions(SpecTypeId.DuctLiningThickness);
            foUTHVACDuctLiningThickness.Accuracy = 1;
            foUTHVACDuctLiningThickness.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.DuctLiningThickness, foUTHVACDuctLiningThickness);
            //UTPipeInsulationThickness
            var foUTPipeInsulationThickness = units.GetFormatOptions(SpecTypeId.PipeInsulationThickness);
            foUTPipeInsulationThickness.Accuracy = 1;
            foUTPipeInsulationThickness.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.PipeInsulationThickness, foUTPipeInsulationThickness);


            //structure
            //UTForce
            var foUTForce = units.GetFormatOptions(SpecTypeId.Force);
            foUTForce.Accuracy = 0.01;
            foUTForce.SetUnitTypeId(UnitTypeId.Kilonewtons);
            units.SetFormatOptions(SpecTypeId.Force, foUTForce);

            //UTLinearForce
            var foUTLinearForce = units.GetFormatOptions(SpecTypeId.LinearForce);
            foUTLinearForce.Accuracy = 0.01;
            foUTLinearForce.SetUnitTypeId(UnitTypeId.KilonewtonsPerMeter);
            units.SetFormatOptions(SpecTypeId.LinearForce, foUTLinearForce);

            //UTAreaForce
            var foUTAreaForce = units.GetFormatOptions(SpecTypeId.AreaForce);
            foUTAreaForce.Accuracy = 0.01;
            foUTAreaForce.SetUnitTypeId(UnitTypeId.KilonewtonsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.AreaForce, foUTAreaForce);
            //UTMoment
            var foUTMoment = units.GetFormatOptions(SpecTypeId.Moment);
            foUTMoment.Accuracy = 0.01;
            foUTMoment.SetUnitTypeId(UnitTypeId.KilonewtonMeters);
            units.SetFormatOptions(SpecTypeId.Moment, foUTMoment);

            //UTLinearMoment
            var foUTLinearMoment = units.GetFormatOptions(SpecTypeId.LinearMoment);
            foUTLinearMoment.Accuracy = 0.01;
            foUTLinearMoment.SetUnitTypeId(UnitTypeId.KilonewtonMetersPerMeter);
            units.SetFormatOptions(SpecTypeId.LinearMoment, foUTLinearMoment);
            //UTStress
            var foUTStress = units.GetFormatOptions(SpecTypeId.Stress);
            foUTStress.Accuracy = 0.1;
            foUTStress.SetUnitTypeId(UnitTypeId.Megapascals);
            units.SetFormatOptions(SpecTypeId.Stress, foUTStress);

            //UTUnitWeight
            var foUTUnitWeight = units.GetFormatOptions(SpecTypeId.UnitWeight);
            foUTUnitWeight.Accuracy = 0.1;
            foUTUnitWeight.SetUnitTypeId(UnitTypeId.KilonewtonsPerCubicMeter);
            units.SetFormatOptions(SpecTypeId.UnitWeight, foUTUnitWeight);
            //UTWeight
            var foUTWeight = units.GetFormatOptions(SpecTypeId.Weight);
            foUTWeight.Accuracy = 0.01;
            foUTWeight.SetUnitTypeId(UnitTypeId.Kilonewtons);
            units.SetFormatOptions(SpecTypeId.Weight, foUTWeight);


            //UTMass
            var foUTMass = units.GetFormatOptions(SpecTypeId.Mass);
            foUTMass.Accuracy = 0.01;
            foUTMass.SetUnitTypeId(UnitTypeId.Kilograms);
            units.SetFormatOptions(SpecTypeId.Mass, foUTMass);
            //UTMassPerUnitArea
            var foUTMassPerUnitArea = units.GetFormatOptions(SpecTypeId.MassPerUnitArea);
            foUTMassPerUnitArea.Accuracy = 0.01;
            foUTMassPerUnitArea.SetUnitTypeId(UnitTypeId.KilogramsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.MassPerUnitArea, foUTMassPerUnitArea);


            //UTThermalExpansion
            var foUTThermalExpansion = units.GetFormatOptions(SpecTypeId.ThermalExpansionCoefficient);
            foUTThermalExpansion.Accuracy = 1E-05;
            foUTThermalExpansion.SetUnitTypeId(UnitTypeId.InverseDegreesCelsius);
            units.SetFormatOptions(SpecTypeId.ThermalExpansionCoefficient, foUTThermalExpansion);
            //UTForcePerLength
            var foUTForcePerLength = units.GetFormatOptions(SpecTypeId.Force);
            foUTForcePerLength.Accuracy = 0.1;
            foUTForcePerLength.SetUnitTypeId(UnitTypeId.Kilonewtons);
            units.SetFormatOptions(SpecTypeId.Force, foUTForcePerLength);

            //UTLinearForcePerLength
            var foUTLinearForcePerLength = units.GetFormatOptions(SpecTypeId.LinearForce);
            foUTLinearForcePerLength.Accuracy = 0.1;
            foUTLinearForcePerLength.SetUnitTypeId(UnitTypeId.KilonewtonsPerMeter);
            units.SetFormatOptions(SpecTypeId.LinearForce, foUTLinearForcePerLength);
            //UTAreaForcePerLength
            var foUTAreaForcePerLength = units.GetFormatOptions(SpecTypeId.AreaForce);
            foUTAreaForcePerLength.Accuracy = 0.1;
            foUTAreaForcePerLength.SetUnitTypeId(UnitTypeId.KilonewtonsPerSquareMeter);
            units.SetFormatOptions(SpecTypeId.AreaForce, foUTAreaForcePerLength);

            //UTDisplacementDeflection
            var foUTDisplacementDeflection = units.GetFormatOptions(SpecTypeId.Displacement);
            foUTDisplacementDeflection.Accuracy = 0.01;
            foUTDisplacementDeflection.SetUnitTypeId(UnitTypeId.Centimeters);
            units.SetFormatOptions(SpecTypeId.Displacement, foUTDisplacementDeflection);
            //UTRotation
            var foUTRotation = units.GetFormatOptions(SpecTypeId.Rotation);
            foUTRotation.Accuracy = 0.001;
            foUTRotation.SetUnitTypeId(UnitTypeId.Radians);
            units.SetFormatOptions(SpecTypeId.Rotation, foUTRotation);

            //UTPeriod
            var foUTPeriod = units.GetFormatOptions(SpecTypeId.Period);
            foUTPeriod.Accuracy = 0.1;
            foUTPeriod.SetUnitTypeId(UnitTypeId.Seconds);
            units.SetFormatOptions(SpecTypeId.Period, foUTPeriod);
            //UTStructuralFrequency
            var foUTStructuralFrequency = units.GetFormatOptions(SpecTypeId.StructuralFrequency);
            foUTStructuralFrequency.Accuracy = 0.1;
            foUTStructuralFrequency.SetUnitTypeId(UnitTypeId.Hertz);
            units.SetFormatOptions(SpecTypeId.StructuralFrequency, foUTStructuralFrequency);

            //UTPulsation
            var foUTPulsation = units.GetFormatOptions(SpecTypeId.Pulsation);
            foUTPulsation.Accuracy = 0.1;
            foUTPulsation.SetUnitTypeId(UnitTypeId.RadiansPerSecond);
            units.SetFormatOptions(SpecTypeId.Pulsation, foUTPulsation);
            //UTStructuralVelocity
            var foUTStructuralVelocity = units.GetFormatOptions(SpecTypeId.StructuralVelocity);
            foUTStructuralVelocity.Accuracy = 0.1;
            foUTStructuralVelocity.SetUnitTypeId(UnitTypeId.MetersPerSecond);
            units.SetFormatOptions(SpecTypeId.StructuralVelocity, foUTStructuralVelocity);
            //UTAcceleration
            var foUTAcceleration = units.GetFormatOptions(SpecTypeId.Acceleration);
            foUTAcceleration.Accuracy = 0.1;
            foUTAcceleration.SetUnitTypeId(UnitTypeId.MetersPerSecondSquared);
            units.SetFormatOptions(SpecTypeId.Acceleration, foUTAcceleration);


            //UTEnergy
            var foUTEnergy = units.GetFormatOptions(SpecTypeId.Energy);
            foUTEnergy.Accuracy = 0.1;
            foUTEnergy.SetUnitTypeId(UnitTypeId.Kilojoules);
            units.SetFormatOptions(SpecTypeId.Energy, foUTEnergy);
            //UTReinforcementVolume
            var foUTReinforcementVolume = units.GetFormatOptions(SpecTypeId.ReinforcementVolume);
            foUTReinforcementVolume.Accuracy = 0.01;
            foUTReinforcementVolume.SetUnitTypeId(UnitTypeId.CubicCentimeters);
            units.SetFormatOptions(SpecTypeId.ReinforcementVolume, foUTReinforcementVolume);


            //UTReinforcementLength
            var foUTReinforcementLength = units.GetFormatOptions(SpecTypeId.ReinforcementLength);
            foUTReinforcementLength.Accuracy = 1;
            foUTReinforcementLength.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.ReinforcementLength, foUTReinforcementLength);
            //UTReinforcementArea
            var foUTReinforcementArea = units.GetFormatOptions(SpecTypeId.ReinforcementArea);
            foUTReinforcementArea.Accuracy = 0.01;
            foUTReinforcementArea.SetUnitTypeId(UnitTypeId.SquareCentimeters);
            units.SetFormatOptions(SpecTypeId.ReinforcementArea, foUTReinforcementArea);


            //UTReinforcementAreaperUnitLength
            var foUTReinforcementAreaperUnitLength =
                units.GetFormatOptions(SpecTypeId.ReinforcementAreaPerUnitLength);
            foUTReinforcementAreaperUnitLength.Accuracy = 0.01;
            foUTReinforcementAreaperUnitLength.SetUnitTypeId(UnitTypeId.SquareCentimetersPerMeter);
            units.SetFormatOptions(SpecTypeId.ReinforcementAreaPerUnitLength, foUTReinforcementAreaperUnitLength);
            //UTReinforcementSpacing
            var foUTReinforcementSpacing = units.GetFormatOptions(SpecTypeId.ReinforcementSpacing);
            foUTReinforcementSpacing.Accuracy = 1;
            foUTReinforcementSpacing.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.ReinforcementSpacing, foUTReinforcementSpacing);


            //UTReinforcementCover
            var foUTReinforcementCover = units.GetFormatOptions(SpecTypeId.ReinforcementCover);
            foUTReinforcementCover.Accuracy = 1;
            foUTReinforcementCover.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.ReinforcementCover, foUTReinforcementCover);
            //UTBarDiameter
            var foUTBarDiameter = units.GetFormatOptions(SpecTypeId.BarDiameter);
            foUTBarDiameter.Accuracy = 1;
            foUTBarDiameter.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.BarDiameter, foUTBarDiameter);


            //UTCrackWidth
            var foUTCrackWidth = units.GetFormatOptions(SpecTypeId.CrackWidth);
            foUTCrackWidth.Accuracy = 0.01;
            foUTCrackWidth.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.CrackWidth, foUTCrackWidth);
            //UTSectionDimension
            var foUTSectionDimension = units.GetFormatOptions(SpecTypeId.SectionDimension);
            foUTSectionDimension.Accuracy = 0.1;
            foUTSectionDimension.SetUnitTypeId(UnitTypeId.Centimeters);
            units.SetFormatOptions(SpecTypeId.SectionDimension, foUTSectionDimension);


            //UTSectionProperty
            var foUTSectionProperty = units.GetFormatOptions(SpecTypeId.SectionProperty);
            foUTSectionProperty.Accuracy = 0.1;
            foUTSectionProperty.SetUnitTypeId(UnitTypeId.Centimeters);
            units.SetFormatOptions(SpecTypeId.SectionProperty, foUTSectionProperty);
            //UTSectionArea
            var foUTSectionArea = units.GetFormatOptions(SpecTypeId.SectionArea);
            foUTSectionArea.Accuracy = 0.1;
            foUTSectionArea.SetUnitTypeId(UnitTypeId.SquareCentimeters);
            units.SetFormatOptions(SpecTypeId.SectionArea, foUTSectionArea);


            //UTSectionModulus
            var foUTSectionModulus = units.GetFormatOptions(SpecTypeId.SectionModulus);
            foUTSectionModulus.Accuracy = 0.1;
            foUTSectionModulus.SetUnitTypeId(UnitTypeId.CubicCentimeters);
            units.SetFormatOptions(SpecTypeId.SectionModulus, foUTSectionModulus);
            //UTMomentofInertia
            var foUTMomentofInertia = units.GetFormatOptions(SpecTypeId.MomentOfInertia);
            foUTMomentofInertia.Accuracy = 0.01;
            foUTMomentofInertia.SetUnitTypeId(UnitTypeId.CentimetersToTheFourthPower);
            units.SetFormatOptions(SpecTypeId.MomentOfInertia, foUTMomentofInertia);


            //UTWarpingConstant
            var foUTWarpingConstant = units.GetFormatOptions(SpecTypeId.WarpingConstant);
            foUTWarpingConstant.Accuracy = 0.1;
            foUTWarpingConstant.SetUnitTypeId(UnitTypeId.CentimetersToTheSixthPower);
            units.SetFormatOptions(SpecTypeId.WarpingConstant, foUTWarpingConstant);
            //UTMassperUnitLength
            var foUTMassperUnitLength = units.GetFormatOptions(SpecTypeId.MassPerUnitLength);
            foUTMassperUnitLength.Accuracy = 0.01;
            foUTMassperUnitLength.SetUnitTypeId(UnitTypeId.KilogramsPerMeter);
            units.SetFormatOptions(SpecTypeId.MassPerUnitLength, foUTMassperUnitLength);

            //UTWeightperUnitLength
            var foUTWeightperUnitLength = units.GetFormatOptions(SpecTypeId.WeightPerUnitLength);
            foUTWeightperUnitLength.Accuracy = 0.01;
            foUTWeightperUnitLength.SetUnitTypeId(UnitTypeId.KilogramsForcePerMeter);
            units.SetFormatOptions(SpecTypeId.WeightPerUnitLength, foUTWeightperUnitLength);


            //UTSurfaceArea
            var foUTSurfaceArea = units.GetFormatOptions(SpecTypeId.SurfaceAreaPerUnitLength);
            foUTSurfaceArea.Accuracy = 0.01;
            foUTSurfaceArea.SetUnitTypeId(UnitTypeId.SquareMetersPerMeter);
            units.SetFormatOptions(SpecTypeId.SurfaceAreaPerUnitLength, foUTSurfaceArea);

            //UTPipeDimension
            var foUTPipeDimension = units.GetFormatOptions(SpecTypeId.PipeDimension);
            foUTPipeDimension.Accuracy = 0.01;
            foUTPipeDimension.SetUnitTypeId(UnitTypeId.Millimeters);
            units.SetFormatOptions(SpecTypeId.PipeDimension, foUTPipeDimension);

            //UTPipeMass
            var foUTPipeMass = units.GetFormatOptions(SpecTypeId.PipeMassPerUnitLength);
            foUTPipeMass.Accuracy = 0.01;
            foUTPipeMass.SetUnitTypeId(UnitTypeId.KilogramsPerMeter);
            units.SetFormatOptions(SpecTypeId.PipeMassPerUnitLength, foUTPipeMass);

            //UTPipeMassPerUnitLength
            var foUTPipeMassPerUnitLength = units.GetFormatOptions(SpecTypeId.PipeMassPerUnitLength);
            foUTPipeMassPerUnitLength.Accuracy = 0.01;
            foUTPipeMassPerUnitLength.SetUnitTypeId(UnitTypeId.KilogramsPerMeter);
            units.SetFormatOptions(SpecTypeId.PipeMassPerUnitLength, foUTPipeMassPerUnitLength);


            using (var t = new Transaction(doc, "Convert to Metric"))
            {
                t.Start();

                doc.SetUnits(units);

                t.Commit();
            }
        }

        private void BatchImperial(Document doc)
        {
            var units = doc.GetUnits();


            //UTLength
            var foUTLength = units.GetFormatOptions(SpecTypeId.Length);
            foUTLength.Accuracy = 0.00260416666666667;
            foUTLength.SetUnitTypeId(UnitTypeId.FractionalInches);
            units.SetFormatOptions(SpecTypeId.Length, foUTLength);
            //UTArea
            var foUTArea = units.GetFormatOptions(SpecTypeId.Area);
            foUTArea.Accuracy = 0.01;
            foUTArea.SetUnitTypeId(UnitTypeId.SquareFeet);
            units.SetFormatOptions(SpecTypeId.Area, foUTArea);
            //UTVolume
            var foUTVolume = units.GetFormatOptions(SpecTypeId.Volume);
            foUTVolume.Accuracy = 0.01;
            foUTVolume.SetUnitTypeId(UnitTypeId.CubicFeet);
            units.SetFormatOptions(SpecTypeId.Volume, foUTVolume);
            //UTAngle
            var foUTAngle = units.GetFormatOptions(SpecTypeId.Angle);
            foUTAngle.Accuracy = 0.01;
            foUTAngle.SetUnitTypeId(UnitTypeId.Degrees);
            units.SetFormatOptions(SpecTypeId.Angle, foUTAngle);
            //UTHVACDensity
            var foUTHVACDensity = units.GetFormatOptions(SpecTypeId.HvacDensity);
            foUTHVACDensity.Accuracy = 0.0001;
            foUTHVACDensity.SetUnitTypeId(UnitTypeId.PoundsMassPerCubicFoot);
            units.SetFormatOptions(SpecTypeId.HvacDensity, foUTHVACDensity);


            //UTHVACEnergy
            var foUTHVACEnergy = units.GetFormatOptions(SpecTypeId.HvacEnergy);
            foUTHVACEnergy.Accuracy = 1;
            foUTHVACEnergy.SetUnitTypeId(UnitTypeId.BritishThermalUnits);
            units.SetFormatOptions(SpecTypeId.HvacEnergy, foUTHVACEnergy);
            //UTHVACFriction
            var foUTHVACFriction = units.GetFormatOptions(SpecTypeId.HvacFriction);
            foUTHVACFriction.Accuracy = 0.01;
            foUTHVACFriction.SetUnitTypeId(UnitTypeId.InchesOfWater60DegreesFahrenheitPer100Feet);
            units.SetFormatOptions(SpecTypeId.HvacFriction, foUTHVACFriction);
            //UTHVACPower
            var foUTHVACPower = units.GetFormatOptions(SpecTypeId.HvacPower);
            foUTHVACPower.Accuracy = 1;
            foUTHVACPower.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.HvacPower, foUTHVACPower);
            //UTHVACPowerDensity
            var foUTHVACPowerDensity = units.GetFormatOptions(SpecTypeId.HvacPowerDensity);
            foUTHVACPowerDensity.Accuracy = 0.01;
            foUTHVACPowerDensity.SetUnitTypeId(UnitTypeId.WattsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.HvacPowerDensity, foUTHVACPowerDensity);
            //UTHVACPressure
            var foUTHVACPressure = units.GetFormatOptions(SpecTypeId.HvacPressure);
            foUTHVACPressure.Accuracy = 0.01;
            foUTHVACPressure.SetUnitTypeId(UnitTypeId.InchesOfWater60DegreesFahrenheit);
            units.SetFormatOptions(SpecTypeId.HvacPressure, foUTHVACPressure);
            //UTHVACTemperature
            var foUTHVACTemperature = units.GetFormatOptions(SpecTypeId.HvacTemperature);
            foUTHVACTemperature.Accuracy = 1;
            foUTHVACTemperature.SetUnitTypeId(UnitTypeId.Fahrenheit);
            units.SetFormatOptions(SpecTypeId.HvacTemperature, foUTHVACTemperature);


            //UTHVACVelocity
            var foUTHVACVelocity = units.GetFormatOptions(SpecTypeId.HvacVelocity);
            foUTHVACVelocity.Accuracy = 1;
            foUTHVACVelocity.SetUnitTypeId(UnitTypeId.FeetPerMinute);
            units.SetFormatOptions(SpecTypeId.HvacVelocity, foUTHVACVelocity);
            //UTHVACAirFlow
            var foUTHVACAirFlow = units.GetFormatOptions(SpecTypeId.AirFlow);
            foUTHVACAirFlow.Accuracy = 1;
            foUTHVACAirFlow.SetUnitTypeId(UnitTypeId.CubicFeetPerMinute);
            units.SetFormatOptions(SpecTypeId.AirFlow, foUTHVACAirFlow);
            //UTHVACDuctSize
            var foUTHVACDuctSize = units.GetFormatOptions(SpecTypeId.DuctSize);
            foUTHVACDuctSize.Accuracy = 1;
            foUTHVACDuctSize.SetUnitTypeId(UnitTypeId.FractionalInches);
            units.SetFormatOptions(SpecTypeId.DuctSize, foUTHVACDuctSize);
            //UTHVACCrossSection
            var foUTHVACCrossSection = units.GetFormatOptions(SpecTypeId.CrossSection);
            foUTHVACCrossSection.Accuracy = 0.01;
            foUTHVACCrossSection.SetUnitTypeId(UnitTypeId.SquareInches);
            units.SetFormatOptions(SpecTypeId.CrossSection, foUTHVACCrossSection);
            //UTHVACHeatGain
            var foUTHVACHeatGain = units.GetFormatOptions(SpecTypeId.HeatGain);
            foUTHVACHeatGain.Accuracy = 0.1;
            foUTHVACHeatGain.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.HeatGain, foUTHVACHeatGain);
            //UTElectricalCurrent
            var foUTElectricalCurrent = units.GetFormatOptions(SpecTypeId.Current);
            foUTElectricalCurrent.Accuracy = 1;
            foUTElectricalCurrent.SetUnitTypeId(UnitTypeId.Amperes);
            units.SetFormatOptions(SpecTypeId.Current, foUTElectricalCurrent);
            //UTElectricalPotential
            var foUTElectricalPotential = units.GetFormatOptions(SpecTypeId.ElectricalPotential);
            foUTElectricalPotential.Accuracy = 1;
            foUTElectricalPotential.SetUnitTypeId(UnitTypeId.Volts);
            units.SetFormatOptions(SpecTypeId.ElectricalPotential, foUTElectricalPotential);
            //UTElectricalFrequency
            var foUTElectricalFrequency = units.GetFormatOptions(SpecTypeId.ElectricalFrequency);
            foUTElectricalFrequency.Accuracy = 1;
            foUTElectricalFrequency.SetUnitTypeId(UnitTypeId.Hertz);
            units.SetFormatOptions(SpecTypeId.ElectricalFrequency, foUTElectricalFrequency);
            //UTElectricalIlluminance
            var foUTElectricalIlluminance = units.GetFormatOptions(SpecTypeId.Illuminance);
            foUTElectricalIlluminance.Accuracy = 1;
            foUTElectricalIlluminance.SetUnitTypeId(UnitTypeId.Lux);
            units.SetFormatOptions(SpecTypeId.Illuminance, foUTElectricalIlluminance);


            //UTElectricalLuminance
            var foUTElectricalLuminance = units.GetFormatOptions(SpecTypeId.Luminance);
            foUTElectricalLuminance.Accuracy = 1;
            foUTElectricalLuminance.SetUnitTypeId(UnitTypeId.CandelasPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.Luminance, foUTElectricalLuminance);
            //UTElectricalLuminousFlux
            var foUTElectricalLuminousFlux = units.GetFormatOptions(SpecTypeId.LuminousFlux);
            foUTElectricalLuminousFlux.Accuracy = 1;
            foUTElectricalLuminousFlux.SetUnitTypeId(UnitTypeId.Lumens);
            units.SetFormatOptions(SpecTypeId.LuminousFlux, foUTElectricalLuminousFlux);
            //UTElectricalLuminousIntensity
            var foUTElectricalLuminousIntensity = units.GetFormatOptions(SpecTypeId.LuminousIntensity);
            foUTElectricalLuminousIntensity.Accuracy = 1;
            foUTElectricalLuminousIntensity.SetUnitTypeId(UnitTypeId.Candelas);
            units.SetFormatOptions(SpecTypeId.LuminousIntensity, foUTElectricalLuminousIntensity);
            //UTElectricalEfficacy
            var foUTElectricalEfficacy = units.GetFormatOptions(SpecTypeId.Efficacy);
            foUTElectricalEfficacy.Accuracy = 1;
            foUTElectricalEfficacy.SetUnitTypeId(UnitTypeId.LumensPerWatt);
            units.SetFormatOptions(SpecTypeId.Efficacy, foUTElectricalEfficacy);
            //UTElectricalWattage
            var foUTElectricalWattage = units.GetFormatOptions(SpecTypeId.Wattage);
            foUTElectricalWattage.Accuracy = 1;
            foUTElectricalWattage.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.Wattage, foUTElectricalWattage);
            //UTColorTemperature
            var foUTColorTemperature = units.GetFormatOptions(SpecTypeId.ColorTemperature);
            foUTColorTemperature.Accuracy = 1;
            foUTColorTemperature.SetUnitTypeId(UnitTypeId.Kelvin);
            units.SetFormatOptions(SpecTypeId.ColorTemperature, foUTColorTemperature);
            //UTElectricalPower
            var foUTElectricalPower = units.GetFormatOptions(SpecTypeId.ElectricalPower);
            foUTElectricalPower.Accuracy = 1;
            foUTElectricalPower.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.ElectricalPower, foUTElectricalPower);


            //UTHVACRoughness
            var foUTHVACRoughness = units.GetFormatOptions(SpecTypeId.HvacRoughness);
            foUTHVACRoughness.Accuracy = 0.0001;
            foUTHVACRoughness.SetUnitTypeId(UnitTypeId.Feet);
            units.SetFormatOptions(SpecTypeId.HvacRoughness, foUTHVACRoughness);
            //UTElectricalApparentPower
            var foUTElectricalApparentPower = units.GetFormatOptions(SpecTypeId.ApparentPower);
            foUTElectricalApparentPower.Accuracy = 1;
            foUTElectricalApparentPower.SetUnitTypeId(UnitTypeId.VoltAmperes);
            units.SetFormatOptions(SpecTypeId.ApparentPower, foUTElectricalApparentPower);
            //UTElectricalPowerDensity
            var foUTElectricalPowerDensity = units.GetFormatOptions(SpecTypeId.ElectricalPowerDensity);
            foUTElectricalPowerDensity.Accuracy = 0.01;
            foUTElectricalPowerDensity.SetUnitTypeId(UnitTypeId.WattsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.ElectricalPowerDensity, foUTElectricalPowerDensity);
            //UTPipingDensity
            var foUTPipingDensity = units.GetFormatOptions(SpecTypeId.PipingDensity);
            foUTPipingDensity.Accuracy = 0.0001;
            foUTPipingDensity.SetUnitTypeId(UnitTypeId.PoundsMassPerCubicFoot);
            units.SetFormatOptions(SpecTypeId.PipingDensity, foUTPipingDensity);
            //UTPipingFlow
            var foUTPipingFlow = units.GetFormatOptions(SpecTypeId.Flow);
            foUTPipingFlow.Accuracy = 1;
            foUTPipingFlow.SetUnitTypeId(UnitTypeId.UsGallonsPerMinute);
            units.SetFormatOptions(SpecTypeId.Flow, foUTPipingFlow);
            //UTPipingFriction
            var foUTPipingFriction = units.GetFormatOptions(SpecTypeId.PipingFriction);
            foUTPipingFriction.Accuracy = 0.01;
            foUTPipingFriction.SetUnitTypeId(UnitTypeId.FeetOfWater39_2DegreesFahrenheitPer100Feet);
            units.SetFormatOptions(SpecTypeId.PipingFriction, foUTPipingFriction);
            //UTPipingPressure
            var foUTPipingPressure = units.GetFormatOptions(SpecTypeId.PipingPressure);
            foUTPipingPressure.Accuracy = 0.01;
            foUTPipingPressure.SetUnitTypeId(UnitTypeId.PoundsForcePerSquareInch);
            units.SetFormatOptions(SpecTypeId.PipingPressure, foUTPipingPressure);


            //UTPipingTemperature
            var foUTPipingTemperature = units.GetFormatOptions(SpecTypeId.PipingTemperature);
            foUTPipingTemperature.Accuracy = 1;
            foUTPipingTemperature.SetUnitTypeId(UnitTypeId.Fahrenheit);
            units.SetFormatOptions(SpecTypeId.PipingTemperature, foUTPipingTemperature);
            //UTPipingVelocity
            var foUTPipingVelocity = units.GetFormatOptions(SpecTypeId.PipingVelocity);
            foUTPipingVelocity.Accuracy = 1;
            foUTPipingVelocity.SetUnitTypeId(UnitTypeId.FeetPerSecond);
            units.SetFormatOptions(SpecTypeId.PipingVelocity, foUTPipingVelocity);
            //UTPipingViscosity
            var foUTPipingViscosity = units.GetFormatOptions(SpecTypeId.PipingViscosity);
            foUTPipingViscosity.Accuracy = 0.01;
            foUTPipingViscosity.SetUnitTypeId(UnitTypeId.Centipoises);
            units.SetFormatOptions(SpecTypeId.PipingViscosity, foUTPipingViscosity);
            //UTPipeSize
            var foUTPipeSize = units.GetFormatOptions(SpecTypeId.PipeSize);
            foUTPipeSize.Accuracy = 1;
            foUTPipeSize.SetUnitTypeId(UnitTypeId.FractionalInches);
            units.SetFormatOptions(SpecTypeId.PipeSize, foUTPipeSize);
            //UTPipingRoughness
            var foUTPipingRoughness = units.GetFormatOptions(SpecTypeId.PipingRoughness);
            foUTPipingRoughness.Accuracy = 1E-05;
            foUTPipingRoughness.SetUnitTypeId(UnitTypeId.Feet);
            units.SetFormatOptions(SpecTypeId.PipingRoughness, foUTPipingRoughness);
            //UTPipingVolume
            var foUTPipingVolume = units.GetFormatOptions(SpecTypeId.PipingVolume);
            foUTPipingVolume.Accuracy = 0.1;
            foUTPipingVolume.SetUnitTypeId(UnitTypeId.UsGallons);
            units.SetFormatOptions(SpecTypeId.PipingVolume, foUTPipingVolume);
            //UTHVACViscosity
            var foUTHVACViscosity = units.GetFormatOptions(SpecTypeId.HvacViscosity);
            foUTHVACViscosity.Accuracy = 0.01;
            foUTHVACViscosity.SetUnitTypeId(UnitTypeId.Centipoises);
            units.SetFormatOptions(SpecTypeId.HvacViscosity, foUTHVACViscosity);


            //UTHVACCoefficientOfHeatTransfer
            var foUTHVACCoefficientOfHeatTransfer = units.GetFormatOptions(SpecTypeId.HeatTransferCoefficient);
            foUTHVACCoefficientOfHeatTransfer.Accuracy = 0.0001;
            foUTHVACCoefficientOfHeatTransfer.SetUnitTypeId(UnitTypeId
                .BritishThermalUnitsPerHourSquareFootDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.HeatTransferCoefficient, foUTHVACCoefficientOfHeatTransfer);

            //UTHVACThermalResistance
            var foUTHVACThermalResistance = units.GetFormatOptions(SpecTypeId.ThermalResistance);
            foUTHVACThermalResistance.Accuracy = 0.0001;
            foUTHVACThermalResistance.SetUnitTypeId(UnitTypeId.HourSquareFootDegreesFahrenheitPerBritishThermalUnit);
            units.SetFormatOptions(SpecTypeId.ThermalResistance, foUTHVACThermalResistance);

            //UTHVACThermalMass
            var foUTHVACThermalMass = units.GetFormatOptions(SpecTypeId.ThermalMass);
            foUTHVACThermalMass.Accuracy = 0.0001;
            foUTHVACThermalMass.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.ThermalMass, foUTHVACThermalMass);
            //UTHVACThermalConductivity
            var foUTHVACThermalConductivity = units.GetFormatOptions(SpecTypeId.ThermalConductivity);
            foUTHVACThermalConductivity.Accuracy = 0.0001;
            foUTHVACThermalConductivity.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourFootDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.ThermalConductivity, foUTHVACThermalConductivity);

            //UTHVACSpecificHeat
            var foUTHVACSpecificHeat = units.GetFormatOptions(SpecTypeId.SpecificHeat);
            foUTHVACSpecificHeat.Accuracy = 0.0001;
            foUTHVACSpecificHeat.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerPoundDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.SpecificHeat, foUTHVACSpecificHeat);
            //UTHVACSpecificHeatOfVaporization
            var foUTHVACSpecificHeatOfVaporization =
                units.GetFormatOptions(SpecTypeId.SpecificHeatOfVaporization);
            foUTHVACSpecificHeatOfVaporization.Accuracy = 0.0001;
            foUTHVACSpecificHeatOfVaporization.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerPound);
            units.SetFormatOptions(SpecTypeId.SpecificHeatOfVaporization, foUTHVACSpecificHeatOfVaporization);

            //UTHVACPermeability
            var foUTHVACPermeability = units.GetFormatOptions(SpecTypeId.Permeability);
            foUTHVACPermeability.Accuracy = 0.0001;
            foUTHVACPermeability.SetUnitTypeId(UnitTypeId.GrainsPerHourSquareFootInchMercury);
            units.SetFormatOptions(SpecTypeId.Permeability, foUTHVACPermeability);
            //UTElectricalResistivity
            var foUTElectricalResistivity = units.GetFormatOptions(SpecTypeId.ElectricalResistivity);
            foUTElectricalResistivity.Accuracy = 0.0001;
            foUTElectricalResistivity.SetUnitTypeId(UnitTypeId.OhmMeters);
            units.SetFormatOptions(SpecTypeId.ElectricalResistivity, foUTElectricalResistivity);

            //UTHVACAirFlowDensity
            var foUTHVACAirFlowDensity = units.GetFormatOptions(SpecTypeId.AirFlowDensity);
            foUTHVACAirFlowDensity.Accuracy = 0.01;
            foUTHVACAirFlowDensity.SetUnitTypeId(UnitTypeId.CubicFeetPerMinuteSquareFoot);
            units.SetFormatOptions(SpecTypeId.AirFlowDensity, foUTHVACAirFlowDensity);
            //UTSlope
            var foUTSlope = units.GetFormatOptions(SpecTypeId.Slope);
            foUTSlope.Accuracy = 0.01;
            foUTSlope.SetUnitTypeId(UnitTypeId.SlopeDegrees);
            units.SetFormatOptions(SpecTypeId.Slope, foUTSlope);


            //UTHVACCoolingLoad
            var foUTHVACCoolingLoad = units.GetFormatOptions(SpecTypeId.CoolingLoad);
            foUTHVACCoolingLoad.Accuracy = 0.1;
            foUTHVACCoolingLoad.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.CoolingLoad, foUTHVACCoolingLoad);
            //UTHVACHeatingLoad
            var foUTHVACHeatingLoad = units.GetFormatOptions(SpecTypeId.HeatingLoad);
            foUTHVACHeatingLoad.Accuracy = 0.1;
            foUTHVACHeatingLoad.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.HeatingLoad, foUTHVACHeatingLoad);
            //UTHVACCoolingLoadDividedByArea
            var foUTHVACCoolingLoadDividedByArea = units.GetFormatOptions(SpecTypeId.CoolingLoadDividedByArea);
            foUTHVACCoolingLoadDividedByArea.Accuracy = 0.01;
            foUTHVACCoolingLoadDividedByArea.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourSquareFoot);
            units.SetFormatOptions(SpecTypeId.CoolingLoadDividedByArea, foUTHVACCoolingLoadDividedByArea);
            //UTHVACHeatingLoadDividedByArea
            var foUTHVACHeatingLoadDividedByArea =
                units.GetFormatOptions(SpecTypeId.HeatingLoadDividedByArea);
            foUTHVACHeatingLoadDividedByArea.Accuracy = 0.01;
            foUTHVACHeatingLoadDividedByArea.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourSquareFoot);
            units.SetFormatOptions(SpecTypeId.HeatingLoadDividedByArea, foUTHVACHeatingLoadDividedByArea);
            //UTHVACCoolingLoadDividedByVolume
            var foUTHVACCoolingLoadDividedByVolume =
                units.GetFormatOptions(SpecTypeId.CoolingLoadDividedByVolume);
            foUTHVACCoolingLoadDividedByVolume.Accuracy = 0.01;
            foUTHVACCoolingLoadDividedByVolume.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourCubicFoot);
            units.SetFormatOptions(SpecTypeId.CoolingLoadDividedByVolume, foUTHVACCoolingLoadDividedByVolume);
            //UTHVACHeatingLoadDividedByVolume
            var foUTHVACHeatingLoadDividedByVolume =
                units.GetFormatOptions(SpecTypeId.HeatingLoadDividedByVolume);
            foUTHVACHeatingLoadDividedByVolume.Accuracy = 0.01;
            foUTHVACHeatingLoadDividedByVolume.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourCubicFoot);
            units.SetFormatOptions(SpecTypeId.HeatingLoadDividedByVolume, foUTHVACHeatingLoadDividedByVolume);


            //UTHVACAirFlowDividedByVolume
            var foUTHVACAirFlowDividedByVolume = units.GetFormatOptions(SpecTypeId.AirFlowDividedByVolume);
            foUTHVACAirFlowDividedByVolume.Accuracy = 0.01;
            foUTHVACAirFlowDividedByVolume.SetUnitTypeId(UnitTypeId.CubicFeetPerMinuteCubicFoot);
            units.SetFormatOptions(SpecTypeId.AirFlowDividedByVolume, foUTHVACAirFlowDividedByVolume);
            //UTHVACAirFlowDividedByCoolingLoad
            var foUTHVACAirFlowDividedByCoolingLoad =
                units.GetFormatOptions(SpecTypeId.AirFlowDividedByCoolingLoad);
            foUTHVACAirFlowDividedByCoolingLoad.Accuracy = 0.01;
            foUTHVACAirFlowDividedByCoolingLoad.SetUnitTypeId(UnitTypeId.CubicFeetPerMinuteTonOfRefrigeration);
            units.SetFormatOptions(SpecTypeId.AirFlowDividedByCoolingLoad,
                foUTHVACAirFlowDividedByCoolingLoad);
            //UTHVACAreaDividedByCoolingLoad
            var foUTHVACAreaDividedByCoolingLoad =
                units.GetFormatOptions(SpecTypeId.AreaDividedByCoolingLoad);
            foUTHVACAreaDividedByCoolingLoad.Accuracy = 0.01;
            foUTHVACAreaDividedByCoolingLoad.SetUnitTypeId(UnitTypeId.SquareFeetPerTonOfRefrigeration);
            units.SetFormatOptions(SpecTypeId.AreaDividedByCoolingLoad, foUTHVACAreaDividedByCoolingLoad);
            //UTHVACAreaDividedByHeatingLoad
            var foUTHVACAreaDividedByHeatingLoad =
                units.GetFormatOptions(SpecTypeId.AreaDividedByHeatingLoad);
            foUTHVACAreaDividedByHeatingLoad.Accuracy = 0.0001;
            foUTHVACAreaDividedByHeatingLoad.SetUnitTypeId(UnitTypeId.SquareFeetPer1000BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.AreaDividedByHeatingLoad, foUTHVACAreaDividedByHeatingLoad);


            //UTWireSize
            var foUTWireSize = units.GetFormatOptions(SpecTypeId.WireDiameter);
            foUTWireSize.Accuracy = 1E-06;
            foUTWireSize.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.WireDiameter, foUTWireSize);
            //UTHVACSlope
            var foUTHVACSlope = units.GetFormatOptions(SpecTypeId.HvacSlope);
            foUTHVACSlope.Accuracy = 0.03125;
            foUTHVACSlope.SetUnitTypeId(UnitTypeId.RiseDividedBy120Inches);
            units.SetFormatOptions(SpecTypeId.HvacSlope, foUTHVACSlope);
            //UTPipingSlope
            var foUTPipingSlope = units.GetFormatOptions(SpecTypeId.PipingSlope);
            foUTPipingSlope.Accuracy = 0.03125;
            foUTPipingSlope.SetUnitTypeId(UnitTypeId.RiseDividedBy120Inches);
            units.SetFormatOptions(SpecTypeId.PipingSlope, foUTPipingSlope);
            //UTCurrency
            var foUTCurrency = units.GetFormatOptions(SpecTypeId.Currency);
            foUTCurrency.Accuracy = 0.01;
            foUTCurrency.SetUnitTypeId(UnitTypeId.Currency);
            units.SetFormatOptions(SpecTypeId.Currency, foUTCurrency);
            //UTMassDensity
            var foUTMassDensity = units.GetFormatOptions(SpecTypeId.MassDensity);
            foUTMassDensity.Accuracy = 0.01;
            foUTMassDensity.SetUnitTypeId(UnitTypeId.PoundsMassPerCubicFoot);
            units.SetFormatOptions(SpecTypeId.MassDensity, foUTMassDensity);
            //UTHVACFactor
            var foUTHVACFactor = units.GetFormatOptions(SpecTypeId.Factor);
            foUTHVACFactor.Accuracy = 0.01;
            foUTHVACFactor.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.Factor, foUTHVACFactor);

            //UTElectricalTemperature
            var foUTElectricalTemperature = units.GetFormatOptions(SpecTypeId.ElectricalTemperature);
            foUTElectricalTemperature.Accuracy = 1;
            foUTElectricalTemperature.SetUnitTypeId(UnitTypeId.Fahrenheit);
            units.SetFormatOptions(SpecTypeId.ElectricalTemperature, foUTElectricalTemperature);
            //UTElectricalCableTraySize
            var foUTElectricalCableTraySize = units.GetFormatOptions(SpecTypeId.CableTraySize);
            foUTElectricalCableTraySize.Accuracy = 0.25;
            foUTElectricalCableTraySize.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.CableTraySize, foUTElectricalCableTraySize);
            //UTElectricalConduitSize
            var foUTElectricalConduitSize = units.GetFormatOptions(SpecTypeId.ConduitSize);
            foUTElectricalConduitSize.Accuracy = 0.125;
            foUTElectricalConduitSize.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.ConduitSize, foUTElectricalConduitSize);
            //UTElectricalDemandFactor
            var foUTElectricalDemandFactor = units.GetFormatOptions(SpecTypeId.DemandFactor);
            foUTElectricalDemandFactor.Accuracy = 0.01;
            foUTElectricalDemandFactor.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.DemandFactor, foUTElectricalDemandFactor);
            //UTHVACDuctInsulationThickness
            var foUTHVACDuctInsulationThickness = units.GetFormatOptions(SpecTypeId.DuctInsulationThickness);
            foUTHVACDuctInsulationThickness.Accuracy = 1;
            foUTHVACDuctInsulationThickness.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.DuctInsulationThickness, foUTHVACDuctInsulationThickness);
            //UTHVACDuctLiningThickness
            var foUTHVACDuctLiningThickness = units.GetFormatOptions(SpecTypeId.DuctLiningThickness);
            foUTHVACDuctLiningThickness.Accuracy = 1;
            foUTHVACDuctLiningThickness.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.DuctLiningThickness, foUTHVACDuctLiningThickness);
            //UTPipeInsulationThickness
            var foUTPipeInsulationThickness = units.GetFormatOptions(SpecTypeId.PipeInsulationThickness);
            foUTPipeInsulationThickness.Accuracy = 1;
            foUTPipeInsulationThickness.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.PipeInsulationThickness, foUTPipeInsulationThickness);


            //UTForce
            var foUTForce = units.GetFormatOptions(SpecTypeId.Force);
            foUTForce.Accuracy = 0.01;
            foUTForce.SetUnitTypeId(UnitTypeId.Kips);
            units.SetFormatOptions(SpecTypeId.Force, foUTForce);
            //UTLinearForce
            var foUTLinearForce = units.GetFormatOptions(SpecTypeId.LinearForce);
            foUTLinearForce.Accuracy = 0.001;
            foUTLinearForce.SetUnitTypeId(UnitTypeId.KipsPerFoot);
            units.SetFormatOptions(SpecTypeId.LinearForce, foUTLinearForce);
            //UTAreaForce
            var foUTAreaForce = units.GetFormatOptions(SpecTypeId.AreaForce);
            foUTAreaForce.Accuracy = 0.0001;
            foUTAreaForce.SetUnitTypeId(UnitTypeId.KipsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.AreaForce, foUTAreaForce);
            //UTMoment
            var foUTMoment = units.GetFormatOptions(SpecTypeId.Moment);
            foUTMoment.Accuracy = 0.01;
            foUTMoment.SetUnitTypeId(UnitTypeId.KipFeet);
            units.SetFormatOptions(SpecTypeId.Moment, foUTMoment);
            //UTLinearMoment
            var foUTLinearMoment = units.GetFormatOptions(SpecTypeId.LinearMoment);
            foUTLinearMoment.Accuracy = 0.01;
            foUTLinearMoment.SetUnitTypeId(UnitTypeId.KipFeetPerFoot);
            units.SetFormatOptions(SpecTypeId.LinearMoment, foUTLinearMoment);
            //UTStress
            var foUTStress = units.GetFormatOptions(SpecTypeId.Stress);
            foUTStress.Accuracy = 0.01;
            foUTStress.SetUnitTypeId(UnitTypeId.KipsPerSquareInch);
            units.SetFormatOptions(SpecTypeId.Stress, foUTStress);
            //UTUnitWeight
            var foUTUnitWeight = units.GetFormatOptions(SpecTypeId.UnitWeight);
            foUTUnitWeight.Accuracy = 0.01;
            foUTUnitWeight.SetUnitTypeId(UnitTypeId.PoundsForcePerCubicFoot);
            units.SetFormatOptions(SpecTypeId.UnitWeight, foUTUnitWeight);
            //UTWeight
            var foUTWeight = units.GetFormatOptions(SpecTypeId.Weight);
            foUTWeight.Accuracy = 0.01;
            foUTWeight.SetUnitTypeId(UnitTypeId.PoundsForce);
            units.SetFormatOptions(SpecTypeId.Weight, foUTWeight);
            //UTMass
            var foUTMass = units.GetFormatOptions(SpecTypeId.Mass);
            foUTMass.Accuracy = 0.01;
            foUTMass.SetUnitTypeId(UnitTypeId.PoundsMass);
            units.SetFormatOptions(SpecTypeId.Mass, foUTMass);


            //UTMassPerUnitArea
            var foUTMassPerUnitArea = units.GetFormatOptions(SpecTypeId.MassPerUnitArea);
            foUTMassPerUnitArea.Accuracy = 0.01;
            foUTMassPerUnitArea.SetUnitTypeId(UnitTypeId.PoundsMassPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.MassPerUnitArea, foUTMassPerUnitArea);


            //UTThermalExpansion
            var foUTThermalExpansion = units.GetFormatOptions(SpecTypeId.ThermalExpansionCoefficient);
            foUTThermalExpansion.Accuracy = 1E-05;
            foUTThermalExpansion.SetUnitTypeId(UnitTypeId.InverseDegreesFahrenheit);
            units.SetFormatOptions(SpecTypeId.ThermalExpansionCoefficient, foUTThermalExpansion);

            //UTForcePerLength
            var foUTForcePerLength = units.GetFormatOptions(SpecTypeId.LinearForce);
            foUTForcePerLength.Accuracy = 0.1;
            foUTForcePerLength.SetUnitTypeId(UnitTypeId.KipsPerInch);
            units.SetFormatOptions(SpecTypeId.LinearForce, foUTForcePerLength);
            //UTLinearForcePerLength


            //UTAreaForcePerLength
            var foUTAreaForcePerLength = units.GetFormatOptions(SpecTypeId.AreaForce);
            foUTAreaForcePerLength.Accuracy = 0.1;
            foUTAreaForcePerLength.SetUnitTypeId(UnitTypeId.KipsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.AreaForce, foUTAreaForcePerLength);


            //UTDisplacementDeflection
            var foUTDisplacementDeflection = units.GetFormatOptions(SpecTypeId.Displacement);
            foUTDisplacementDeflection.Accuracy = 0.01;
            foUTDisplacementDeflection.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.Displacement, foUTDisplacementDeflection);


            //UTRotation
            var foUTRotation = units.GetFormatOptions(SpecTypeId.Rotation);
            foUTRotation.Accuracy = 0.001;
            foUTRotation.SetUnitTypeId(UnitTypeId.Degrees);
            units.SetFormatOptions(SpecTypeId.Rotation, foUTRotation);
            //UTPeriod
            var foUTPeriod = units.GetFormatOptions(SpecTypeId.Period);
            foUTPeriod.Accuracy = 0.1;
            foUTPeriod.SetUnitTypeId(UnitTypeId.Seconds);
            units.SetFormatOptions(SpecTypeId.Period, foUTPeriod);
            //UTStructuralFrequency
            var foUTStructuralFrequency = units.GetFormatOptions(SpecTypeId.StructuralFrequency);
            foUTStructuralFrequency.Accuracy = 0.1;
            foUTStructuralFrequency.SetUnitTypeId(UnitTypeId.Hertz);
            units.SetFormatOptions(SpecTypeId.StructuralFrequency, foUTStructuralFrequency);
            //UTPulsation
            var foUTPulsation = units.GetFormatOptions(SpecTypeId.Pulsation);
            foUTPulsation.Accuracy = 0.1;
            foUTPulsation.SetUnitTypeId(UnitTypeId.RadiansPerSecond);
            units.SetFormatOptions(SpecTypeId.Pulsation, foUTPulsation);
            //UTStructuralVelocity
            var foUTStructuralVelocity = units.GetFormatOptions(SpecTypeId.StructuralVelocity);
            foUTStructuralVelocity.Accuracy = 0.1;
            foUTStructuralVelocity.SetUnitTypeId(UnitTypeId.FeetPerSecond);
            units.SetFormatOptions(SpecTypeId.StructuralVelocity, foUTStructuralVelocity);


            //UTAcceleration
            var foUTAcceleration = units.GetFormatOptions(SpecTypeId.Acceleration);
            foUTAcceleration.Accuracy = 0.1;
            foUTAcceleration.SetUnitTypeId(UnitTypeId.FeetPerSecondSquared);
            units.SetFormatOptions(SpecTypeId.Acceleration, foUTAcceleration);
            //UTEnergy
            var foUTEnergy = units.GetFormatOptions(SpecTypeId.Energy);
            foUTEnergy.Accuracy = 0.1;
            foUTEnergy.SetUnitTypeId(UnitTypeId.PoundForceFeet);
            units.SetFormatOptions(SpecTypeId.Energy, foUTEnergy);
            //UTReinforcementVolume
            var foUTReinforcementVolume = units.GetFormatOptions(SpecTypeId.ReinforcementVolume);
            foUTReinforcementVolume.Accuracy = 0.01;
            foUTReinforcementVolume.SetUnitTypeId(UnitTypeId.CubicInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementVolume, foUTReinforcementVolume);
            //UTReinforcementLength
            var foUTReinforcementLength = units.GetFormatOptions(SpecTypeId.ReinforcementLength);
            foUTReinforcementLength.Accuracy = 0.00260416666666667;
            foUTReinforcementLength.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementLength, foUTReinforcementLength);

            //UTReinforcementArea
            var foUTReinforcementArea = units.GetFormatOptions(SpecTypeId.ReinforcementArea);
            foUTReinforcementArea.Accuracy = 0.01;
            foUTReinforcementArea.SetUnitTypeId(UnitTypeId.SquareInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementArea, foUTReinforcementArea);
            //UTReinforcementAreaperUnitLength
            var foUTReinforcementAreaperUnitLength =
                units.GetFormatOptions(SpecTypeId.ReinforcementAreaPerUnitLength);
            foUTReinforcementAreaperUnitLength.Accuracy = 0.01;
            foUTReinforcementAreaperUnitLength.SetUnitTypeId(UnitTypeId.SquareInchesPerFoot);
            units.SetFormatOptions(SpecTypeId.ReinforcementAreaPerUnitLength, foUTReinforcementAreaperUnitLength);
            //UTReinforcementSpacing
            var foUTReinforcementSpacing = units.GetFormatOptions(SpecTypeId.ReinforcementSpacing);
            foUTReinforcementSpacing.Accuracy = 0.00260416666666667;
            foUTReinforcementSpacing.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementSpacing, foUTReinforcementSpacing);
            //UTReinforcementCover
            var foUTReinforcementCover = units.GetFormatOptions(SpecTypeId.ReinforcementCover);
            foUTReinforcementCover.Accuracy = 0.00260416666666667;
            foUTReinforcementCover.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementCover, foUTReinforcementCover);

            //UTBarDiameter
            var foUTBarDiameter = units.GetFormatOptions(SpecTypeId.BarDiameter);
            foUTBarDiameter.Accuracy = 0.00260416666666667;
            foUTBarDiameter.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.BarDiameter, foUTBarDiameter);
            //UTCrackWidth
            var foUTCrackWidth = units.GetFormatOptions(SpecTypeId.CrackWidth);
            foUTCrackWidth.Accuracy = 0.01;
            foUTCrackWidth.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.CrackWidth, foUTCrackWidth);
            //UTSectionDimension
            var foUTSectionDimension = units.GetFormatOptions(SpecTypeId.SectionDimension);
            foUTSectionDimension.Accuracy = 0.00520833333333333;
            foUTSectionDimension.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.SectionDimension, foUTSectionDimension);
            //UTSectionProperty
            var foUTSectionProperty = units.GetFormatOptions(SpecTypeId.SectionProperty);
            foUTSectionProperty.Accuracy = 0.001;
            foUTSectionProperty.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.SectionProperty, foUTSectionProperty);
            //UTSectionArea
            var foUTSectionArea = units.GetFormatOptions(SpecTypeId.SectionArea);
            foUTSectionArea.Accuracy = 0.01;
            foUTSectionArea.SetUnitTypeId(UnitTypeId.SquareInches);
            units.SetFormatOptions(SpecTypeId.SectionArea, foUTSectionArea);

            //UTSectionModulus
            var foUTSectionModulus = units.GetFormatOptions(SpecTypeId.SectionModulus);
            foUTSectionModulus.Accuracy = 0.01;
            foUTSectionModulus.SetUnitTypeId(UnitTypeId.CubicInches);
            units.SetFormatOptions(SpecTypeId.SectionModulus, foUTSectionModulus);
            //UTMomentofInertia
            var foUTMomentofInertia = units.GetFormatOptions(SpecTypeId.MomentOfInertia);
            foUTMomentofInertia.Accuracy = 0.01;
            foUTMomentofInertia.SetUnitTypeId(UnitTypeId.InchesToTheFourthPower);
            units.SetFormatOptions(SpecTypeId.MomentOfInertia, foUTMomentofInertia);
            //UTWarpingConstant
            var foUTWarpingConstant = units.GetFormatOptions(SpecTypeId.WarpingConstant);
            foUTWarpingConstant.Accuracy = 0.01;
            foUTWarpingConstant.SetUnitTypeId(UnitTypeId.InchesToTheSixthPower);
            units.SetFormatOptions(SpecTypeId.WarpingConstant, foUTWarpingConstant);
            //UTMassperUnitLength
            var foUTMassperUnitLength = units.GetFormatOptions(SpecTypeId.MassPerUnitLength);
            foUTMassperUnitLength.Accuracy = 0.01;
            foUTMassperUnitLength.SetUnitTypeId(UnitTypeId.PoundsMassPerFoot);
            units.SetFormatOptions(SpecTypeId.MassPerUnitLength, foUTMassperUnitLength);
            //UTWeightperUnitLength
            var foUTWeightperUnitLength = units.GetFormatOptions(SpecTypeId.WeightPerUnitLength);
            foUTWeightperUnitLength.Accuracy = 0.01;
            foUTWeightperUnitLength.SetUnitTypeId(UnitTypeId.PoundsForcePerFoot);
            units.SetFormatOptions(SpecTypeId.WeightPerUnitLength, foUTWeightperUnitLength);

            //UTSurfaceArea
            var foUTSurfaceArea = units.GetFormatOptions(SpecTypeId.SurfaceAreaPerUnitLength);
            foUTSurfaceArea.Accuracy = 0.01;
            foUTSurfaceArea.SetUnitTypeId(UnitTypeId.SquareFeetPerFoot);
            units.SetFormatOptions(SpecTypeId.SurfaceAreaPerUnitLength, foUTSurfaceArea);
            //UTPipeDimension
            var foUTPipeDimension = units.GetFormatOptions(SpecTypeId.PipeDimension);
            foUTPipeDimension.Accuracy = 0.001;
            foUTPipeDimension.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.PipeDimension, foUTPipeDimension);

            //UTPipeMass
            var foUTPipeMass = units.GetFormatOptions(SpecTypeId.PipeMassPerUnitLength);
            foUTPipeMass.Accuracy = 0.01;
            foUTPipeMass.SetUnitTypeId(UnitTypeId.PoundsMassPerFoot);
            units.SetFormatOptions(SpecTypeId.PipeMassPerUnitLength, foUTPipeMass);

            //UTPipeMassPerUnitLength
            var foUTPipeMassPerUnitLength = units.GetFormatOptions(SpecTypeId.PipeMassPerUnitLength);
            foUTPipeMassPerUnitLength.Accuracy = 0.01;
            foUTPipeMassPerUnitLength.SetUnitTypeId(UnitTypeId.PoundsMassPerFoot);
            units.SetFormatOptions(SpecTypeId.PipeMassPerUnitLength, foUTPipeMassPerUnitLength);


            using (var t = new Transaction(doc, "Convert to Imperial"))
            {
                t.Start();

                doc.SetUnits(units);

                t.Commit();
            }
        }

        private void radioButtonMetric_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.revit.com.au");
        }
    }
}