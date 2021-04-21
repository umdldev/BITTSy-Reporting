using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace BITTSyReporting
{
    static partial class GenerateReport
    {
        public static List<Object> Generate(List<String> reportTypes, List<String> logPaths, String currentVersion, List<String> stimulusTypesToInclude)
        {
            List<Object> allRecords = new List<Object>();

            List<String> startHeaders = new List<String>();
            List<List<String>> startInfo = new List<List<String>>();
            List<String> recordHeaders = new List<String>();
            List<List<String>> records = new List<List<String>>();

            foreach (String path in logPaths)
            {
                //getting the starting datetime and BITTSy version number here, because these should be 
                //included at the top of every report type
                String[] logText = File.ReadAllLines(path);
                String subjectID = Regex.Split(logText[5], "subject_names ")[1];
                String experimentDateTime = Regex.Split(logText[8], "test_date ")[1];
                String reportDateTime = DateTime.Now.ToString();
                String BITTSyVersionNum = Regex.Split(logText[1], "BITTSy version ")[1];
                String reportingVersionNum = currentVersion;
                String logFilePath = path;

                startHeaders = new List<String>() { "SubjectID", "ExperimentDateTime", "ReportDateTime", "BITTSyVersion", "ReportingVersion", "LogFilePath" };
                List<String> record = new List<String>();
                record.Add(subjectID);
                record.Add(experimentDateTime);
                record.Add(reportDateTime);
                record.Add(BITTSyVersionNum);
                record.Add(reportingVersionNum);
                record.Add(logFilePath);
                startInfo.Add(record);
            }

            allRecords.Add(startHeaders);
            allRecords.Add(startInfo);

            foreach (String reportType in reportTypes)
            {
                switch (reportType)
                {
                    case "Header Information":
                        (recordHeaders, records) = HeaderInfo(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Listing of Played Media":
                        (recordHeaders, records) = ListMedia(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Overall Looking Information":
                        (recordHeaders, records) = OverallLooking(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Overall Looking Time (By Trial)":
                        (recordHeaders, records) = LookingByTrial(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Number of Looks per Trial":
                        (recordHeaders, records) = NumberOfLooks(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Individual Looks By Trial":
                        (recordHeaders, records) = IndividualLooksByTrial(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Summary Across Sides":
                        (recordHeaders, records) = SummaryAcrossSides(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Summary Across Groups and Tags":
                        (recordHeaders, records) = SummaryAcrossGroupsAndTags(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Detailed Looking Time":
                        (recordHeaders, records) = DetailedLooking(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Habituation Information":
                        //the habituation summary outputs multiple "tables" of data in one report, unlike the other types which just
                        //have one, and therefore is handled a little differently here - if you want your custom report to also have more
                        //than one table, note that each table in the Habituation reports has one header and at least one entry, so as long 
                        //as these are List<String> and List<List<String>> respectively, we can still use MainWindow's Generate_Click logic

                        //In other words, your custom summary types can have multiple tables as long as each has a List<String> header and
                        //List<List<String>> data, and you must add these to allRecords in the order of header, data, header, data, etc.
                        List<String> habitReqHeaders = new List<String>();
                        List<List<String>> habitReqs = new List<List<String>>();
                        List<String> wasHabitMetHeader = new List<String>();
                        List<List<String>> wasHabitMet = new List<List<String>>();
                        (habitReqHeaders, habitReqs, wasHabitMetHeader, wasHabitMet, recordHeaders, records) = Habituation(logPaths, stimulusTypesToInclude);

                        allRecords.Add(habitReqHeaders);
                        allRecords.Add(habitReqs);
                        allRecords.Add(wasHabitMetHeader);
                        allRecords.Add(wasHabitMet);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "All Event Info":
                        (recordHeaders, records) = Everything(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    case "Custom Report":
                        (recordHeaders, records) = Custom(logPaths, stimulusTypesToInclude);
                        allRecords.Add(recordHeaders);
                        allRecords.Add(records);
                        break;
                    default:
                        return null;
                }
            }

            return allRecords;
        }

        private static (List<String>, List<List<String>>) HeaderInfo(List<String> logPaths, List<String> stimulusTypesToInclude)
        {
            List<String> recordHeaders = new List<String>() { "SubjectID", "SubjectDOB", "Experimenter", "TestDate", "ProtocolName", "Comments" };
            List<List<String>> records = new List<List<String>>();

            foreach (String logFilePath in logPaths)
            {
                String[] logText = File.ReadAllLines(logFilePath);
                bool completed = false;
                int line = 0;
                while (!completed)
                {
                    if (logText[line].Contains("experiment information"))
                    {
                        String protoName = Regex.Split(logText[++line], "protocol_name ")[1];
                        String subjName = Regex.Split(logText[++line], "subject_names ")[1];
                        String subjDOB = Regex.Split(logText[++line], "subject_dob ")[1];
                        String tester = Regex.Split(logText[++line], "experimenter_name ")[1];
                        //String date = "\"" + Regex.Split(logText[++line], "test_date ")[1] + "\"";
                        String date = Regex.Split(logText[++line], "test_date ")[1];
                        String expComments = Regex.Split(logText[++line], "comments: ")[1];

                        List<String> record = new List<String>() { subjName, subjDOB, tester, date, protoName, expComments } ;
                        records.Add(record);

                        completed = true;
                    }
                    else
                    {
                        line++;
                        if (line >= logText.Length)
                        {
                            System.Windows.MessageBox.Show("Error in generating Header Information: no such information exists in the following log file: " + logFilePath, "Error");
                            break;
                        }
                    }
                }
                completed = false;
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) ListMedia(List<String> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = logPaths.Count > 1;
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "Trial", "Type", "Tag", "Side" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "Trial", "Type", "Tag", "Side" };
            }
            List<List<String>> records = new List<List<String>>();

            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                String[] logText = File.ReadAllLines(logFilePath);
                String subjName = "";
                String phaseName = "";
                String trialNum = "";
                bool inTrial = false;
                foreach (String line in logText)
                {
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "trial (.+) started");
                        trialNum = match.Groups[1].Captures[0].ToString();
                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        trialNum = "";
                        inTrial = false;
                    }
                    if (line.Contains("stimuli") && !line.Contains("off") && inTrial)
                    {
                        String type = "";
                        String side = "";
                        String tag = "";
                        Match match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                                type = "light_" + match.Groups[2].Captures[0].ToString();
                                side = match.Groups[4].Captures[0].ToString();
                                tag = "no_tag";
                            }
                            else
                            {
                                type = "light_" + match.Groups[2].Captures[0].ToString();
                                side = match.Groups[3].Captures[0].ToString();
                                tag = "no_tag";
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            type = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[3].Captures[0].ToString();
                            tag = match.Groups[4].Captures[0].ToString();
                        }
                        else
                        {
                            type = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[2].Captures[0].ToString();
                            tag = match.Groups[3].Captures[0].ToString();
                        }

                        if (stimulusTypesToInclude.Contains(type) || (type.Contains("light") && stimulusTypesToInclude.Contains("light")))
                        {
                            List<String> record = new List<String>();

                            if (multipleLogs)
                            {
                                record.Add(subjName);
                            }
                            record.Add(phaseName);
                            record.Add(trialNum);
                            record.Add(type);
                            record.Add(tag);
                            record.Add(side);

                            records.Add(record);
                        }
                    }
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) OverallLooking(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "MeanLookingTime", "TotalLookingTime", "MeanOrientTime" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "MeanLookingTime", "TotalLookingTime", "MeanOrientTime" };
            }
            List<List<String>> records = new List<List<String>>();

            Dictionary<String, List<double>> allMeanLooks = new Dictionary<String, List<double>>();
            Dictionary<String, List<double>> allTotalLooks = new Dictionary<String, List<double>>();
            Dictionary<String, List<double>> allMeanOrients = new Dictionary<String, List<double>>();
            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                bool inTrial = false;
                bool lookAlreadyLogged = false;
                String subjName = "";
                String phaseName = "";
                double meanLook = 0.0;
                double totalLook = 0.0;
                int numLooks = 0;
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                Dictionary<String, String> activeStimuliOnTimes = new Dictionary<String, String>();
                Dictionary<String, double> stimuliOrientTimes = new Dictionary<String, double>();
                Dictionary<String, bool> stimuliOriented = new Dictionary<String, bool>();
                String[,] keyPressTimes = new String[3, 2] { { "SPACE", "0" }, { "SPACE", "0" }, { "SPACE", "0" } };
                bool lookAutoHandled = false;
                double totalOrientationTime = 0.0;
                int numOrients = 0;
                int lineCounter = 0;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (lookAlreadyLogged && (!line.Contains("duration") || line.Contains("Debug")))
                    {
                        //we were dealing with multiple look-duration lines that corresponded to a single
                        //look - now we're done with that, so reset lookAlreadyLogged to false
                        lookAlreadyLogged = false;
                    }
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if ((line.Contains("phase") && line.Contains("ended")) || (line.Contains("Halted Prematurely") && !phaseName.Equals("")))
                    {
                        //in the first case the phase we were evaluating has ended, and in the second case the experiment was halted before
                        //the phase could end - in either case, we want to now add the information we've retrieved for this phase, and then
                        //reset fields for the next phase
                        meanLook = numLooks > 0 ? (totalLook / numLooks) : (0.0);
                        double meanOrient = numOrients > 0 ? (totalOrientationTime / numOrients) : (0.0);

                        List<String> record = new List<String>();
                        if (multipleLogs)
                        {
                            record.Add(subjName);
                        }
                        record.Add(phaseName);
                        record.Add(meanLook.ToString());
                        record.Add(totalLook.ToString());
                        record.Add(meanOrient.ToString());

                        records.Add(record);

                        if (multipleLogs)
                        {
                            //we need to store this information so that we can calculate averages across all subjects
                            //after all the logs are finished processing
                            if (!allMeanLooks.ContainsKey(phaseName))
                            {
                                allMeanLooks.Add(phaseName, new List<double>());
                            }
                            if (!allTotalLooks.ContainsKey(phaseName))
                            {
                                allTotalLooks.Add(phaseName, new List<double>());
                            }
                            if (!allMeanOrients.ContainsKey(phaseName))
                            {
                                allMeanOrients.Add(phaseName, new List<double>());
                            }

                            allMeanLooks[phaseName].Add(meanLook);
                            allTotalLooks[phaseName].Add(totalLook);
                            allMeanOrients[phaseName].Add(meanOrient);
                        }

                        meanLook = 0.0;
                        totalLook = 0.0;
                        numLooks = 0;
                        totalOrientationTime = 0.0;
                        numOrients = 0;
                    }

                    else if (line.Contains("stimuli") && !line.Contains("off"))
                    {
                        String stimulusTag = "";
                        String type = "";
                        String onTime = "";
                        String side = "";

                        //the reason for recording onTime is so that once we later record an offTime, we can then report the total 
                        //time that the stimulus was playing for, by subtracting the time elapsed when ON from the time elapsed when OFF
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                            else
                            {
                                match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                        activeStimuli.Add(side + "|" + type, stimulusTag);
                        activeStimuliOnTimes.Add(side + "|" + type, onTime);
                        stimuliOrientTimes.Add(side + "|" + type, 0.0);
                        stimuliOriented.Add(side + "|" + type, false);
                    }
                    else if (line.Contains("stimuli") && line.Contains("off"))
                    {
                        String side = "";
                        String type = "";

                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        type = match.Groups[2].Captures[0].ToString();
                        side = match.Groups[3].Captures[0].ToString();

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "trial (.+) started");
                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;
                        lookAutoHandled = false;
                    }
                    
                    else if (line.Contains("duration") && !line.Contains("in_progress") && !line.Contains("Debug") && inTrial)
                    {
                        //first check to see if this is a partial look, i.e. if there was an incomplete lookaway that follows this look and
                        //precedes the next one, as an incomplete lookaway would result in those two looks being combined - if this is the case,
                        //we want to avoid handling this partial look because it will be handled as part of the final, combined look later
                        bool isPartialLook = false;
                        int currCounter = lineCounter + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        Match match = Regex.Match(line, @"look (\S+) (\S+) (\S+) duration (\S+)");
                        String side = match.Groups[2].Captures[0].ToString();
                        String type = match.Groups[1].Captures[0].ToString();
                        String stimulusTag = match.Groups[3].Captures[0].ToString();
                        if (stimulusTag.Equals("no_tag"))
                        {
                            type = "light";
                            stimulusTag = activeStimuli[side + "|" + type];
                        }
                        if (type.Equals("display"))
                        {
                            //figure out whether this display stimulus was a video or image
                            type = (activeStimuli.ContainsKey(side + "|" + "video")) ? "video" : "image";
                        }

                        //calculating orientation time
                        String onTime = "";
                        double orientTime = 0.0;
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        if (isPartialLook)
                        {
                            String keyTime = keyPressTimes[0, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else if (lookAutoHandled)
                        {
                            String keyTime = keyPressTimes[2, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else
                        {
                            String keyTime = keyPressTimes[1, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        if (orientTime < 0.0)
                        {
                            //the subject was already looking towards this direction when the stimulus turned on, so orientation time
                            //should just be 0 instead of its current negative value
                            orientTime = 0.0;
                        }
                        if (!stimuliOriented[side + "|" + type])
                        {
                            totalOrientationTime += orientTime;
                            numOrients++;
                            stimuliOriented[side + "|" + type] = true;
                        }

                        //adding look, but only if it isn't a partial one
                        if (!isPartialLook)
                        {
                            int duration = 0;
                            duration = Int32.Parse(match.Groups[4].Captures[0].ToString());
                            if (!lookAlreadyLogged)
                            {
                                totalLook += duration;
                            }

                            if (!lookAlreadyLogged)
                            {
                                lookAutoHandled = false;
                                lookAlreadyLogged = true;

                                numLooks++;
                            }
                        }
                    }

                    else if (line.Contains("Debug: ") && line.Contains("pressed"))
                    {
                        keyPressTimes[0, 0] = keyPressTimes[1, 0];
                        keyPressTimes[0, 1] = keyPressTimes[1, 1];

                        keyPressTimes[1, 0] = keyPressTimes[2, 0];
                        keyPressTimes[1, 1] = keyPressTimes[2, 1];

                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]Debug: (\S+) pressed");
                        keyPressTimes[2, 0] = match.Groups[2].Captures[0].ToString();
                        keyPressTimes[2, 1] = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("being handled automatically at end of trial"))
                    {
                        lookAutoHandled = true;
                    }

                    lineCounter++;
                }
                if (phaseName.Equals(""))
                {
                    meanLook = numLooks > 0 ? (totalLook / numLooks) : (0.0);
                    double meanOrient = numOrients > 0 ? (totalOrientationTime / numOrients) : (0.0);

                    List<String> record = new List<String>();
                    if (multipleLogs)
                    {
                        record.Add(subjName);
                    }
                    record.Add(phaseName);
                    record.Add(meanLook.ToString());
                    record.Add(totalLook.ToString());
                    record.Add(meanOrient.ToString());

                    records.Add(record);

                    if (multipleLogs)
                    {
                        //we need to store this information so that we can calculate averages across all subjects
                        //after all the logs are finished processing
                        if (!allMeanLooks.ContainsKey(phaseName))
                        {
                            allMeanLooks.Add(phaseName, new List<double>());
                        }
                        if (!allTotalLooks.ContainsKey(phaseName))
                        {
                            allTotalLooks.Add(phaseName, new List<double>());
                        }
                        if (!allMeanOrients.ContainsKey(phaseName))
                        {
                            allTotalLooks.Add(phaseName, new List<double>());
                        }

                        allMeanLooks[phaseName].Add(meanLook);
                        allTotalLooks[phaseName].Add(totalLook);
                        allMeanOrients[phaseName].Add(meanOrient);
                    }

                    meanLook = 0.0;
                    totalLook = 0.0;
                    numLooks = 0;
                    totalOrientationTime = 0.0;
                    numOrients = 0;
                }
            }

            //check if we need to include averages across subjects (i.e. check if multiple subjects)
            if (multipleLogs)
            {
                foreach (String phase in allMeanLooks.Keys)
                {
                    List<String> record = new List<String>();

                    double overallMeanLook = allMeanLooks[phase].Sum() / allMeanLooks[phase].Count();
                    double overallTotalLook = allTotalLooks[phase].Sum() / allTotalLooks[phase].Count();
                    double overallMeanOrient = allMeanOrients[phase].Sum() / allMeanOrients[phase].Count();

                    record.Add("(Average)");
                    record.Add(phase);
                    record.Add(overallMeanLook.ToString());
                    record.Add(overallTotalLook.ToString());
                    record.Add(overallMeanOrient.ToString());

                    records.Add(record);
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) LookingByTrial(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "TrialNum", "Stimulus", "Group", "Side", "TimeToOrient", "TotalPlayingTime", "TotalLookingTime" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "TrialNum", "Stimulus", "Group", "Side", "TimeToOrient", "TotalPlayingTime", "TotalLookingTime" };
            }
            List<List<String>> records = new List<List<String>>();

            Dictionary<String, Dictionary<String, List<double>>> allOrientTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allTotalPlayingTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allTotalLookingTimes = new Dictionary<String, Dictionary<String, List<double>>>();

            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                bool inTrial = false;
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                Dictionary<String, String> activeStimuliOnTimes = new Dictionary<String, String>();
                Dictionary<String, String> stimuliGroups = new Dictionary<String, String>();
                Dictionary<String, int> stimuliTotalLooks = new Dictionary<String, int>();
                Dictionary<String, double> stimuliOrientTimes = new Dictionary<String, double>();
                Dictionary<String, bool> stimuliOriented = new Dictionary<String, bool>();
                String[,] keyPressTimes = new String[3, 2] { {"SPACE", "0"}, {"SPACE", "0"}, {"SPACE", "0"} };
                bool lookAutoHandled = false;
                String phaseName = "";
                String subjName = "";
                String trialNum = "";
                int lineCounter = 0;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("from group") && line.Contains("Tag") && !line.Contains("SDL"))
                    {
                        Match match = null;
                        if (line.Contains("taken"))
                        {
                            match = Regex.Match(line, @"Tag (\S+) taken from group (\S+)");
                        }
                        else
                        {
                            match = Regex.Match(line, @"Tag (\S+) from group (\S+)");
                        }

                        String group = match.Groups[2].Captures[0].ToString();
                        String tag = match.Groups[1].Captures[0].ToString();
                        if (stimuliGroups.ContainsKey(tag))
                        {
                            stimuliGroups.Remove(tag);
                        }
                        stimuliGroups.Add(tag, group);
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("stimuli") && !line.Contains("off"))
                    {
                        String stimulusTag = "";
                        String type = "";
                        String onTime = "";
                        String side = "";

                        //the reason for recording onTime is so that once we later record an offTime, we can then report the total 
                        //time that the stimulus was playing for, by subtracting the time elapsed when ON from the time elapsed when OFF
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                            else
                            {
                                match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                        activeStimuli.Add(side + "|" + type, stimulusTag);
                        activeStimuliOnTimes.Add(side + "|" + type, onTime);
                        stimuliOrientTimes.Add(side + "|" + type, 0.0);
                        stimuliOriented.Add(side + "|" + type, false);

                        if (!stimuliTotalLooks.ContainsKey(stimulusTag))
                        {
                            stimuliTotalLooks.Add(stimulusTag, 0);
                        }
                    }
                    else if (line.Contains("stimuli") && line.Contains("off"))
                    {
                        String onTime = "";
                        String offTime = "";
                        String side = "";
                        String type = "";
                        String stimulusTag = "";
                        String group = "";
                        int totalLook = 0;

                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        type = match.Groups[2].Captures[0].ToString();
                        side = match.Groups[3].Captures[0].ToString();
                        offTime = match.Groups[1].Captures[0].ToString();
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        stimulusTag = activeStimuli[side + "|" + type];
                        group = stimuliGroups.ContainsKey(stimulusTag) ? stimuliGroups[stimulusTag] : "";
                        totalLook = stimuliTotalLooks[stimulusTag];

                        //now that we have an offTime, we can calculate the total time that the stimulus was playing/presented for
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime off = DateTime.ParseExact(offTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double totalPlay = off.Subtract(on).TotalMilliseconds;

                        double orientTime = stimuliOrientTimes[side + "|" + type];

                        if (totalLook > 0.0 && stimulusTypesToInclude.Contains(type))
                        {
                            List<String> record = new List<String>();
                            if (multipleLogs)
                            {
                                record.Add(subjName);
                            }
                            record.Add(phaseName);
                            record.Add(trialNum);
                            record.Add(stimulusTag);
                            record.Add(group);
                            record.Add(side);
                            record.Add(orientTime.ToString());
                            record.Add(totalPlay.ToString());
                            record.Add(totalLook.ToString());

                            records.Add(record);

                            if (multipleLogs)
                            {
                                //we need to store this information so that we can calculate averages across all subjects
                                //after all the logs are finished processing
                                if (!allOrientTimes.ContainsKey(phaseName))
                                {
                                    allOrientTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allTotalPlayingTimes.ContainsKey(phaseName))
                                {
                                    allTotalPlayingTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allTotalLookingTimes.ContainsKey(phaseName))
                                {
                                    allTotalLookingTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }

                                if (!allOrientTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allOrientTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allTotalPlayingTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allTotalPlayingTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allTotalLookingTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allTotalLookingTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                allOrientTimes[phaseName][trialNum].Add(orientTime);
                                allTotalPlayingTimes[phaseName][trialNum].Add(totalPlay);
                                allTotalLookingTimes[phaseName][trialNum].Add(totalLook);
                            }
                        }

                        stimuliTotalLooks[stimulusTag] = 0;

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "trial (.+) started");
                        trialNum = match.Groups[1].Captures[0].ToString();

                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;
                        lookAutoHandled = false;
                    }
                    else if (line.Contains("duration") && !line.Contains("in_progress") && !line.Contains("Debug") && inTrial)
                    {
                        bool isPartialLook = false;
                        int currCounter = lineCounter + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        Match match = Regex.Match(line, @"look (\S+) (\S+) (\S+) duration (\S+)");
                        String side = match.Groups[2].Captures[0].ToString();
                        String type = match.Groups[1].Captures[0].ToString();
                        String stimulusTag = match.Groups[3].Captures[0].ToString();

                        if (stimulusTag.Equals("no_tag"))
                        {
                            type = "light";
                            stimulusTag = activeStimuli[side + "|" + type];
                        }
                        if (type.Equals("display"))
                        {
                            //figure out whether this display stimulus was a video or image
                            type = (activeStimuli.ContainsKey(side + "|" + "video")) ? "video" : "image";
                        }

                        if (!isPartialLook)
                        {
                            int lookTime = Int32.Parse(match.Groups[4].Captures[0].ToString());
                            stimuliTotalLooks[stimulusTag] += lookTime;

                            //lookAutoHandled = false;
                        }

                        //calculating orientation time
                        String onTime = "";
                        double orientTime = 0.0;
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        if (isPartialLook)
                        {
                            String keyTime = keyPressTimes[0, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else if (lookAutoHandled)
                        {
                            String keyTime = keyPressTimes[2, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else
                        {
                            String keyTime = keyPressTimes[1, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        if (orientTime < 0.0)
                        {
                            orientTime = 0.0;
                        }
                        if (!stimuliOriented[side + "|" + type])
                        {
                            stimuliOrientTimes[side + "|" + type] = orientTime;
                            stimuliOriented[side + "|" + type] = true;
                        }
                    }
                    else if (line.Contains("Debug: ") && line.Contains("pressed"))
                    {
                        keyPressTimes[0, 0] = keyPressTimes[1, 0];
                        keyPressTimes[0, 1] = keyPressTimes[1, 1];

                        keyPressTimes[1, 0] = keyPressTimes[2, 0];
                        keyPressTimes[1, 1] = keyPressTimes[2, 1];

                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]Debug: (\S+) pressed");
                        keyPressTimes[2, 0] = match.Groups[2].Captures[0].ToString();
                        keyPressTimes[2, 1] = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("being handled automatically at end of trial"))
                    {
                        lookAutoHandled = true;
                    }

                    lineCounter++;
                }
            }

            //check if we need to include averages across subjects (i.e. check if multiple subjects)
            if (multipleLogs)
            {
                foreach (String phase in allOrientTimes.Keys)
                {
                    foreach (String trialNum in allOrientTimes[phase].Keys)
                    {
                        List<String> record = new List<String>();

                        double overallOrientTime = allOrientTimes[phase][trialNum].Sum() / allOrientTimes[phase][trialNum].Count();
                        double overallPlayingTime = allTotalPlayingTimes[phase][trialNum].Sum() / allTotalPlayingTimes[phase][trialNum].Count();
                        double overallLookingTime = allTotalLookingTimes[phase][trialNum].Sum() / allTotalLookingTimes[phase][trialNum].Count();

                        record.Add("(Average)");
                        record.Add(phase);
                        record.Add(trialNum);
                        record.Add("");
                        record.Add("");
                        record.Add("");
                        record.Add(overallOrientTime.ToString());
                        record.Add(overallPlayingTime.ToString());
                        record.Add(overallLookingTime.ToString());

                        records.Add(record);
                    }
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) NumberOfLooks(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "TrialNum", "Stimulus", "Group", "Side", "TimeToOrient", "TotalPlayingTime", "TotalLookingTime", "NumLooks" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "TrialNum", "Stimulus", "Group", "Side", "TimeToOrient", "TotalPlayingTime", "TotalLookingTime", "NumLooks" };
            }
            List<List<String>> records = new List<List<String>>();

            Dictionary<String, Dictionary<String, List<double>>> allOrientTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allTotalPlayingTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allTotalLookingTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allNumLooks = new Dictionary<String, Dictionary<String, List<double>>>();

            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                bool inTrial = false;
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                Dictionary<String, String> activeStimuliOnTimes = new Dictionary<String, String>();
                Dictionary<String, String> stimuliGroups = new Dictionary<String, String>();
                Dictionary<String, int> stimuliTotalLooks = new Dictionary<String, int>();
                Dictionary<String, int> stimuliNumLooks = new Dictionary<String, int>();
                Dictionary<String, double> stimuliOrientTimes = new Dictionary<String, double>();
                Dictionary<String, bool> stimuliOriented = new Dictionary<String, bool>();
                String[,] keyPressTimes = new String[3, 2] { { "SPACE", "0" }, { "SPACE", "0" }, { "SPACE", "0" } };
                bool lookAutoHandled = false;
                String phaseName = "";
                String subjName = "";
                String trialNum = "";
                int lineCounter = 0;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("from group") && line.Contains("Tag") && !line.Contains("SDL"))
                    {
                        Match match = null;
                        if (line.Contains("taken"))
                        {
                            match = Regex.Match(line, @"Tag (\S+) taken from group (\S+)");
                        }
                        else
                        {
                            match = Regex.Match(line, @"Tag (\S+) from group (\S+)");
                        }

                        String group = match.Groups[2].Captures[0].ToString();
                        String tag = match.Groups[1].Captures[0].ToString();
                        if (stimuliGroups.ContainsKey(tag))
                        {
                            stimuliGroups.Remove(tag);
                        }
                        stimuliGroups.Add(tag, group);
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("stimuli") && !line.Contains("off"))
                    {
                        String stimulusTag = "";
                        String type = "";
                        String onTime = "";
                        String side = "";

                        //the reason for recording onTime is so that once we later record an offTime, we can then report the total 
                        //time that the stimulus was playing for, by subtracting the time elapsed when ON from the time elapsed when OFF
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                            else
                            {
                                match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                        activeStimuli.Add(side + "|" + type, stimulusTag);
                        activeStimuliOnTimes.Add(side + "|" + type, onTime);
                        stimuliOrientTimes.Add(side + "|" + type, 0.0);
                        stimuliOriented.Add(side + "|" + type, false);

                        if (!stimuliTotalLooks.ContainsKey(stimulusTag))
                        {
                            stimuliTotalLooks.Add(stimulusTag, 0);
                        }
                        if (!stimuliNumLooks.ContainsKey(stimulusTag))
                        {
                            stimuliNumLooks.Add(stimulusTag, 0);
                        }
                    }
                    else if (line.Contains("stimuli") && line.Contains("off"))
                    {
                        String onTime = "";
                        String offTime = "";
                        String side = "";
                        String type = "";
                        String stimulusTag = "";
                        String group = "";
                        int totalLook = 0;
                        int numLooks = 0;
                        //double orientTime = 0.0;

                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        type = match.Groups[2].Captures[0].ToString();
                        side = match.Groups[3].Captures[0].ToString();
                        offTime = match.Groups[1].Captures[0].ToString();
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        stimulusTag = activeStimuli[side + "|" + type];
                        group = stimuliGroups.ContainsKey(stimulusTag) ? stimuliGroups[stimulusTag] : "";
                        totalLook = stimuliTotalLooks[stimulusTag];
                        numLooks = stimuliNumLooks[stimulusTag];

                        //now that we have an offTime, we can calculate the total time that the stimulus was playing/presented for
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime off = DateTime.ParseExact(offTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double totalPlay = off.Subtract(on).TotalMilliseconds;

                        double orientTime = stimuliOrientTimes[side + "|" + type];

                        if (totalLook > 0.0 && stimulusTypesToInclude.Contains(type))
                        {
                            List<String> record = new List<String>();
                            if (multipleLogs)
                            {
                                record.Add(subjName);
                            }
                            record.Add(phaseName);
                            record.Add(trialNum);
                            record.Add(stimulusTag);
                            record.Add(group);
                            record.Add(side);
                            record.Add(orientTime.ToString());
                            record.Add(totalPlay.ToString());
                            record.Add(totalLook.ToString());
                            record.Add(numLooks.ToString());

                            records.Add(record);

                            if (multipleLogs)
                            {
                                //we need to store this information so that we can calculate averages across all subjects
                                //after all the logs are finished processing

                                if (!allOrientTimes.ContainsKey(phaseName))
                                {
                                    allOrientTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allTotalPlayingTimes.ContainsKey(phaseName))
                                {
                                    allTotalPlayingTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allTotalLookingTimes.ContainsKey(phaseName))
                                {
                                    allTotalLookingTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allNumLooks.ContainsKey(phaseName))
                                {
                                    allNumLooks.Add(phaseName, new Dictionary<String, List<double>>());
                                }

                                if (!allOrientTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allOrientTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allTotalPlayingTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allTotalPlayingTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allTotalLookingTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allTotalLookingTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allNumLooks[phaseName].ContainsKey(trialNum))
                                {
                                    allNumLooks[phaseName].Add(trialNum, new List<double>());
                                }
                                allOrientTimes[phaseName][trialNum].Add(orientTime);
                                allTotalPlayingTimes[phaseName][trialNum].Add(totalPlay);
                                allTotalLookingTimes[phaseName][trialNum].Add(totalLook);
                                allNumLooks[phaseName][trialNum].Add(numLooks);
                            }
                        }

                        stimuliTotalLooks[stimulusTag] = 0;
                        stimuliNumLooks[stimulusTag] = 0;

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "trial (.+) started");
                        trialNum = match.Groups[1].Captures[0].ToString();

                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;
                        lookAutoHandled = false;
                    }
                    else if (line.Contains("duration") && !line.Contains("in_progress") && !line.Contains("Debug") && inTrial)
                    {
                        bool isPartialLook = false;
                        int currCounter = lineCounter + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        Match match = Regex.Match(line, @"look (\S+) (\S+) (\S+) duration (\S+)");
                        String side = match.Groups[2].Captures[0].ToString();
                        String type = match.Groups[1].Captures[0].ToString();
                        String stimulusTag = match.Groups[3].Captures[0].ToString();
                        if (stimulusTag.Equals("no_tag"))
                        {
                            type = "light";
                            stimulusTag = activeStimuli[side + "|" + type];
                        }
                        if (type.Equals("display"))
                        {
                            //figure out whether this display stimulus was a video or image
                            type = (activeStimuli.ContainsKey(side + "|" + "video")) ? "video" : "image";
                        }

                        //calculating orientation time
                        String onTime = "";
                        double orientTime = 0.0;
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        if (isPartialLook)
                        {
                            String keyTime = keyPressTimes[0, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else if (lookAutoHandled)
                        {
                            String keyTime = keyPressTimes[2, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else
                        {
                            String keyTime = keyPressTimes[1, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        if (orientTime < 0.0)
                        {
                            orientTime = 0.0;
                        }
                        if (!stimuliOriented[side + "|" + type])
                        {
                            stimuliOrientTimes[side + "|" + type] = orientTime;
                            stimuliOriented[side + "|" + type] = true;
                        }

                        if (!isPartialLook)
                        {
                            int lookTime = Int32.Parse(match.Groups[4].Captures[0].ToString());
                            stimuliTotalLooks[stimulusTag] += lookTime;

                            stimuliNumLooks[stimulusTag]++;
                        }
                    }
                    else if (line.Contains("Debug: ") && line.Contains("pressed"))
                    {
                        keyPressTimes[0, 0] = keyPressTimes[1, 0];
                        keyPressTimes[0, 1] = keyPressTimes[1, 1];

                        keyPressTimes[1, 0] = keyPressTimes[2, 0];
                        keyPressTimes[1, 1] = keyPressTimes[2, 1];

                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]Debug: (\S+) pressed");
                        keyPressTimes[2, 0] = match.Groups[2].Captures[0].ToString();
                        keyPressTimes[2, 1] = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("being handled automatically at end of trial"))
                    {
                        lookAutoHandled = true;
                    }

                    lineCounter++;
                }
            }

            //check if we need to include averages across subjects (i.e. check if multiple subjects)
            if (multipleLogs)
            {
                foreach (String phase in allOrientTimes.Keys)
                {
                    foreach (String trialNum in allOrientTimes[phase].Keys)
                    {
                        List<String> record = new List<String>();

                        double overallOrientTime = allOrientTimes[phase][trialNum].Sum() / allOrientTimes[phase][trialNum].Count();
                        double overallPlayingTime = allTotalPlayingTimes[phase][trialNum].Sum() / allTotalPlayingTimes[phase][trialNum].Count();
                        double overallLookingTime = allTotalLookingTimes[phase][trialNum].Sum() / allTotalLookingTimes[phase][trialNum].Count();
                        double overallNumLooks = allNumLooks[phase][trialNum].Sum() / allNumLooks[phase][trialNum].Count();

                        record.Add("(Average)");
                        record.Add(phase);
                        record.Add(trialNum);
                        record.Add("");
                        record.Add("");
                        record.Add("");
                        record.Add(overallOrientTime.ToString());
                        record.Add(overallPlayingTime.ToString());
                        record.Add(overallLookingTime.ToString());
                        record.Add(overallNumLooks.ToString());

                        records.Add(record);
                    }
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) IndividualLooksByTrial(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "Stimulus", "Group", "TrialNum", "LookingTime" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "Stimulus", "Group", "TrialNum", "LookingTime" };
            }
            List<List<String>> records = new List<List<String>>();

            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                bool inTrial = false;

                String phaseName = "";
                String subjName = "";
                String trialNum = "";
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                Dictionary<String, String> stimuliGroups = new Dictionary<String, String>();
                Dictionary<String, String> stimuliTypes = new Dictionary<String, String>();
                int lineCounter = 0;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    else if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("stimuli") && !line.Contains("off"))
                    {
                        String stimulusTag = "";
                        String side = "";
                        String type = "";
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                            else
                            {
                                match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }

                        if (!activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Add(side + "|" + type, stimulusTag);
                        }

                        if (stimuliTypes.ContainsKey(stimulusTag))
                        {
                            stimuliTypes.Remove(stimulusTag);
                        }
                        stimuliTypes.Add(stimulusTag, type);
                    }
                    else if (line.Contains("stimuli") && line.Contains("off"))
                    {
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        String type = match.Groups[2].Captures[0].ToString();
                        String side = match.Groups[3].Captures[0].ToString();

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                    }
                    else if (line.Contains("from group") && line.Contains("Tag") && !line.Contains("SDL"))
                    {
                        Match match = null;
                        if (line.Contains("taken"))
                        {
                            match = Regex.Match(line, @"Tag (\S+) taken from group (\S+)");
                        }
                        else
                        {
                            match = Regex.Match(line, @"Tag (\S+) from group (\S+)");
                        }

                        String group = match.Groups[2].Captures[0].ToString();
                        String tag = match.Groups[1].Captures[0].ToString();
                        if (stimuliGroups.ContainsKey(tag))
                        {
                            stimuliGroups.Remove(tag);
                        }
                        stimuliGroups.Add(tag, group);
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "trial (.+) started");
                        trialNum = match.Groups[1].Captures[0].ToString();

                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;
                    }
                    else if (line.Contains("duration") && !line.Contains("Debug") && inTrial)
                    {
                        bool isPartialLook = false;
                        int currCounter = lineCounter + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        if (!isPartialLook)
                        {
                            Match match = Regex.Match(line, @"look (\S+) (\S+) (\S+) duration (\S+)");
                            String stimulusTag = "";
                            String look = "";
                            String group = "";
                            if (line.Contains("in_progress"))
                            {
                                look = "in_progress";
                                stimulusTag = match.Groups[3].Captures[0].ToString();
                                if (stimulusTag.Equals("no_tag"))
                                {
                                    String side = match.Groups[2].Captures[0].ToString();
                                    stimulusTag = activeStimuli[side + "|" + "light"];
                                }
                                if (stimuliGroups.ContainsKey(stimulusTag))
                                {
                                    group = stimuliGroups[stimulusTag];
                                }
                            }
                            else
                            {
                                look = match.Groups[4].Captures[0].ToString();
                                int lookTime = Int32.Parse(look);
                                stimulusTag = match.Groups[3].Captures[0].ToString();

                                if (stimulusTag.Equals("no_tag"))
                                {
                                    String side = match.Groups[2].Captures[0].ToString();
                                    stimulusTag = activeStimuli[side + "|" + "light"];
                                }

                                if (stimuliGroups.ContainsKey(stimulusTag))
                                {
                                    group = stimuliGroups[stimulusTag];
                                }
                            }

                            String type = stimuliTypes[stimulusTag];
                            if (stimulusTypesToInclude.Contains(type))
                            {
                                List<String> record = new List<String>();
                                if (multipleLogs)
                                {
                                    record.Add(subjName);
                                }
                                record.Add(phaseName);
                                record.Add(stimulusTag);
                                record.Add(group);
                                record.Add(trialNum);
                                record.Add(look);

                                records.Add(record);
                            }
                        }
                    }

                    lineCounter++;
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) SummaryAcrossSides(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = new List<String>();
            List<List<String>> records = new List<List<String>>();

            Dictionary<String, Dictionary<String, List<double>>> allValues = new Dictionary<String, Dictionary<String, List<double>>>();
            List<String> subjectsWhoDidntFinish = new List<String>();

            //before doing the actual processing of the log(s), we need to do some pre-processing to create an overall list of all the sides
            //used, because this is necessary information for creating the header
            List<String> sides = new List<String>();
            foreach (String logFilePath in logPaths)
            {
                String[] logText = File.ReadAllLines(logFilePath);
                bool inTrial = false;
                foreach (String line in logText)
                {
                    if (line.Contains("stimuli") && !line.Contains("off") && inTrial)
                    {
                        String side = "";
                        Match match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                side = match.Groups[4].Captures[0].ToString();
                            }
                            else
                            {
                                match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+)");
                                side = match.Groups[3].Captures[0].ToString();
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            side = match.Groups[3].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+)");
                            side = match.Groups[2].Captures[0].ToString();
                        }

                        if (!sides.Contains(side))
                        {
                            sides.Add(side);
                        }
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;
                    }
                }
            }

            foreach (String logFilePath in logPaths)
            {
                bool inTrial = false;
                bool lookAlreadyLogged = false;
                //bool firstLookAtSideInTrial = true;
                List<String> sidesLookedAtInTrial = new List<String>();
                String subjName = "";
                String phaseName = "";

                //List<String> sides = new List<String>();
                Dictionary<String, int> numLooks = new Dictionary<String, int>();
                Dictionary<String, int> numStimuli = new Dictionary<String, int>();
                Dictionary<String, double> totalLooks = new Dictionary<String, double>();
                Dictionary<String, int> numTrialsWithSideLook = new Dictionary<String, int>();
                int lineCounter = 0;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (lookAlreadyLogged && (!line.Contains("duration") || line.Contains("Debug")))
                    {
                        //we were dealing with multiple look-duration lines that corresponded to a single
                        //look - now we're done with that, so reset lookAlreadyLogged to false
                        lookAlreadyLogged = false;
                    }
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if ((line.Contains("phase") && line.Contains("ended")) || (line.Contains("Halted Prematurely") && !phaseName.Equals("")))
                    {
                        //in the first case the phase we were evaluating has ended, and in the second case the experiment was halted before
                        //the phase could end - in either case, we want to now add the information we've retrieved for this phase, and then
                        //reset fields for the next phase

                        sides.Sort();

                        double totalLook = 0.0;
                        Dictionary<String, double> means = new Dictionary<String, double>();
                        Dictionary<String, double> trialMeans = new Dictionary<String, double>();

                        foreach (String side in totalLooks.Keys)
                        {
                            totalLook += totalLooks[side];
                            double meanLook = (numLooks[side] > 0) ? (totalLooks[side] / numLooks[side]) : (0.0);
                            means.Add(side, meanLook);

                            double meanTrialLook = (numTrialsWithSideLook[side] > 0) ? (totalLooks[side] / numTrialsWithSideLook[side]) : (0.0);
                            trialMeans.Add(side, meanTrialLook);
                        }

                        List<String> record = new List<String>();
                        recordHeaders = new List<String>();

                        if (multipleLogs)
                        {
                            recordHeaders.Add("SubjectID");
                            record.Add(subjName);
                        }
                        recordHeaders.Add("Phase");
                        record.Add(phaseName);
                        for (int x = 0; x < sides.Count; x++)
                        {
                            String side = sides.ElementAt(x);

                            if (totalLooks.Keys.Contains(side))
                            {
                                recordHeaders.Add(side + "StimuliCount");
                                record.Add(numStimuli[side].ToString());
                                recordHeaders.Add("TotalTimeLooking" + side);
                                record.Add(totalLooks[side].ToString());
                                recordHeaders.Add("MeanSingleLook" + side);
                                record.Add(means[side].ToString());
                                recordHeaders.Add("MeanTrialLook" + side);
                                record.Add(trialMeans[side].ToString());
                            }
                            else
                            {
                                recordHeaders.Add(side + "StimuliCount");
                                record.Add("0");
                                recordHeaders.Add("TotalTimeLooking" + side);
                                record.Add("0");
                                recordHeaders.Add("MeanSingleLook" + side);
                                record.Add("0");
                                recordHeaders.Add("MeanTrialLook" + side);
                                record.Add("0");
                            }
                        }

                        double totalLookAllSides = 0.0;
                        foreach (double look in totalLooks.Values)
                        {
                            totalLookAllSides += look;
                        }

                        List<double> percents = new List<double>();
                        for (int x = 0; x < sides.Count; x++)
                        {
                            String side = sides.ElementAt(x);

                            if (totalLooks.Keys.Contains(side))
                            {
                                double percent = totalLooks[side] / totalLookAllSides;
                                percents.Add(percent);
                                recordHeaders.Add("PercentOfLooking" + side);
                                record.Add(percent.ToString());
                            }
                            else
                            {
                                recordHeaders.Add("PercentOfLooking" + side);
                                record.Add("0.0");
                            }
                        }

                        //if we need to calculate averages across subjects, add all values to the allValues dict
                        if (multipleLogs)
                        {
                            if (!allValues.ContainsKey(phaseName))
                            {
                                allValues.Add(phaseName, new Dictionary<string, List<double>>());
                            }
                            for (int x = 0; x < sides.Count(); x++)
                            {
                                String side = sides.ElementAt(x);
                                if (!allValues[phaseName].ContainsKey(side))
                                {
                                    allValues[phaseName].Add(side, new List<double>());
                                }

                                if (totalLooks.Keys.Contains(side))
                                {
                                    allValues[phaseName][side].Add(numStimuli[side]);
                                    allValues[phaseName][side].Add(totalLooks[side]);
                                    allValues[phaseName][side].Add(means[side]);
                                    allValues[phaseName][side].Add(trialMeans[side]);
                                    allValues[phaseName][side].Add(percents.ElementAt(x));
                                }
                            }
                        }

                        records.Add(record);

                        foreach (String side in totalLooks.Keys.ToList<String>())
                        {
                            totalLooks[side] = 0.0;
                            numLooks[side] = 0;
                            numStimuli[side] = 0;
                            numTrialsWithSideLook[side] = 0;
                        }
                    }
                    else if (line.Contains("stimuli") && !line.Contains("off") && inTrial)
                    {
                        String side = "";
                        Match match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            if (line.Contains("blink"))
                            {
                                side = match.Groups[4].Captures[0].ToString();
                            }
                            else
                            {
                                match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+)");
                                side = match.Groups[3].Captures[0].ToString();
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            side = match.Groups[3].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+)");
                            side = match.Groups[2].Captures[0].ToString();
                        }

                        if (!sides.Contains(side))
                        {
                            sides.Add(side);
                        }

                        if (!numLooks.ContainsKey(side))
                        {
                            numLooks.Add(side, 0);
                        }

                        if (!totalLooks.ContainsKey(side))
                        {
                            totalLooks.Add(side, 0.0);
                        }

                        if (!numStimuli.ContainsKey(side))
                        {
                            numStimuli.Add(side, 0);
                        }

                        if (!numTrialsWithSideLook.ContainsKey(side))
                        {
                            numTrialsWithSideLook.Add(side, 0);
                        }

                        numStimuli[side]++;
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;
                        //firstLookAtSideInTrial = true;
                        sidesLookedAtInTrial = new List<String>();
                    }
                    else if (line.Contains("duration") && !line.Contains("in_progress") && !line.Contains("Debug") && inTrial && !lookAlreadyLogged)
                    {
                        bool isPartialLook = false;
                        int currCounter = lineCounter + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        if (!isPartialLook)
                        {
                            int duration = 0;
                            String side = "";

                            Match match = Regex.Match(line, @"look (\S+) (\S+) (\S+) duration (\S+)");
                            side = match.Groups[2].Captures[0].ToString();
                            duration = Int32.Parse(match.Groups[4].Captures[0].ToString());

                            if (sides.Contains(side))
                            {
                                numLooks[side]++;
                                totalLooks[side] += duration;
                            }
                            else
                            {
                                Console.WriteLine("Error in SummaryAcrossSides - a look was logged towards a side that never " +
                                    "had a stimulus ON (this should never happen)");
                            }
                            lookAlreadyLogged = true;

                            //if (firstLookAtSideInTrial)
                            if (!sidesLookedAtInTrial.Contains(side))
                            {
                                numTrialsWithSideLook[side]++;
                                //firstLookAtSideInTrial = false;
                                sidesLookedAtInTrial.Add(side);
                            }
                        }
                    }

                    lineCounter++;
                }

                if (phaseName.Equals(""))
                {
                    //phase names weren't used in this protocol - so we can just treat the experiment as having been one large "phase"

                    sides.Sort();

                    double totalLook = 0.0;
                    Dictionary<String, double> means = new Dictionary<String, double>();
                    Dictionary<String, double> trialMeans = new Dictionary<String, double>();

                    foreach (String side in totalLooks.Keys)
                    {
                        totalLook += totalLooks[side];
                        double meanLook = (numLooks[side] > 0) ? (totalLooks[side] / numLooks[side]) : (0.0);
                        means.Add(side, meanLook);

                        double meanTrialLook = (numTrialsWithSideLook[side] > 0) ? (totalLooks[side] / numTrialsWithSideLook[side]) : (0.0);
                        trialMeans.Add(side, meanTrialLook);
                    }

                    List<String> record = new List<String>();
                    recordHeaders = new List<String>();

                    if (multipleLogs)
                    {
                        recordHeaders.Add("SubjectID");
                        record.Add(subjName);
                    }
                    recordHeaders.Add("Phase");
                    record.Add(phaseName);
                    for (int x = 0; x < sides.Count; x++)
                    {
                        String side = sides.ElementAt(x);

                        recordHeaders.Add(side + "StimuliCount");
                        record.Add(numStimuli[side].ToString());
                        recordHeaders.Add("TotalTimeLooking" + side);
                        record.Add(totalLooks[side].ToString());
                        recordHeaders.Add("MeanSingleLook" + side);
                        record.Add(means[side].ToString());
                        recordHeaders.Add("MeanTrialLook" + side);
                        record.Add(trialMeans[side].ToString());

                    }

                    double totalLookAllSides = 0.0;
                    foreach (double look in totalLooks.Values)
                    {
                        totalLookAllSides += look;
                    }

                    List<double> percents = new List<double>();
                    for (int x = 0; x < sides.Count; x++)
                    {
                        String side = sides.ElementAt(x);
                        double percent = totalLooks[side] / totalLookAllSides;
                        percents.Add(percent);
                        recordHeaders.Add("PercentOfLooking" + side);
                        record.Add(percent.ToString());
                    }

                    //if we need to calculate averages across subjects, add all values to the allValues dict
                    if (multipleLogs)
                    {
                        if (!allValues.ContainsKey(phaseName))
                        {
                            allValues.Add(phaseName, new Dictionary<string, List<double>>());
                        }
                        for (int x = 0; x < sides.Count(); x++)
                        {
                            String side = sides.ElementAt(x);
                            if (!allValues[phaseName].ContainsKey(side))
                            {
                                allValues[phaseName].Add(side, new List<double>());
                            }
                            allValues[phaseName][side].Add(numStimuli[side]);
                            allValues[phaseName][side].Add(totalLooks[side]);
                            allValues[phaseName][side].Add(means[side]);
                            allValues[phaseName][side].Add(trialMeans[side]);
                            allValues[phaseName][side].Add(percents.ElementAt(x));
                        }
                    }

                    records.Add(record);

                    foreach (String side in sides)
                    {
                        totalLooks[side] = 0.0;
                        numLooks[side] = 0;
                        numStimuli[side] = 0;
                        numTrialsWithSideLook[side] = 0;
                    }
                }
            }

            //check if we need to add averages across subjects
            if (multipleLogs)
            {
                foreach (String phase in allValues.Keys)
                {
                    List<String> record = new List<String>();
                    record.Add("(average)");
                    record.Add(phase);
                    List<double> overallPercents = new List<double>();

                    //List<String> sides = new List<String>(allValues[phase].Keys);
                    sides.Sort();
                    foreach (String side in sides)
                    {
                        List<double> sideVals = allValues[phase][side];
                        double overallSideStimuliCount = 0.0;
                        double overallTimeLookingSide = 0.0;
                        double overallMeanSingleLookSide = 0.0;
                        double overallMeanTrialLookSide = 0.0;
                        double overallPercentOfLookingSide = 0.0;

                        int counter = 0;
                        for (int x = 0; x < sideVals.Count(); x += 5)
                        {
                            counter++;
                            overallSideStimuliCount += sideVals[x];
                            overallTimeLookingSide += sideVals[x + 1];
                            overallMeanSingleLookSide += sideVals[x + 2];
                            overallMeanTrialLookSide += sideVals[x + 3];
                            overallPercentOfLookingSide += sideVals[x + 4];
                        }

                        overallSideStimuliCount /= counter;
                        overallTimeLookingSide /= counter;
                        overallMeanSingleLookSide /= counter;
                        overallMeanTrialLookSide /= counter;
                        overallPercentOfLookingSide /= counter;
                        record.Add(overallSideStimuliCount.ToString());
                        record.Add(overallTimeLookingSide.ToString());
                        record.Add(overallMeanSingleLookSide.ToString());
                        record.Add(overallMeanTrialLookSide.ToString());
                        overallPercents.Add(overallPercentOfLookingSide);
                    }

                    for (int x = 0; x < sides.Count(); x++)
                    {
                        record.Add(overallPercents[x].ToString());
                    }

                    records.Add(record);
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) SummaryAcrossGroupsAndTags(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "Group", "Tag", "LookingTimePerTrial" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "Group", "Tag", "LookingTimePerTrial" };
            }
            List<List<String>> records = new List<List<String>>();

            Dictionary<String, Dictionary<String, List<double>>> allAvgGroupTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allAvgTagTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                bool inTrial = false;

                String phaseName = "";
                String subjName = "";
                String trialNum = "";
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                Dictionary<String, String> stimuliLastGroup = new Dictionary<String, String>();
                Dictionary<String, String> stimuliAllGroups = new Dictionary<String, String>();
                Dictionary<String, String> stimuliTypes = new Dictionary<String, String>();
                Dictionary<String, double> groupTotalLooks = new Dictionary<String, double>();
                Dictionary<String, int> groupNumLooks = new Dictionary<String, int>();
                Dictionary<String, double> stimuliTotalLooks = new Dictionary<String, double>();
                Dictionary<String, int> stimuliNumLooks = new Dictionary<String, int>();
                Dictionary<String, int> numTrialsWithTagActive = new Dictionary<String, int>();
                Dictionary<String, int> numTrialsWithGroupActive = new Dictionary<String, int>();

                List<String> tagsSeenTrial = new List<String>();
                List<String> groupsSeenTrial = new List<String>();

                int lineCounter = 0;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    else if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("stimuli") && !line.Contains("off"))
                    {
                        String stimulusTag = "";
                        String side = "";
                        String type = "";
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            continue;
                            /* if (line.Contains("blink"))
                            {
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                            else
                            {
                                match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            } */
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }

                        if (!activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Add(side + "|" + type, stimulusTag);
                        }

                        if (stimuliTypes.ContainsKey(stimulusTag))
                        {
                            stimuliTypes.Remove(stimulusTag);
                        }
                        stimuliTypes.Add(stimulusTag, type);

                        //we don't need to worry about the following things if the stimulus type isn't one of the selected types
                        //to include in the report, so first check if it is
                        if (stimulusTypesToInclude.Contains(type))
                        {
                            if (!stimuliNumLooks.ContainsKey(stimulusTag))
                            {
                                stimuliNumLooks.Add(stimulusTag, 0);
                            }
                            if (!stimuliTotalLooks.ContainsKey(stimulusTag))
                            {
                                stimuliTotalLooks.Add(stimulusTag, 0.0);
                            }
                            if (!numTrialsWithTagActive.ContainsKey(stimulusTag))
                            {
                                numTrialsWithTagActive.Add(stimulusTag, 0);
                            }

                            if (stimuliLastGroup.ContainsKey(stimulusTag))
                            {
                                String group = stimuliLastGroup[stimulusTag];
                                if (!numTrialsWithGroupActive.ContainsKey(group))
                                {
                                    numTrialsWithGroupActive.Add(group, 0);
                                }

                                if (stimuliAllGroups.ContainsKey(stimulusTag))
                                {
                                    String preNewGroup = stimuliAllGroups[stimulusTag];
                                    if (!preNewGroup.Contains(group))
                                    {
                                        String postNewGroup = preNewGroup + "," + group;
                                        stimuliAllGroups.Remove(stimulusTag);
                                        stimuliAllGroups.Add(stimulusTag, postNewGroup);
                                    }
                                }
                                else
                                {
                                    stimuliAllGroups.Add(stimulusTag, group);
                                }

                                if (!groupNumLooks.ContainsKey(group))
                                {
                                    groupNumLooks.Add(group, 0);
                                }
                                if (!groupTotalLooks.ContainsKey(group))
                                {
                                    groupTotalLooks.Add(group, 0.0);
                                }
                            }

                            if (inTrial)
                            {
                                if (!tagsSeenTrial.Contains(stimulusTag))
                                {
                                    numTrialsWithTagActive[stimulusTag]++;
                                    tagsSeenTrial.Add(stimulusTag);
                                }

                                if (stimuliLastGroup.ContainsKey(stimulusTag))
                                {
                                    String group = stimuliLastGroup[stimulusTag];
                                    if (!groupsSeenTrial.Contains(group))
                                    {
                                        numTrialsWithGroupActive[group]++;
                                        groupsSeenTrial.Add(group);
                                    }
                                }
                            }
                        }
                    }
                    else if (line.Contains("stimuli") && line.Contains("off"))
                    {
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        String type = match.Groups[2].Captures[0].ToString();
                        String side = match.Groups[3].Captures[0].ToString();

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                    }
                    else if (line.Contains("from group") && line.Contains("Tag") && !line.Contains("SDL"))
                    {
                        Match match = null;
                        if (line.Contains("taken"))
                        {
                            match = Regex.Match(line, @"Tag (\S+) taken from group (\S+)");
                        }
                        else
                        {
                            match = Regex.Match(line, @"Tag (\S+) from group (\S+)");
                        }

                        String group = match.Groups[2].Captures[0].ToString();
                        String tag = match.Groups[1].Captures[0].ToString();
                        
                        if (stimuliLastGroup.ContainsKey(tag))
                        {
                            stimuliLastGroup.Remove(tag);
                        }
                        stimuliLastGroup.Add(tag, group);
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if ((line.Contains("phase") && line.Contains("ended")) || (line.Contains("Halted Prematurely") && !phaseName.Equals("")) || (line.Contains("Experiment Ended") && phaseName.Equals("")))
                    {
                        //in the first case the phase we were evaluating has ended, and in the second case the experiment was halted before
                        //the phase could end - in either case, we want to now add the information we've retrieved for this phase, and then
                        //reset fields for the next phase
                        //the third case is when we've reached the end of the log file and phaseName was never changed - this means phases
                        //weren't used in this experimment, so again this is a time where we want to add the information we've retrieved

                        List<String> sortedGroups = groupTotalLooks.Keys.ToList<String>();
                        List<String> sortedTags = stimuliTotalLooks.Keys.ToList<String>();
                        sortedGroups.Sort();
                        sortedTags.Sort();

                        foreach (String groupName in sortedGroups)
                        {
                            if (multipleLogs)
                            {
                                if (!allAvgGroupTimes.ContainsKey(phaseName))
                                {
                                    allAvgGroupTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allAvgGroupTimes[phaseName].ContainsKey(groupName))
                                {
                                    allAvgGroupTimes[phaseName].Add(groupName, new List<double>());
                                }
                            }

                            String avgGroupTime = "n/a";
                            if (numTrialsWithGroupActive[groupName] > 0)
                            {
                                avgGroupTime = (groupTotalLooks[groupName] / numTrialsWithGroupActive[groupName]).ToString();
                                if (multipleLogs)
                                {
                                    allAvgGroupTimes[phaseName][groupName].Add(groupTotalLooks[groupName] / numTrialsWithGroupActive[groupName]);
                                }
                            }

                            List<String> record = new List<String>();
                            if (multipleLogs)
                            {
                                record.Add(subjName);
                            }
                            record.Add(phaseName);
                            record.Add(groupName);
                            record.Add("(average)");
                            record.Add(avgGroupTime);

                            records.Add(record);
                        }

                        foreach (String tagName in sortedTags)
                        {
                            if (multipleLogs)
                            {
                                if (!allAvgTagTimes.ContainsKey(phaseName))
                                {
                                    allAvgTagTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allAvgTagTimes[phaseName].ContainsKey(tagName))
                                {
                                    allAvgTagTimes[phaseName].Add(tagName, new List<double>());
                                }
                            }

                            String avgTagTime = "n/a";
                            if (numTrialsWithTagActive[tagName] > 0)
                            {
                                avgTagTime = (stimuliTotalLooks[tagName] / numTrialsWithTagActive[tagName]).ToString();
                                if (multipleLogs)
                                {
                                    allAvgTagTimes[phaseName][tagName].Add(stimuliTotalLooks[tagName] / numTrialsWithTagActive[tagName]);
                                }
                            }

                            List<String> record = new List<String>();
                            if (multipleLogs)
                            {
                                record.Add(subjName);
                            }
                            record.Add(phaseName);
                            record.Add(stimuliAllGroups.ContainsKey(tagName) ? (stimuliAllGroups[tagName]) : (""));
                            record.Add(tagName);
                            record.Add(avgTagTime);

                            records.Add(record);
                        }

                        activeStimuli = new Dictionary<String, String>();
                        stimuliLastGroup = new Dictionary<String, String>();
                        stimuliAllGroups = new Dictionary<String, String>();
                        stimuliTypes = new Dictionary<String, String>();
                        groupTotalLooks = new Dictionary<String, double>();
                        groupNumLooks = new Dictionary<String, int>();
                        stimuliTotalLooks = new Dictionary<String, double>();
                        stimuliNumLooks = new Dictionary<String, int>();
                        numTrialsWithTagActive = new Dictionary<String, int>();
                        numTrialsWithGroupActive = new Dictionary<String, int>();
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, "trial (.+) started");
                        trialNum = match.Groups[1].Captures[0].ToString();

                        inTrial = true;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        inTrial = false;

                        tagsSeenTrial = new List<String>();
                        groupsSeenTrial = new List<String>();
                    }
                    else if (line.Contains("duration") && !line.Contains("Debug") && inTrial)
                    {
                        bool isPartialLook = false;
                        int currCounter = lineCounter + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        if (!isPartialLook)
                        {
                            Match match = Regex.Match(line, @"look (\S+) (\S+) (\S+) duration (\S+)");
                            String stimulusTag = "";
                            String look = "";
                            String group = "";
                            String side = match.Groups[2].Captures[0].ToString();

                            if (line.Contains("in_progress"))
                            {
                                continue;
                                /*look = "in_progress";
                                stimulusTag = match.Groups[3].Captures[0].ToString();
                                if (stimulusTag.Equals("no_tag"))
                                {
                                    continue;
                                    /* String type = "light";
                                    stimulusTag = activeStimuli[side + "|" + type];
                                    //stimulusTag = "light_" + match.Groups[2].Captures[0].ToString(); *./
                                }
                                if (stimuliLastGroup.ContainsKey(stimulusTag))
                                {
                                    group = stimuliLastGroup[stimulusTag];
                                }*/
                            }
                            else
                            {
                                look = match.Groups[4].Captures[0].ToString();
                                int lookTime = Int32.Parse(look);
                                stimulusTag = match.Groups[3].Captures[0].ToString();
                                //totalLookTrial += lookTime;

                                if (stimulusTag.Equals("no_tag"))
                                {
                                    continue;
                                    /* String type = "light";
                                    stimulusTag = activeStimuli[side + "|" + type];
                                    //stimulusTag = "light_" + match.Groups[2].Captures[0].ToString(); */
                                }

                                String type = stimuliTypes[stimulusTag];
                                if (stimulusTypesToInclude.Contains(type))
                                {
                                    if (stimuliLastGroup.ContainsKey(stimulusTag))
                                    {
                                        group = stimuliLastGroup[stimulusTag];
                                        groupTotalLooks[group] += lookTime;
                                        groupNumLooks[group]++;
                                    }

                                    stimuliTotalLooks[stimulusTag] += lookTime;
                                    stimuliNumLooks[stimulusTag]++;
                                }
                            }
                        }
                    }

                    lineCounter++;
                }
            }

            //check if we need to include averages across subjects (i.e. check if multiple subjects)
            if (multipleLogs)
            {
                foreach (String phase in allAvgTagTimes.Keys)
                {
                    List<String> sortedGroups = allAvgGroupTimes[phase].Keys.ToList<String>();
                    List<String> sortedTags = allAvgTagTimes[phase].Keys.ToList<String>();
                    sortedGroups.Sort();
                    sortedTags.Sort();

                    foreach (String group in sortedGroups)
                    {
                        List<String> record = new List<String>();

                        String overallGroupLookingTime = "n/a";
                        if (allAvgGroupTimes[phase][group].Count() > 0)
                        {
                            overallGroupLookingTime = (allAvgGroupTimes[phase][group].Sum() / allAvgGroupTimes[phase][group].Count()).ToString();
                        }

                        record.Add("(average)");
                        record.Add(phase);
                        record.Add(group);
                        record.Add("(average)");
                        record.Add(overallGroupLookingTime);

                        records.Add(record);
                    }

                    foreach (String tag in sortedTags)
                    {
                        List<String> record = new List<String>();

                        String overallTagLookingTime = "n/a";
                        if (allAvgTagTimes[phase][tag].Count() > 0)
                        {
                            overallTagLookingTime = (allAvgTagTimes[phase][tag].Sum() / allAvgTagTimes[phase][tag].Count()).ToString();
                        }

                        record.Add("(average)");
                        record.Add(phase);
                        record.Add("(average)");
                        record.Add(tag);
                        record.Add(overallTagLookingTime);

                        records.Add(record);
                    }
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) DetailedLooking(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count() > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Phase", "Trial", "TimeExperiment", "TimeTrial", "Key", "ActiveStimuli", "ActiveSides" };
            }
            else
            {
                recordHeaders = new List<String>() { "Phase", "Trial", "TimeExperiment", "TimeTrial", "Key", "ActiveStimuli", "ActiveSides" };
            }
            List<List<String>> records = new List<List<String>>();

            List<String> subjectsWhoDidntFinish = new List<String>();

            foreach (String logFilePath in logPaths)
            {
                String subjName = "";
                String phaseName = "";
                String trialNum = "";
                String lastKeyPress = "SPACE";
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                double lastTime = 0.0;
                int linesAdded = 1;
                int linesAddedTrial = 0;
                bool inTrial = false;

                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (line.Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];
                    }
                    else if (line.Contains("execution_begin"))
                    {
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]execution_begin");
                        String startTimeStr = match.Groups[1].Captures[0].ToString();
                        DateTime startTime = DateTime.ParseExact(startTimeStr, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        lastTime = startTime.TimeOfDay.TotalMilliseconds;
                        lastTime = Math.Ceiling(lastTime / 50.0) * 50.0;
                    }
                    else if (line.Contains("stimuli") && !line.Contains("off"))
                    {
                        String side = "";
                        String type = "";
                        String stimulusTag = "";
                        String time = "";
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (line.Contains("light"))
                        {
                            type = "light";
                            if (line.Contains("blink"))
                            {
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                time = match.Groups[1].Captures[0].ToString();
                            }
                            else
                            {
                                match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                time = match.Groups[1].Captures[0].ToString();
                            }
                        }
                        else if (line.Contains("ONCE") || line.Contains("LOOP"))
                        {
                            type = match.Groups[2].Captures[0].ToString();
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            time = match.Groups[1].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            type = match.Groups[2].Captures[0].ToString();
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            time = match.Groups[1].Captures[0].ToString();
                        }

                        DateTime timeParse = DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double newTime = timeParse.TimeOfDay.TotalMilliseconds;

                        newTime = ((int)(newTime / 50)) * 50;

                        double elapsed = newTime - lastTime;
                        int linesToAdd = (int)Math.Round(elapsed / 50, MidpointRounding.AwayFromZero);
                        if (inTrial)
                        {
                            String currentStimuli = "";
                            String currentStimuliSides = "";
                            foreach (String sidetype in activeStimuli.Keys)
                            {
                                String curSide = Regex.Split(sidetype, @"\|")[0];
                                String curStim = activeStimuli[sidetype];
                                currentStimuliSides = (currentStimuliSides.Equals("") ? curSide : currentStimuliSides + "," + curSide);
                                currentStimuli = (currentStimuli.Equals("") ? curStim : currentStimuli + "," + curStim);
                            }
                            for (int i = 0; i < linesToAdd; i++)
                            {
                                linesAdded++;
                                linesAddedTrial++;

                                List<String> record = new List<String>();
                                if (multipleLogs)
                                {
                                    record.Add(subjName);
                                }
                                record.Add(phaseName);
                                record.Add(trialNum);
                                record.Add((linesAdded * 50).ToString());
                                record.Add((linesAddedTrial * 50).ToString());
                                record.Add(lastKeyPress);
                                record.Add(currentStimuli);
                                record.Add(currentStimuliSides);

                                records.Add(record);
                            }
                            lastTime = newTime;
                        }

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        activeStimuli.Add(side + "|" + type, stimulusTag);
                    }
                    else if (line.Contains("stimuli") && line.Contains("off"))
                    {
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        String type = match.Groups[2].Captures[0].ToString();
                        String side = match.Groups[3].Captures[0].ToString();
                        String time = match.Groups[1].Captures[0].ToString();

                        DateTime timeParse = DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double newTime = timeParse.TimeOfDay.TotalMilliseconds;

                        newTime = ((int)(newTime / 50)) * 50;

                        double elapsed = newTime - lastTime;
                        int linesToAdd = (int)Math.Round(elapsed / 50, MidpointRounding.AwayFromZero);
                        if (inTrial)
                        {
                            String currentStimuli = "";
                            String currentStimuliSides = "";
                            foreach (String sidetype in activeStimuli.Keys)
                            {
                                String curSide = Regex.Split(sidetype, @"\|")[0];
                                String curStim = activeStimuli[sidetype];
                                currentStimuliSides = (currentStimuliSides.Equals("") ? curSide : currentStimuliSides + "," + curSide);
                                currentStimuli = (currentStimuli.Equals("") ? curStim : currentStimuli + "," + curStim);
                            }
                            for (int i = 0; i < linesToAdd; i++)
                            {
                                linesAdded++;
                                linesAddedTrial++;

                                List<String> record = new List<String>();
                                if (multipleLogs)
                                {
                                    record.Add(subjName);
                                }
                                record.Add(phaseName);
                                record.Add(trialNum);
                                record.Add((linesAdded * 50).ToString());
                                record.Add((linesAddedTrial * 50).ToString());
                                record.Add(lastKeyPress);
                                record.Add(currentStimuli);
                                record.Add(currentStimuliSides);

                                records.Add(record);
                            }
                            lastTime = newTime;
                        }

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        else
                        {
                            Console.WriteLine("DetailedLooking - failed to remove a stimulus from activeStimuli dict" +
                                "at side " + side + " and of type " + type);
                        }
                    }
                    else if (line.Contains("phase") && line.Contains("started"))
                    {
                        Match match = Regex.Match(line, @"phase (.+) started (.+)");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (line.Contains("trial") && line.Contains("started"))
                    {
                        inTrial = true;
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]habituation trial (.+) started (.+)");
                        trialNum = match.Groups[2].Captures[0].ToString();
                        String time = match.Groups[1].Captures[0].ToString();

                        DateTime timeParse = DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double newTime = timeParse.TimeOfDay.TotalMilliseconds;

                        newTime = ((int)(newTime / 50)) * 50;

                        double elapsed = newTime - lastTime;
                        linesAdded += (int)Math.Round(elapsed / 50, MidpointRounding.AwayFromZero);

                        String currentStimuli = "";
                        String currentStimuliSides = "";
                        foreach (String sidetype in activeStimuli.Keys)
                        {
                            String curSide = Regex.Split(sidetype, @"\|")[0];
                            String curStim = activeStimuli[sidetype];
                            currentStimuliSides = (currentStimuliSides.Equals("") ? curSide : currentStimuliSides + "," + curSide);
                            currentStimuli = (currentStimuli.Equals("") ? curStim : currentStimuli + "," + curStim);
                        }

                        List<String> record = new List<String>();
                        if (multipleLogs)
                        {
                            record.Add(subjName);
                        }
                        record.Add(phaseName);
                        record.Add(trialNum);
                        record.Add((linesAdded * 50).ToString());
                        record.Add("Trial Start");
                        record.Add(lastKeyPress);
                        record.Add(currentStimuli);
                        record.Add(currentStimuliSides);

                        records.Add(record);

                        lastTime = newTime;
                    }
                    else if (line.Contains("trial") && line.Contains("ended"))
                    {
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]habituation trial (.+) ended_successfully (.+)");
                        String time = match.Groups[1].Captures[0].ToString();

                        DateTime timeParse = DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double newTime = timeParse.TimeOfDay.TotalMilliseconds;

                        newTime = ((int)(newTime / 50)) * 50;

                        double elapsedSinceLastLook = newTime - lastTime;
                        int linesToAdd = (int)Math.Round(elapsedSinceLastLook / 50, MidpointRounding.AwayFromZero);
                        String currentStimuli = "";
                        String currentStimuliSides = "";
                        foreach (String sidetype in activeStimuli.Keys)
                        {
                            String curSide = Regex.Split(sidetype, @"\|")[0];
                            String curStim = activeStimuli[sidetype];
                            currentStimuliSides = (currentStimuliSides.Equals("") ? curSide : currentStimuliSides + "," + curSide);
                            currentStimuli = (currentStimuli.Equals("") ? curStim : currentStimuli + "," + curStim);
                        }

                        for (int i = 0; i < linesToAdd - 1; i++)
                        {
                            linesAdded++;
                            linesAddedTrial++;

                            List<String> subrecord = new List<String>();
                            if (multipleLogs)
                            {
                                subrecord.Add(subjName);
                            }
                            subrecord.Add(phaseName);
                            subrecord.Add(trialNum);
                            subrecord.Add((linesAdded * 50).ToString());
                            subrecord.Add((linesAddedTrial * 50).ToString());
                            subrecord.Add(lastKeyPress);
                            subrecord.Add(currentStimuli);
                            subrecord.Add(currentStimuliSides);

                            records.Add(subrecord);
                        }

                        inTrial = false;
                        linesAdded++;
                        linesAddedTrial = 0;

                        List<String> record = new List<String>();
                        if (multipleLogs)
                        {
                            record.Add(subjName);
                        }
                        record.Add(phaseName);
                        record.Add(trialNum);
                        record.Add((linesAdded * 50).ToString());
                        record.Add("Trial End");
                        record.Add(lastKeyPress);
                        record.Add(currentStimuli);
                        record.Add(currentStimuliSides);

                        records.Add(record);

                        lastTime = newTime;
                    }
                    else if (line.Contains("Debug:") && line.Contains("pressed"))
                    {
                        Match match = Regex.Match(line, @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]Debug: (\S+) pressed");
                        String time = match.Groups[1].Captures[0].ToString();

                        DateTime timeParse = DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double newTime = timeParse.TimeOfDay.TotalMilliseconds;

                        newTime = ((int)(newTime / 50)) * 50;
                        double elapsed = newTime - lastTime;
                        int linesToAdd = (int)Math.Round(elapsed / 50, MidpointRounding.AwayFromZero);
                        if (inTrial)
                        {
                            String currentStimuli = "";
                            String currentStimuliSides = "";
                            foreach (String sidetype in activeStimuli.Keys)
                            {
                                String curSide = Regex.Split(sidetype, @"\|")[0];
                                String curStim = activeStimuli[sidetype];
                                currentStimuliSides = (currentStimuliSides.Equals("") ? curSide : currentStimuliSides + "," + curSide);
                                currentStimuli = (currentStimuli.Equals("") ? curStim : currentStimuli + "," + curStim);
                            }
                            for (int i = 0; i < linesToAdd; i++)
                            {
                                linesAdded++;
                                linesAddedTrial++;

                                List<String> record = new List<String>();
                                if (multipleLogs)
                                {
                                    record.Add(subjName);
                                }
                                record.Add(phaseName);
                                record.Add(trialNum);
                                record.Add((linesAdded * 50).ToString());
                                record.Add((linesAddedTrial * 50).ToString());
                                record.Add(lastKeyPress);
                                record.Add(currentStimuli);
                                record.Add(currentStimuliSides);

                                records.Add(record);
                            }

                            lastTime = newTime;
                        }
                        lastKeyPress = match.Groups[2].Captures[0].ToString();
                    }
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (recordHeaders, records);
        }

        public static (List<String>, List<List<String>>, List<String>, List<List<String>>, List<String>, List<List<String>>) Habituation(List<String> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);

            List<String> habitReqHeader = null;
            List<String> recordHeaders = null;
            List<String> wasHabituationMetHeader = null;
            if (multipleLogs)
            {
                habitReqHeader = new List<String>() { "SubjectID", "WindowSize", "WindowOverlap", "CriterionReduction", "BasisChosen", "WindowType", "BasisMinimumTime" };
                wasHabituationMetHeader = new List<String>() { "SubjectID", "HabituationMet", "NumberOfHabituationTrials" };
                recordHeaders = new List<String>() { "SubjectID", "Phase", "TrialNum", "Stimulus", "Group", "Side", "TimeToOrient", "TotalPlayingTime", "TotalLookingTime" };
            }
            else
            {
                habitReqHeader = new List<String>() { "WindowSize", "WindowOverlap", "CriterionReduction", "BasisChosen", "WindowType", "BasisMinimumTime" };
                wasHabituationMetHeader = new List<String>() { "HabituationMet", "NumberOfHabituationTrials" };
                recordHeaders = new List<String>() { "Phase", "TrialNum", "Stimulus", "Group", "Side", "TimeToOrient", "TotalPlayingTime", "TotalLookingTime" };
            }
            List<List<String>> wasHabituationMet = new List<List<String>>();
            List<List<String>> habituationRequirements = new List<List<String>>();
            List<List<String>> records = new List<List<String>>();

            Dictionary<String, Dictionary<String, List<double>>> allOrientTimes = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allTotalPlays = new Dictionary<String, Dictionary<String, List<double>>>();
            Dictionary<String, Dictionary<String, List<double>>> allTotalLooks = new Dictionary<String, Dictionary<String, List<double>>>();
            List<double> allTrialHabitReachedAt = new List<double>();
            List<String> subjectsWhoDidntFinish = new List<String>();
            
            foreach (String logFilePath in logPaths)
            {
                bool lookAlreadyLogged = false;
                String phaseName = "";
                String subjName = "";
                String trialNum = "";
                String trialReachedHabituation = "";
                Dictionary<String, String> activeStimuli = new Dictionary<String, String>();
                Dictionary<String, String> activeStimuliOnTimes = new Dictionary<String, String>();
                Dictionary<String, String> stimuliGroups = new Dictionary<String, String>();
                Dictionary<String, int> stimuliTotalLooks = new Dictionary<String, int>();
                Dictionary<String, double> stimuliOrientTimes = new Dictionary<String, double>();
                Dictionary<String, bool> stimuliOriented = new Dictionary<String, bool>();
                String[,] keyPressTimes = new String[3, 2] { { "SPACE", "0" }, { "SPACE", "0" }, { "SPACE", "0" } };
                bool lookAutoHandled = false;

                bool inTrial = true;
                bool habituationMet = false;

                String[] logText = File.ReadAllLines(logFilePath);
                bool completed = false;
                int line = 0;
                while (!completed)
                {
                    if (Regex.IsMatch(logText[line], @"habituation$"))
                    {
                        line += 2;
                        //the following lines contain the habituation requirements that we want to include
                        String windowSize = Regex.Split(logText[++line], "windowsize ")[1];
                        String windowOverlap = Regex.Split(logText[++line], "window_overlap ")[1];
                        String criterionReduction = Regex.Split(logText[++line], "criterion_reduction ")[1];
                        String basisChosen = Regex.Split(logText[++line], "basis_chosen ")[1];
                        String windowType = Regex.Split(logText[++line], "window_type ")[1];
                        String basisMinimumTime = Regex.Split(logText[++line], "basisminimumtime ")[1];

                        List<String> record = new List<String>();
                        if (multipleLogs)
                        {
                            record.Add(subjName);
                        }
                        record.Add(windowSize);
                        record.Add(windowOverlap);
                        record.Add(criterionReduction);
                        record.Add(basisChosen);
                        record.Add(windowType);
                        record.Add(basisMinimumTime);

                        habituationRequirements.Add(record);
                    }

                    if (lookAlreadyLogged && (!logText[line].Contains("duration") || logText[line].Contains("Debug")))
                    {
                        //we were dealing with multiple look-duration lines that corresponded to a single
                        //look - now we're done with that, so reset lookAlreadyLogged to false
                        lookAlreadyLogged = false;
                        lookAutoHandled = false;
                    }
                    if (logText[line].Contains("Halted Prematurely"))
                    {
                        subjectsWhoDidntFinish.Add(subjName);
                    }

                    if (logText[line].Contains("subject_names"))
                    {
                        subjName = Regex.Split(logText[line], "subject_names ")[1];
                    }
                    else if (logText[line].Contains("phase") && logText[line].Contains("started"))
                    {
                        Match match = Regex.Match(logText[line], "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (logText[line].Contains("trial") && logText[line].Contains("started"))
                    {
                        Match match = Regex.Match(logText[line], "trial (.+) started");
                        trialNum = match.Groups[1].Captures[0].ToString();

                        inTrial = true;
                    }
                    else if (logText[line].Contains("from group") && logText[line].Contains("Tag") && !logText[line].Contains("SDL"))
                    {
                        //this line corresponds to a tag being taken from a group, so we want to record the group's name as the
                        //current group 
                        Match match = null;
                        if (logText[line].Contains("taken"))
                        {
                            match = Regex.Match(logText[line], @"Tag (\S+) taken from group (\S+)");
                        }
                        else
                        {
                            match = Regex.Match(logText[line], @"Tag (\S+) from group (\S+)");
                        }

                        String group = match.Groups[2].Captures[0].ToString();
                        String tag = match.Groups[1].Captures[0].ToString();
                        if (stimuliGroups.ContainsKey(tag))
                        {
                            stimuliGroups.Remove(tag);
                        }
                        stimuliGroups.Add(tag, group);
                    }
                    else if (logText[line].Contains("phase") && logText[line].Contains("started"))
                    {
                        Match match = Regex.Match(logText[line], "phase (.+) started");
                        phaseName = match.Groups[1].Captures[0].ToString();
                    }
                    else if (logText[line].Contains("stimuli") && !logText[line].Contains("off"))
                    {
                        String stimulusTag = "";
                        String side = "";
                        String onTime = "";
                        String type = "";

                        //the reason for recording onTime is so that once we later record an offTime, we can then report the total 
                        //time that the stimulus was playing for, by subtracting the time elapsed when ON from the time elapsed when OFF
                        //Match match = Regex.Match(line, @"stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        Match match = Regex.Match(logText[line], @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+) (\S+)");
                        if (logText[line].Contains("light"))
                        {
                            //line++;
                            //continue;
                            if (logText[line].Contains("blink"))
                            {
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[5].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                            else
                            {
                                match = Regex.Match(logText[line], @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                                onTime = match.Groups[1].Captures[0].ToString();
                                side = match.Groups[4].Captures[0].ToString();
                                stimulusTag = "light_" + match.Groups[3].Captures[0].ToString();
                                type = "light";
                            }
                        }
                        else if (logText[line].Contains("ONCE") || logText[line].Contains("LOOP"))
                        {
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[4].Captures[0].ToString();
                            stimulusTag = match.Groups[5].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }
                        else
                        {
                            match = Regex.Match(logText[line], @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) (\S+) (\S+) (\S+)");
                            onTime = match.Groups[1].Captures[0].ToString();
                            side = match.Groups[3].Captures[0].ToString();
                            stimulusTag = match.Groups[4].Captures[0].ToString();
                            type = match.Groups[2].Captures[0].ToString();
                        }

                        String stimKey = side + "|" + type;
                        if (activeStimuli.ContainsKey(stimKey))
                        {
                            activeStimuli.Remove(stimKey);
                        }
                        if (activeStimuliOnTimes.ContainsKey(stimKey))
                        {
                            activeStimuliOnTimes.Remove(stimKey);
                        }
                        if (stimuliOrientTimes.ContainsKey(stimKey))
                        {
                            stimuliOrientTimes.Remove(stimKey);
                        }
                        if (stimuliOriented.ContainsKey(stimKey))
                        {
                            stimuliOriented.Remove(stimKey);
                        }
                        activeStimuli.Add(side + "|" + type, stimulusTag);
                        activeStimuliOnTimes.Add(side + "|" + type, onTime);
                        stimuliOrientTimes.Add(side + "|" + type, 0.0);
                        stimuliOriented.Add(side + "|" + type, false);

                        if (!stimuliTotalLooks.ContainsKey(stimulusTag))
                        {
                            stimuliTotalLooks.Add(stimulusTag, 0);
                        }
                    }
                    else if (logText[line].Contains("stimuli") && logText[line].Contains("off"))
                    {
                        String stimulusTag = "";
                        String side = "";
                        String type = "";
                        String offTime = "";
                        String onTime = "";
                        String group = "";
                        int totalLook = 0;

                        Match match = Regex.Match(logText[line], @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]stimuli (\S+) off (\S+)(\s+)(\S+)");
                        type = match.Groups[2].Captures[0].ToString();
                        side = match.Groups[3].Captures[0].ToString();
                        offTime = match.Groups[1].Captures[0].ToString();
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        stimulusTag = activeStimuli[side + "|" + type];
                        totalLook = stimuliTotalLooks[stimulusTag];

                        //now that we have an offTime, we can calculate the total time that the stimulus was playing/presented for
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime off = DateTime.ParseExact(offTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        double totalPlay = off.Subtract(on).TotalMilliseconds;

                        double orientTime = stimuliOrientTimes[side + "|" + type];
                        group = stimuliGroups.ContainsKey(stimulusTag) ? stimuliGroups[stimulusTag] : "";

                        if (totalLook > 0.0 && stimulusTypesToInclude.Contains(type))
                        {
                            //a look was logged towards the stimulus that just turned off, so add this to the summary
                            List<String> record = new List<String>();
                            if (multipleLogs)
                            {
                                record.Add(subjName);
                            }
                            record.Add(phaseName);
                            record.Add(trialNum);
                            record.Add(stimulusTag);
                            record.Add(group);
                            record.Add(side);
                            record.Add(orientTime.ToString());
                            record.Add(totalPlay.ToString());
                            record.Add(totalLook.ToString());

                            records.Add(record);

                            if (multipleLogs)
                            {
                                if (!allOrientTimes.ContainsKey(phaseName))
                                {
                                    allOrientTimes.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allTotalLooks.ContainsKey(phaseName))
                                {
                                    allTotalLooks.Add(phaseName, new Dictionary<String, List<double>>());
                                }
                                if (!allTotalPlays.ContainsKey(phaseName))
                                {
                                    allTotalPlays.Add(phaseName, new Dictionary<String, List<double>>());
                                }

                                if (!allOrientTimes[phaseName].ContainsKey(trialNum))
                                {
                                    allOrientTimes[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allTotalLooks[phaseName].ContainsKey(trialNum))
                                {
                                    allTotalLooks[phaseName].Add(trialNum, new List<double>());
                                }
                                if (!allTotalPlays[phaseName].ContainsKey(trialNum))
                                {
                                    allTotalPlays[phaseName].Add(trialNum, new List<double>());
                                }

                                allOrientTimes[phaseName][trialNum].Add(orientTime);
                                allTotalLooks[phaseName][trialNum].Add(totalLook);
                                allTotalPlays[phaseName][trialNum].Add(totalPlay);
                            }
                        }

                        stimuliTotalLooks[stimulusTag] = 0;

                        if (activeStimuli.ContainsKey(side + "|" + type))
                        {
                            activeStimuli.Remove(side + "|" + type);
                        }
                        if (activeStimuliOnTimes.ContainsKey(side + "|" + type))
                        {
                            activeStimuliOnTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOrientTimes.ContainsKey(side + "|" + type))
                        {
                            stimuliOrientTimes.Remove(side + "|" + type);
                        }
                        if (stimuliOriented.ContainsKey(side + "|" + type))
                        {
                            stimuliOriented.Remove(side + "|" + type);
                        }
                    }
                    else if (logText[line].Contains("trial") && logText[line].Contains("ended"))
                    {
                        inTrial = false;
                        lookAutoHandled = false;
                    }
                    else if (logText[line].Contains("habituation criterion_met"))
                    {
                        habituationMet = true;
                        trialReachedHabituation = trialNum;
                    }
                    else if (logText[line].Contains("duration") && !logText[line].Contains("in_progress") && !logText[line].Contains("Debug") && inTrial)
                    {
                        bool isPartialLook = false;
                        int currCounter = line + 1;
                        String currLine = logText[currCounter];
                        while ((!currLine.Contains("duration") || currLine.Contains("Debug")) && inTrial && (currCounter < logText.Length - 1))
                        {
                            if (currLine.Contains("lookaway") && currLine.Contains("incomplete"))
                            {
                                isPartialLook = true;
                            }

                            currCounter++;
                            currLine = logText[currCounter];
                        }

                        Match match = Regex.Match(logText[line], @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]look (\S+) (\S+) (\S+) duration (\S+)");
                        String type = match.Groups[2].Captures[0].ToString();
                        String side = match.Groups[3].Captures[0].ToString();
                        String stimulusTag = match.Groups[4].Captures[0].ToString();
                        if (stimulusTag.Equals("no_tag"))
                        {
                            type = "light";
                            stimulusTag = activeStimuli[side + "|" + type];
                        }
                        if (type.Equals("display"))
                        {
                            //figure out whether this display stimulus was a video or image
                            type = (activeStimuli.ContainsKey(side + "|" + "video")) ? "video" : "image";
                        }

                        //calculating orientation time
                        String onTime = "";
                        double orientTime = 0.0;
                        onTime = activeStimuliOnTimes[side + "|" + type];
                        DateTime on = DateTime.ParseExact(onTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        if (isPartialLook)
                        {
                            String keyTime = keyPressTimes[0, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else if (lookAutoHandled)
                        {
                            String keyTime = keyPressTimes[2, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        else
                        {
                            String keyTime = keyPressTimes[1, 1];
                            DateTime keypressed = DateTime.ParseExact(keyTime, "MM/dd/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                            orientTime = keypressed.Subtract(on).TotalMilliseconds;
                        }
                        if (orientTime < 0.0)
                        {
                            //the subject was already looking towards this direction when the stimulus turned on, so orientation time
                            //is just 0
                            orientTime = 0.0;
                        }
                        if (!stimuliOriented[side + "|" + type])
                        {
                            stimuliOrientTimes[side + "|" + type] = orientTime;
                            stimuliOriented[side + "|" + type] = true;
                        }

                        if (!isPartialLook)
                        {
                            int lookTime = Int32.Parse(match.Groups[5].Captures[0].ToString());
                            stimuliTotalLooks[stimulusTag] += lookTime;

                            //lookAutoHandled = false;
                            lookAlreadyLogged = true;
                        }
                    }
                    else if (logText[line].Contains("Experiment Ended") || logText[line].Contains("Halted Prematurely"))
                    {
                        completed = true;
                        String habitMet = habituationMet ? "Yes" : "No";
                        String trialReachedHabitNum = habituationMet ? (trialReachedHabituation) : ("n/a");

                        List<String> record = new List<String>();
                        if (multipleLogs)
                        {
                            record.Add(subjName);
                        }
                        record.Add(habitMet);
                        record.Add(trialReachedHabitNum);

                        wasHabituationMet.Add(record);

                        if (multipleLogs && habituationMet)
                        {
                            allTrialHabitReachedAt.Add(Double.Parse(trialReachedHabituation));
                        }
                    }
                    else if (logText[line].Contains("Debug: ") && logText[line].Contains("pressed"))
                    {
                        keyPressTimes[0, 0] = keyPressTimes[1, 0];
                        keyPressTimes[0, 1] = keyPressTimes[1, 1];

                        keyPressTimes[1, 0] = keyPressTimes[2, 0];
                        keyPressTimes[1, 1] = keyPressTimes[2, 1];

                        Match match = Regex.Match(logText[line], @"\[(\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d)\]Debug: (\S+) pressed");
                        keyPressTimes[2, 0] = match.Groups[2].Captures[0].ToString();
                        keyPressTimes[2, 1] = match.Groups[1].Captures[0].ToString();
                    }
                    else if (logText[line].Contains("being handled automatically at end of trial"))
                    {
                        lookAutoHandled = true;
                    }

                    line++;
                }
            }

            //check if we need to include averages across subjects
            if (multipleLogs)
            {
                String overallTrialReachedHabit = (allTrialHabitReachedAt.Count() > 0) ? 
                    ((allTrialHabitReachedAt.Sum() / allTrialHabitReachedAt.Count()).ToString()) : ("n/a");
                List<String> habitMetRecord = new List<String>();

                habitMetRecord.Add("(average)");
                habitMetRecord.Add("");
                habitMetRecord.Add(overallTrialReachedHabit);
                wasHabituationMet.Add(habitMetRecord);

                foreach (String phase in allTotalPlays.Keys)
                {
                    foreach (String trialNum in allTotalPlays[phase].Keys)
                    {
                        List<String> lookRecord = new List<String>();

                        double overallPlayTime = allTotalPlays[phase][trialNum].Sum() / allTotalPlays[phase][trialNum].Count();
                        double overallLookTime = allTotalLooks[phase][trialNum].Sum() / allTotalLooks[phase][trialNum].Count();
                        double overallOrientTime = allOrientTimes[phase][trialNum].Sum() / allOrientTimes[phase][trialNum].Count();

                        lookRecord.Add("(average)");
                        lookRecord.Add(phase);
                        lookRecord.Add(trialNum);
                        lookRecord.Add("");
                        lookRecord.Add("");
                        lookRecord.Add("");
                        lookRecord.Add(overallOrientTime.ToString());
                        lookRecord.Add(overallPlayTime.ToString());
                        lookRecord.Add(overallLookTime.ToString());

                        records.Add(lookRecord);
                    }
                }
            }

            //check if any subjects' experiments were halted early, and note this if so
            if (subjectsWhoDidntFinish.Count() > 0)
            {
                List<String> warningRecord = new List<String>();
                foreach (String subject in subjectsWhoDidntFinish)
                {
                    warningRecord.Add("Note: reported data for subject " + subject + " is likely incomplete, as their experiment was halted prematurely; this may also affect reported averages, if any");
                }
                records.Add(warningRecord);
            }

            return (habitReqHeader, habituationRequirements, wasHabituationMetHeader, wasHabituationMet, recordHeaders, records);
        }

        private static (List<String>, List<List<String>>) Everything(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            bool multipleLogs = (logPaths.Count > 1);
            List<String> recordHeaders = null;
            if (multipleLogs)
            {
                recordHeaders = new List<String>() { "SubjectID", "Date", "Time", "Event" };
            }
            else
            {
                recordHeaders = new List<String>() { "Date", "Time", "Event" };
            }
            List<List<String>> records = new List<List<String>>();

            foreach (String logFilePath in logPaths)
            {
                String subjName = "";
                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    if (line.Contains("subject_names"))
                    {
                        subjName = Regex.Split(line, "subject_names ")[1];

                        if (multipleLogs)
                        {
                            //since 3 lines come before the subjectID line in log files, and we're including a subjectID column
                            //in the report(because of multipleLogs), we need to go back and insert subjName now that we know 
                            //what it is
                            int count = records.Count();
                            records[count - 1][0] = subjName;
                            records[count - 2][0] = subjName;
                            records[count - 3][0] = subjName;
                        }
                    }

                    if (line.Contains("###################") || line.Contains("==================="))
                    {
                        continue;
                    }
                    else
                    {
                        Match match = Regex.Match(line, @"\[(\S+) (\S+)\](.+)");
                        String date = match.Groups[1].Captures[0].ToString();
                        String time = "\'" + match.Groups[2].Captures[0].ToString() + "\'";
                        String rest = match.Groups[3].Captures[0].ToString();

                        List<String> record = new List<String>();
                        if (multipleLogs)
                        {
                            record.Add(subjName);
                        }
                        record.Add(date);
                        record.Add(time);
                        record.Add(rest);

                        records.Add(record);
                    }
                }
            }

            return (recordHeaders, records);
        }

        //Design your own custom summary type with this method stub!
        private static (List<String>, List<List<String>>) Custom(List<string> logPaths, List<String> stimulusTypesToInclude)
        {
            //recordHeaders is a String List where each entry will become one heading cell in the generated
            // .csv summary - you can add all headings at once, like the commented-out example below shows, or
            //you can add headings while processing the data (useful if your headings might vary based on what
            //is one particular event log vs. another)
            List<String> recordHeaders = new List<String>();

            //List<String> recordHeaders = new List<String>() { "Phase", "Trial", "LookingTime" });



            //records is a List of String Lists - each String List will become one row of data in your summary,
            //with each String in that list getting its own cell. One thing to keep in mind is that the data
            //you add to records should match up with the headers you define in recordHeaders, or else the summary
            //will be misaligned. 
            List<List<String>> records = new List<List<String>>();



            //multipleLogs is a boolean that determines whether multiple log files were loaded, or just
            //one - you can use this to establish different behavior for document vs. folder loading (ex.
            //perhaps you only want to include a column for SubjectID if multiple log files were loaded)
            bool multipleLogs = (logPaths.Count > 1);



            foreach (String logFilePath in logPaths)
            {
                String[] logText = File.ReadAllLines(logFilePath);
                foreach (String line in logText)
                {
                    //This is where each log file (whether one or multiple) is analyzed, one line at a time,
                    //so you can use regular expressions and other String tools to find relevant lines and parse
                    //necessary information from them - you can look at the preexisting methods above for examples

                    //When you're ready to add a new line to the summary file, create a new List<String>(), add each piece of
                    //information, and then add this List to the records variable that was previously defined. Example:

                    /* List<String> record = new List<String>();
                     * record.Add(phaseName);
                     * record.Add(trialNum);
                     * record.Add(totalLookTrial);
                     * 
                     * records.Add(record);
                    */

                    //Based on the above example, recordHeaders will need to have three entries: one for the phase name, one for 
                    //the trial number, and one for the trial looking time. See preexisting methods for more examples of 
                    //headers/data - here's some particularly good examples to help you get started: 
                    //         -HeaderInfo and ListMedia are two relatively straightforward examples of basic parsing
                    //         -SummaryAcrossSides gives a way of creating headers while parsing through event logs, instead of
                    //               defining them all beforehand
                    //         -Habituation shows how you can tweak the method signature to have multiple "tables" in one .csv file
                }
            }

            return (recordHeaders, records);
        }
    }
}
