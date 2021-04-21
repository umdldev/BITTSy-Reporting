using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using CsvHelper;

namespace BITTSyReporting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Change this field in order to update the version number in both the Main Window
        //and in generated reports
        public String currentVersion = "0.4";

        public bool isSingleDocument = true;
        public String filepath = "";
        public String folderpath = "";
        public List<String> logPaths;
        public String savepath = "";
        public bool reportSelected = false;
        public String stub = "";
        public List<String> stimulusTypesToInclude = new List<String>() { "video", "image", "audio", "light" };
        private int numReports = 0;
        private List<String> selectedReports;

        //Lists that will help us determine if stimulus type selection should be allowed based on the selected report type(s) - all
        //report types in the first list are compatible with stimulus type selection, while all report types in the second list are not
        private readonly List<String> StimulusSelectReports = new List<String>() { "Listing of Played Media", "Overall Looking Time (By Trial)", "Number of Looks per Trial", "Individual Looks By Trial", "Summary Across Groups and Tags", "Habituation Information" };
        private readonly List<String> NAStimulusSelectReports = new List<String>() { "Header Information", "Overall Looking Information", "Summary Across Sides", "Detailed Looking Time", "All Event Info", "Custom Report" };
        private int numStimSelectReports = 0;

        public MainWindow()
        {
            InitializeComponent();

            logPaths = new List<String>();
            selectedReports = new List<String>();

            this.documentRadio.IsChecked = true;
            this.Version.Content = "Version " + currentVersion;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            //reset paths to log files, in case the user initially picked a document but now wants to load
            //a folder, or vice versa
            logPaths.Clear();

            if (isSingleDocument)
            {
                //loading a single protocol file
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
                if (fileDialog.ShowDialog() == true)
                {
                    this.filepath = fileDialog.FileName;
                }

                logPaths.Add(filepath);
            }
            else
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.folderpath = folderDialog.SelectedPath;
                    }
                }

                try
                {
                    DirectoryInfo d = new DirectoryInfo(folderpath);
                    if (d.Exists)
                    {
                        foreach (var file in d.GetFiles("*.txt"))
                        {
                            logPaths.Add(file.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //the user started to load a folder, then exited out, causing 'folderpath' to
                    //be invalid and throwing an exception - catch it here and do nothing
                    return;
                }

            }
        }

        private void StimType_Check(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkbox = (System.Windows.Controls.CheckBox)sender;
            String typeToAdd = checkbox.Content.ToString().ToLower();
            if (!stimulusTypesToInclude.Contains(typeToAdd))
            {
                stimulusTypesToInclude.Add(typeToAdd);
            }
        }

        private void StimType_Uncheck(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkbox = (System.Windows.Controls.CheckBox)sender;
            String typeToRemove = checkbox.Content.ToString().ToLower();
            if (stimulusTypesToInclude.Contains(typeToRemove))
            {
                stimulusTypesToInclude.Remove(typeToRemove);
            }
        }

        private void document_Check(object sender, RoutedEventArgs e)
        {
            isSingleDocument = true;
            this.LoadButton.Content = "Load Document";
            this.LoadButton.Visibility = Visibility.Visible;

            //reset path to folder, in case one was selected and then the user changed the type to Document
            folderpath = "";
        }

        private void folder_Check(object sender, RoutedEventArgs e)
        {
            isSingleDocument = false;
            this.LoadButton.Content = "Load Folder";
            this.LoadButton.Visibility = Visibility.Visible;

            //reset path to file, in case one was selected and then the user changed the type to File
            filepath = "";
        }

        private void report_Check(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkbox = (System.Windows.Controls.CheckBox)sender;
            String reportType = (String)checkbox.Content;
            reportSelected = true;

            if (!(selectedReports.Contains(reportType)))
            {
                selectedReports.Add(reportType);
            }

            numReports++;

            switch (reportType)
            {
                case "Listing of Played Media":
                case "Overall Looking Time (By Trial)":
                case "Number of Looks per Trial":
                case "Individual Looks By Trial":
                case "Summary Across Groups and Tags":
                case "Habituation Information":
                    numStimSelectReports++;

                    this.NoReportTypeSelected.Visibility = Visibility.Hidden;
                    this.StimulusTypesAllNAWarning.Visibility = Visibility.Hidden;
                    this.StimulusTypesNAWarning.Visibility = Visibility.Hidden;
                    this.StimulusTypesPrompt.Visibility = Visibility.Visible;

                    this.AudioBox.Visibility = Visibility.Visible;
                    this.VideoBox.Visibility = Visibility.Visible;
                    this.ImageBox.Visibility = Visibility.Visible;
                    this.LightBox.Visibility = Visibility.Visible;
                    this.AudioBox.IsChecked = true;
                    this.VideoBox.IsChecked = true;
                    this.ImageBox.IsChecked = true;
                    this.LightBox.IsChecked = true;

                    this.StimulusTypesNAWarning.Visibility = Visibility.Hidden;

                    if (numStimSelectReports != numReports)
                    {
                        StimTypeLabelHelper();
                        this.StimulusTypesSomeNAWarning.Visibility = Visibility.Visible;
                    }

                    break;
                case "Header Information":
                case "Overall Looking Information":
                case "Summary Across Sides":
                case "Detailed Looking Time":
                case "All Event Info":
                case "Custom Report":
                default:
                    if (numStimSelectReports == 0 && numReports == 1)
                    {
                        this.NoReportTypeSelected.Visibility = Visibility.Hidden;
                        this.StimulusTypesPrompt.Visibility = Visibility.Hidden;
                        this.StimulusTypesNAWarning.Visibility = Visibility.Visible;
                    }
                    else if (numStimSelectReports == 0)
                    {
                        this.StimulusTypesNAWarning.Visibility = Visibility.Hidden;
                        this.StimulusTypesAllNAWarning.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StimTypeLabelHelper();
                        this.StimulusTypesSomeNAWarning.Visibility = Visibility.Visible;
                    }

                    break;
            }

            //change "Generate Report" button to be plural if multiple report types have been selected
            if (numReports > 1)
            {
                this.GenerateButton.Content = "Generate Reports";
            }

            savepath = "";
        }

        private void report_Uncheck(object sender, RoutedEventArgs e)
        {
            numReports--;
            System.Windows.Controls.CheckBox checkbox = (System.Windows.Controls.CheckBox)sender;
            String reportType = (String)checkbox.Content;

            if (numReports == 0)
            {
                reportSelected = false;
            }

            if (selectedReports.Contains(reportType))
            {
                selectedReports.Remove(reportType);
            }

            switch (reportType)
            {
                case "Listing of Played Media":
                case "Overall Looking Time (By Trial)":
                case "Number of Looks per Trial":
                case "Individual Looks By Trial":
                case "Summary Across Groups and Tags":
                case "Habituation Information":
                    numStimSelectReports--;

                    if (numStimSelectReports == 0)
                    {
                        this.AudioBox.Visibility = Visibility.Hidden;
                        this.VideoBox.Visibility = Visibility.Hidden;
                        this.ImageBox.Visibility = Visibility.Hidden;
                        this.LightBox.Visibility = Visibility.Hidden;
                        this.AudioBox.IsChecked = true;
                        this.VideoBox.IsChecked = true;
                        this.ImageBox.IsChecked = true;
                        this.LightBox.IsChecked = true;

                        if (numReports == 0)
                        {
                            this.StimulusTypesPrompt.Visibility = Visibility.Visible;
                            this.NoReportTypeSelected.Visibility = Visibility.Visible;
                        }
                        else if (numReports == 1)
                        {
                            this.StimulusTypesPrompt.Visibility = Visibility.Hidden;
                            this.StimulusTypesNAWarning.Visibility = Visibility.Visible;
                            this.StimulusTypesSomeNAWarning.Visibility = Visibility.Hidden;
                            this.StimulusTypesAllNAWarning.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.StimulusTypesPrompt.Visibility = Visibility.Hidden;
                            this.StimulusTypesAllNAWarning.Visibility = Visibility.Hidden;
                            this.StimulusTypesSomeNAWarning.Visibility = Visibility.Hidden;
                            this.StimulusTypesAllNAWarning.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        StimTypeLabelHelper();
                        //this.StimulusTypesSomeNAWarning.Visibility = Visibility.Visible;
                    }
                    break;
                case "Header Information":
                case "Overall Looking Information":
                case "Summary Across Sides":
                case "Detailed Looking Time":
                case "All Event Info":
                case "Custom Report":
                default:
                    if (numReports == 0)
                    {
                        this.StimulusTypesNAWarning.Visibility = Visibility.Hidden;
                        this.StimulusTypesPrompt.Visibility = Visibility.Visible;
                        this.NoReportTypeSelected.Visibility = Visibility.Visible;
                    }
                    else if (numStimSelectReports == numReports)
                    {
                        this.StimulusTypesNAWarning.Visibility = Visibility.Hidden;
                        this.StimulusTypesSomeNAWarning.Visibility = Visibility.Hidden;
                        this.StimulusTypesAllNAWarning.Visibility = Visibility.Hidden;
                        this.StimulusTypesPrompt.Visibility = Visibility.Visible;
                        this.NoReportTypeSelected.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        if (numStimSelectReports == 0 && numReports == 1)
                        {
                            this.NoReportTypeSelected.Visibility = Visibility.Hidden;
                            this.StimulusTypesPrompt.Visibility = Visibility.Hidden;
                            this.StimulusTypesAllNAWarning.Visibility = Visibility.Hidden;
                            this.StimulusTypesNAWarning.Visibility = Visibility.Visible;
                        }
                        else if (numStimSelectReports == 0)
                        {
                            this.StimulusTypesNAWarning.Visibility = Visibility.Hidden;
                            this.StimulusTypesSomeNAWarning.Visibility = Visibility.Hidden;
                            this.StimulusTypesAllNAWarning.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            StimTypeLabelHelper();
                            this.StimulusTypesSomeNAWarning.Visibility = Visibility.Visible;
                        }
                    }
                    break;
            }

            if (numReports <= 1)
            {
                this.GenerateButton.Content = "Generate Report";
            }

            savepath = "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (numReports == 0)
            {
                System.Windows.MessageBox.Show("No Report Types Selected Yet!", "Error");
            }
            else if (numReports > 1 && !isSingleDocument)
            {
                //creating multiple reports for multiple log files, so each will be saved into its own file - need to prompt
                //for a folder to save all reports into
                OpenSaveFolderWindow();
            }
            else
            {
                //saving a single report
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
                saveDialog.Filter = "CSV (*.csv)|*.csv";
                if (saveDialog.ShowDialog() == true)
                {
                    savepath = saveDialog.FileName;
                }
            }
            
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (isSingleDocument && String.IsNullOrEmpty(filepath))
            {
                System.Windows.MessageBox.Show("No Protocol File Selected Yet!", "Error");
            }
            else if (!isSingleDocument && String.IsNullOrEmpty(folderpath))
            {
                System.Windows.MessageBox.Show("No Folder of Protocol Files Selected Yet!", "Error");
            }
            else if (!reportSelected)
            {
                System.Windows.MessageBox.Show("No Report Types Selected Yet!", "Error");
            }
            else if (String.IsNullOrEmpty(savepath))
            {
                System.Windows.MessageBox.Show("No Save Location Selected Yet!", "Error");
            }
            else
            {
                //all conditions are met, now we can generate the report(s)
                try
                {
                    if (isSingleDocument && numReports > 1)
                    {
                        //multiple reports are being created for one log file, so we should save these in one single .csv file
                        var allRecords = GenerateReport.Generate(selectedReports, logPaths, currentVersion, stimulusTypesToInclude);
                        using (var writer = new StreamWriter(savepath))
                        using (var csv = new CsvWriter(writer))
                        {
                            csv.Configuration.HasHeaderRecord = false;

                            for (int i = 0; i < allRecords.Count; i += 2)
                            {
                                //writing header
                                List<String> header = (List<String>)allRecords[i];
                                foreach (String heading in header)
                                {
                                    csv.WriteField(heading);
                                }
                                csv.NextRecord();

                                //writing data
                                List<List<String>> data = (List<List<String>>)allRecords[i + 1];
                                foreach (List<String> row in data)
                                {
                                    foreach (String entry in row)
                                    {
                                        csv.WriteField(entry);
                                    }
                                    csv.NextRecord();
                                }
                                csv.NextRecord();
                            }
                        }

                        System.Windows.MessageBox.Show("Report generated successfully!", "Success");
                        savepath = "";
                    }
                    else
                    {
                        //either multiple reports are being created for multiple log files, so each report should be saved into separate
                        //files for readability purposes - or, just one report is being created - both cases are handled here
                        foreach (String reportType in selectedReports)
                        {
                            List<String> singleReportType = new List<String>() { reportType };
                            String singleFilename = (numReports == 1) ? (savepath) : (FilenameHelper(stub, reportType));
                            var allRecords = GenerateReport.Generate(singleReportType, logPaths, currentVersion, stimulusTypesToInclude);
                            using (var writer = new StreamWriter(singleFilename))
                            using (var csv = new CsvWriter(writer))
                            {
                                csv.Configuration.HasHeaderRecord = false;

                                for (int i = 0; i < allRecords.Count; i += 2)
                                {
                                    //writing header
                                    List<String> header = (List<String>)allRecords[i];
                                    foreach (String heading in header)
                                    {
                                        csv.WriteField(heading);
                                    }
                                    csv.NextRecord();

                                    //writing data
                                    List<List<String>> data = (List<List<String>>)allRecords[i + 1];
                                    foreach (List<String> row in data)
                                    {
                                        foreach (String entry in row)
                                        {
                                            csv.WriteField(entry);
                                        }
                                        csv.NextRecord();
                                    }
                                    csv.NextRecord();
                                }
                            }
                        }

                        if (numReports > 1)
                        {
                            System.Windows.MessageBox.Show("All reports generated successfully!", "Success");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Report generated successfully!", "Success");
                        }
                        savepath = "";
                    }
                }

                catch (System.IO.IOException)
                {
                    System.Windows.MessageBox.Show("There was an error attempting to create a summary file - make sure that you are not" +
                        " saving to a read-only folder, and that the file isn't already open elsewhere (for example, if you're overwriting" +
                        " an existing summary file, it might still be open in Excel).", "Error");
                    return;
                }
            }
        }

        private void OpenSaveFolderWindow()
        {
            SavingFolderWindow saveWindow = new SavingFolderWindow();
            this.Fade.Visibility = Visibility.Visible;
            saveWindow.ShowDialog();

            stub = saveWindow.StubPrompt.Text;
            savepath = saveWindow.folderpath;
            this.Fade.Visibility = Visibility.Hidden;
        }

        private void StimTypeLabelHelper()
        {
            String reports = "";
            for (int i = 0; i < selectedReports.Count; i++)
            {
                if (StimulusSelectReports.Contains(selectedReports[i]))
                {
                    reports += ("\u2022" + selectedReports[i] + "\n");
                }
            }
            //reports = reports.TrimEnd(',');

            if (numStimSelectReports >=3 && numStimSelectReports <= 5)
            {
                this.StimulusTypesSomeNAWarning.Text = "Note: selections only apply to the following selected reports:\n" + reports + "Stimulus type selection is N/A for\nthe other selected reports";
            }
            else
            {
                this.StimulusTypesSomeNAWarning.Text = "Note: selections only apply to the following selected reports:\n" + reports + "Stimulus type selection is N/A for the other selected reports";
            }
        }

        private String FilenameHelper(String stub, String reportType)
        {
            String filename = (stub.Equals("")) ? (savepath + "\\") : (savepath + "\\" + stub);
            switch(reportType)
            {
                case "Listing of Played Media":
                    filename += "ListMedia";
                    break;
                case "Overall Looking Time (By Trial)":
                    filename += "LookingByTrial";
                    break;
                case "Number of Looks per Trial":
                    filename += "NumLooks";
                    break;
                case "Individual Looks By Trial":
                    filename += "IndividualLooks";
                    break;
                case "Summary Across Groups and Tags":
                    filename += "GroupsAndTagsSummary";
                    break;
                case "Habituation Information":
                    filename += "Habituation";
                    break;
                case "Header Information":
                    filename += "Header";
                    break;
                case "Overall Looking Information":
                    filename += "OverallLooking";
                    break;
                case "Summary Across Sides":
                    filename += "SidesSummary";
                    break;
                case "Detailed Looking Time":
                    filename += "DetailedLooking";
                    break;
                case "All Event Info":
                    filename += "AllEventInfo";
                    break;
                case "Custom Report":
                    filename += "Custom";
                    break;
                default:
                    break;
            }

            filename += ".csv";
            return filename;
        }
    }
}
