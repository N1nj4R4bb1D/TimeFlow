using System;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley.Event;

namespace TimeFlow
{
    [Mod]
    public class TimeFlow : DiskResource
    {
        public static ModConfig TimeFlowConfig { get; private set; }
        public double timeCounter = 0;
        public double lastGameTimeInterval = 0;
        public int TenMinuteTickInterval = 7;

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            TimeFlowConfig = new ModConfig();
            TimeFlowConfig = (ModConfig)Config.InitializeConfig(PathOnDisk + "\\Config.json", TimeFlowConfig);
            /*
            Console.WriteLine("The config file for TimeFlow has been loaded.\n"+
                "\n\tTickIntervalOutside: {0}"+
                "\n\tTickIntervalInside: {1}"+
                "\n\tTickIntervalInMines: {2}"+
                "\n\tFreezeTimeInMines: {3}",
                TimeFlowConfig.TickIntervalOutdoors, TimeFlowConfig.TickIntervalIndoors, TimeFlowConfig.TickIntervalInMines, TimeFlowConfig.FreezeTimeInMines);
            Console.WriteLine("\nTimeFlow Initialization Completed");
            */
            TenMinuteTickInterval = TimeFlowConfig.TickIntervalIndoors;
        }

        [Subscribe]
        public void Pre10MinuteClockUpdateCallback(Pre10MinuteClockUpdateEvent @event)
        {
            //Console.WriteLine("TimeFlow : 10MinuteClockUpdate : " + DateTime.Now.ToString("HH:mm:ss.ffff"));
            var location = @event.Root.CurrentLocation;
            if (location != null && !location.IsOutdoors && ((location.Name.Equals("UndergroundMine") || location.Name.Equals("FarmCave")) && TimeFlowConfig.FreezeTimeInMines))
            {
                @event.ReturnEarly = true;
            }
            timeCounter = 0;
            lastGameTimeInterval = 0;
        }

        [Subscribe]
        public void UpdateGameClockCallback(UpdateGameClockEvent @event)
        {
            if (@event.Root.DayOfMonth != null && @event.Root.CurrentSeason != null)
            {
                timeCounter += Math.Abs((@event.Root.GameTimeInterval - lastGameTimeInterval));
                double fraction = Math.Abs(timeCounter / TenMinuteTickInterval);
                double proportion;

                if (!@event.Root.CurrentLocation.IsOutdoors)
                    switch (@event.Root.CurrentLocation.Name)
                    {
                        case "UndergroundMine":
                        case "FarmCave":
                            timeCounter = (!TenMinuteTickInterval.Equals(TimeFlowConfig.TickIntervalInMines)) ? Math.Abs(TimeFlowConfig.TickIntervalInMines * fraction) : timeCounter;
                            TenMinuteTickInterval = TimeFlowConfig.TickIntervalInMines;
                            break;
                        default:
                            timeCounter = (!TenMinuteTickInterval.Equals(TimeFlowConfig.TickIntervalIndoors)) ? Math.Abs(TimeFlowConfig.TickIntervalIndoors * fraction) : timeCounter;
                            TenMinuteTickInterval = TimeFlowConfig.TickIntervalIndoors;
                            break;
                    }
                else
                {
                    timeCounter = (!TenMinuteTickInterval.Equals(TimeFlowConfig.TickIntervalOutdoors)) ? Math.Abs(TimeFlowConfig.TickIntervalOutdoors * fraction) : timeCounter;
                    TenMinuteTickInterval = TimeFlowConfig.TickIntervalOutdoors;
                }

                proportion = Math.Abs(7 * timeCounter / TenMinuteTickInterval);
                @event.Root.GameTimeInterval = Convert.ToInt32(proportion);
                lastGameTimeInterval = @event.Root.GameTimeInterval;
                /*
                if (lastGameTimeInterval % 100 == 0)
                    Console.WriteLine("TimeFlow : " + @event.Root.CurrentLocation.Name + " : " + DateTime.Now.ToString("HH:mm:ss.ffff") +
                        "\n\ttimeCounter : " + Convert.ToInt32(timeCounter).ToString() + "/" + (TenMinuteTickInterval * 1000).ToString() +
                        "\n\tproportion : " + Convert.ToInt32(proportion).ToString() + "/7000");
                */
            }
        }
    }

    public class ModConfig : Config
    {
        public int TickIntervalOutdoors { get; set; }
        public int TickIntervalIndoors { get; set; }
        public int TickIntervalInMines { get; set; }
        public bool FreezeTimeInMines { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            TickIntervalOutdoors = 21;
            TickIntervalIndoors = 28;
            TickIntervalInMines = 35;
            FreezeTimeInMines = false;

            return this;
        }
    }
}
