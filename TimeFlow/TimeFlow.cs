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
        public string lastLocationName = "";
        public int TenMinuteTickInterval = 7;

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            TimeFlowConfig = new ModConfig();
            TimeFlowConfig = (ModConfig)Config.InitializeConfig(PathOnDisk + "\\Config.json", TimeFlowConfig);
            Console.WriteLine("The config file for TimeFlow has been loaded."+
                "\n\tTickIntervalOutside: {0}"+
                "\n\tTickIntervalInside: {1}"+
                "\n\tTickIntervalInMines: {2}"+
                "\n\tFreezeTimeInMines: {3}",
                TimeFlowConfig.TickIntervalOutside, TimeFlowConfig.TickIntervalInside, TimeFlowConfig.TickIntervalInMines, TimeFlowConfig.FreezeTimeInMines);
            Console.WriteLine("TimeFlow Initialization Completed");
            TenMinuteTickInterval = TimeFlowConfig.TickIntervalInside;
        }

        [Subscribe]
        public void Pre10MinuteClockUpdateCallback(Pre10MinuteClockUpdateEvent @event)
        {
            Console.WriteLine("TimeFlow : 10MinuteClockUpdate : " + DateTime.Now.ToString("HH:mm:ss.ffff"));
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
                            timeCounter = Math.Abs(TimeFlowConfig.TickIntervalInMines * fraction);
                            proportion = Math.Abs(7 * timeCounter / TimeFlowConfig.TickIntervalInMines);
                            break;
                        default:
                            timeCounter = Math.Abs(TimeFlowConfig.TickIntervalInside * fraction);
                            proportion = Math.Abs(7 * timeCounter / TimeFlowConfig.TickIntervalInside);
                            break;
                    }
                else
                {
                    timeCounter = Math.Abs(TimeFlowConfig.TickIntervalOutside * fraction);
                    proportion = Math.Abs(7 * timeCounter / TimeFlowConfig.TickIntervalOutside);
                }
                @event.Root.GameTimeInterval = Convert.ToInt32(proportion);
                lastGameTimeInterval = @event.Root.GameTimeInterval;
                if (proportion % 10 == 0)
                    Console.WriteLine("TimeFlow : " + @event.Root.CurrentLocation.Name+
                        " : " + Convert.ToInt32(proportion).ToString() + "/7000"+
                        " : " + DateTime.Now.ToString("HH:mm:ss.ffff"));
                /*
                if (LocationChangeCheck(@event.Root.CurrentLocation.Name, @event.Root.CurrentLocation.IsOutdoors))
                {
                    timeCounter = Math.Abs(TenMinuteTickInterval * fraction);
                    Console.WriteLine("TimeFlow : LocationChange : " + @event.Root.CurrentLocation.Name+
                        " : TickInterval = " + TenMinuteTickInterval.ToString()+
                        " : " + DateTime.Now.ToString("HH:mm:ss.ffff"));
                }
                */
            }
        }
    }

    public class ModConfig : Config
    {
        public int TickIntervalOutside { get; set; }
        public int TickIntervalInside { get; set; }
        public int TickIntervalInMines { get; set; }
        public bool FreezeTimeInMines { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            TickIntervalOutside = 21;
            TickIntervalInside = 28;
            TickIntervalInMines = 28;
            FreezeTimeInMines = false;

            return this;
        }
    }
}
