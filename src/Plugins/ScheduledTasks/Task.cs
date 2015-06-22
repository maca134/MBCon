using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using Proxy;
using Proxy.BE;

namespace ScheduledTasks
{
    public class Task
    {

        private static List<Task> _tasks = new List<Task>();
        public static List<Task> Tasks
        {
            get
            {
                return _tasks;
            }
        }

        public static Task Next
        {
            get
            {
                if (_tasks.Count == 0)
                    return null;
                _tasks = _tasks.OrderBy(o => o.NextRun).ToList();
                return _tasks[0];
            }
        }

        public static void LoadTasks(IniParser ini)
        {
            var _i = -1;
            while (true)
            {
                var now = DateTime.Now;
                _i++;
                var command = ini.GetSetting("Tasks", String.Format("Command{0}", _i));
                if (String.IsNullOrWhiteSpace(command))
                {
                    if (_i == 0)
                        continue;
                    else
                    {
                        AppConsole.Log("Finished loading tasks", ConsoleColor.DarkMagenta);
                        break;
                    }
                }

                Command cmd;
                DateTime nextRun;
                var interval = TimeSpan.FromDays(1);
                var loop = 1;

                try
                {
                    cmd = new Command(command);
                }
                catch (CommandException ex)
                {
                    AppConsole.Log(String.Format("Error loading task {0}: {1}", _i, ex.Message), ConsoleColor.Red);
                    continue;
                }

                var time = ini.GetSetting("Tasks", String.Format("Time{0}", _i));
                if (!String.IsNullOrWhiteSpace(time))
                {
                    try
                    {
                        var h = Convert.ToDouble(time.Substring(0, 2));
                        var m = Convert.ToDouble(time.Substring(2, 2));

                        nextRun = DateTime.Today;
                        nextRun = nextRun.AddHours(h);
                        nextRun = nextRun.AddMinutes(m);

                        if (now > nextRun)
                        {
                            nextRun = nextRun.Add(interval);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppConsole.Log(String.Format("Error loading task: Bad String Time {0}", ex.Message), ConsoleColor.Red);
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        interval = TimeSpan.FromSeconds(Convert.ToInt32(ini.GetSetting("Tasks", String.Format("Interval{0}", _i))));
                    }
                    catch (Exception ex)
                    {
                        AppConsole.Log(String.Format("Error loading task: Bad String Interval {0}", ex.Message), ConsoleColor.Red);
                        continue;
                    }

                    TimeSpan delay;
                    try
                    {
                        delay = TimeSpan.FromSeconds(Convert.ToDouble(ini.GetSetting("Tasks", String.Format("Delay{0}", _i), "0")));
                    }
                    catch
                    {
                        delay = new TimeSpan();
                    }
                    loop = (ini.GetBoolSetting("Tasks", String.Format("Repeat{0}", _i))) ? -1 : 1;

                    nextRun = now.Add(delay);
                    nextRun = nextRun.Add(interval);
                }

                var task = new Task()
                {
                    _command = cmd,
                    _nextRun = nextRun,
                    _loop = loop,
                    _interval = interval
                };
                _tasks.Add(task);
            }
            AppConsole.Log(String.Format("Added {0} tasks", _tasks.Count), ConsoleColor.DarkMagenta);
        }

        public static void LoadFromJSON(string jsonPath)
        {
            dynamic tasks;
            var now = DateTime.Now;

            try
            {
                var json = File.ReadAllText(jsonPath);
                tasks = JsonConvert.DeserializeObject(json);
            }
            catch (Exception ex)
            {
                throw new ScheduledTasksException(String.Format("Could not open or parse schedule json file: {0}", ex.Message));
            }
            var i = 0;
            foreach (var item in tasks)
            {
                i++;
                string start;
                Command cmd;
                DateTime nextRun;
                int interval;
                int loop;

                try
                {
                    start = item.start.ToString();
                    if (String.IsNullOrWhiteSpace(start))
                        throw new ScheduledTasksException("Tasks need to have a start defined.");
                }
                catch (Exception ex)
                {
                    throw new ScheduledTasksException(String.Format("Could not parse command: {0}", ex.Message));
                }

                try
                {
                    cmd = new Command(item.cmd.ToString());
                }
                catch (CommandException ex)
                {
                    throw new ScheduledTasksException(String.Format("Could not parse command: {0}", ex.Message));
                }

                try
                {
                    loop = Convert.ToInt16(item.loop.ToString());
                }
                catch
                {
                    loop = -1;
                }

                if (loop == 0)
                {
                    throw new ScheduledTasksException("Loop can not be 0.");
                }

                try
                {
                    interval = Convert.ToInt16(item.interval.ToString());
                }
                catch
                {
                    interval = -1;
                }

                if (Regex.Match(start, "^[0-9]{2}:[0-9]{2}:[0-9]{2}$", RegexOptions.IgnoreCase).Success)
                {
                    // hh:mm:ss
                    var h = Convert.ToDouble(start.Substring(0, 2));
                    var m = Convert.ToDouble(start.Substring(3, 2));
                    var s = Convert.ToDouble(start.Substring(6, 2));

                    nextRun = DateTime.Today;
                    nextRun = nextRun.AddHours(h);
                    nextRun = nextRun.AddMinutes(m);
                    nextRun = nextRun.AddSeconds(s);
                    interval = 86400;
                    if (now > nextRun)
                    {
                        nextRun = nextRun.AddSeconds(interval);
                    }
                }
                else if (Regex.Match(start, "^[0-9]+$", RegexOptions.IgnoreCase).Success)
                {
                    // hhmmss
                    nextRun = now.AddSeconds(Convert.ToDouble(start));
                }
                else
                {
                    throw new ScheduledTasksException("The time field is formatted incorrectly.");
                }

                var task = new Task()
                {
                    _command = cmd,
                    _nextRun = nextRun,
                    _interval = TimeSpan.FromSeconds(interval),
                    _loop = loop
                };
                _tasks.Add(task);
            }
        }

        public static void LoadFromXML(string xmlPath)
        {
            var now = DateTime.Now;
            XmlDocument xmlDoc;
            try
            {
                xmlDoc = new XmlDocument(); // Create an XML document object
                xmlDoc.Load(xmlPath);
            }
            catch (Exception ex)
            {
                throw new ScheduledTasksException(String.Format("Could not open or parse schedule xml file: {0}", ex.Message));
            }

            var schedule = xmlDoc.GetElementsByTagName("job");
            for (var i = 0; i < schedule.Count; i++)
            {
                var node = schedule[i];

                Command cmd;
                var interval = 0;
                var loop = -1;
                DateTime nextRun;

                try
                {
                    cmd = new Command(node["cmd"].InnerText);
                    var start = node["start"].InnerText;
                    try
                    {
                        loop = Convert.ToInt16(node["loop"].InnerText);
                        if (loop > -1)
                        {
                            loop++;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    if (Regex.Match(start, "^[0-9]{2}:[0-9]{2}:[0-9]{2}$", RegexOptions.IgnoreCase).Success)
                    {
                        interval = 86400;
                        nextRun = DateTime.Today;
                        nextRun = nextRun.AddHours(Convert.ToDouble(start.Substring(0, 2)));
                        nextRun = nextRun.AddMinutes(Convert.ToDouble(start.Substring(3, 2)));
                        nextRun = nextRun.AddSeconds(Convert.ToDouble(start.Substring(6, 2)));

                        if (now > nextRun)
                        {
                            nextRun = nextRun.AddSeconds(interval);
                        }
                    }
                    else if (Regex.Match(start, "^[0-9]{6}$", RegexOptions.IgnoreCase).Success)
                    {
                        nextRun = now.AddHours(Convert.ToDouble(start.Substring(0, 2)));
                        nextRun = nextRun.AddMinutes(Convert.ToDouble(start.Substring(2, 2)));
                        nextRun = nextRun.AddSeconds(Convert.ToDouble(start.Substring(4, 2)));
                        if (Regex.Match(node["runtime"].InnerText, "^[0-9]{6}$", RegexOptions.IgnoreCase).Success)
                        {
                            var runtime = node["runtime"].InnerText;
                            interval = Convert.ToInt16(runtime.Substring(4, 2)) + (Convert.ToInt16(runtime.Substring(2, 2)) * 60) + (Convert.ToInt16(runtime.Substring(0, 2)) * 60 * 60);
                        }
                    }
                    else
                    {
                        throw new ScheduledTasksException("The time field is formatted incorrectly.");
                    }
                }
                catch (Exception ex)
                {
                    throw new ScheduledTasksException(String.Format("Could not open or parse schedule xml file: {0}", ex.Message));
                }

                try
                {
                    var task = new Task()
                    {
                        _command = cmd,
                        _nextRun = nextRun,
                        _interval = TimeSpan.FromSeconds(interval),
                        _loop = loop
                    };
                    _tasks.Add(task);
                }
                catch (Exception ex)
                {
                    throw new ScheduledTasksException(String.Format("Error adding task: {0}", ex.Message));
                }
            }
        }

        private DateTime _nextRun;
        public DateTime NextRun
        {
            get
            {
                return _nextRun;
            }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                var now = DateTime.Now;

                if (now > _nextRun)
                    _nextRun.Add(_interval);
                return _nextRun.Subtract(DateTime.Now);
            }
        }

        private TimeSpan _interval;
        public TimeSpan Interval
        {
            get
            {
                return _interval;
            }
        }

        private int _loop = -1;
        public int Loop
        {
            get
            {
                return _loop;
            }
        }

        private Command _command;
        public Command Command
        {
            get
            {
                return _command;
            }
        }

        public void Increment()
        {
            if (_loop > 0)
            {
                _loop--;
            }
            if (_loop == 0)
            {
                _tasks.Remove(this);
                return;
            }
            _nextRun = _nextRun.Add(_interval);
        }

    }
}
