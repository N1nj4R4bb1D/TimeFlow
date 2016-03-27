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
        public bool FreezeTimeToggle = false;

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            TimeFlowConfig = new ModConfig();
            TimeFlowConfig = (ModConfig)Config.InitializeConfig(PathOnDisk + "\\Config.json", TimeFlowConfig);
#if DEBUG
            Console.WriteLine("The config file for TimeFlow has been loaded.\n" +
                "\n\tTickIntervalOutdoors: {0}" +
                "\n\tTickIntervalIndoors: {1}" +
                "\n\tTickIntervalFarmIndoors: {2}" +
                "\n\tTickIntervalInMines: {3}" +
                "\n\tFreezeTimeToggleKey: {4}",
                TimeFlowConfig.TickIntervalOutdoors, TimeFlowConfig.TickIntervalIndoors, TimeFlowConfig.TickIntervalFarmIndoors,
                TimeFlowConfig.TickIntervalInMines, TimeFlowConfig.FreezeTimeToggleKey);
            Console.WriteLine("\nTimeFlow Initialization Completed");
#endif
            TenMinuteTickInterval = TimeFlowConfig.TickIntervalIndoors;
        }

        [Subscribe]
        public void Pre10MinuteClockUpdateCallback(Pre10MinuteClockUpdateEvent @event)
        {
#if DEBUG
            Console.WriteLine("TimeFlow : 10MinuteClockUpdate : " + DateTime.Now.ToString("HH:mm:ss.ffff") +
                "\n\tFreezeTimeToggle : " + FreezeTimeToggle.ToString());
#endif
            var location = @event.Root.CurrentLocation;
            if (location != null && FreezeTimeToggle)
                @event.ReturnEarly = true;
            timeCounter = 0;
            lastGameTimeInterval = 0;
        }

        [Subscribe]
        public void UpdateGameClockCallback(UpdateGameClockEvent @event)
        {
            if (@event.Root.DayOfMonth != null && @event.Root.CurrentSeason != null)
            {
                timeCounter += Math.Abs((@event.Root.GameTimeInterval - lastGameTimeInterval));
                double fraction = (timeCounter / TenMinuteTickInterval);
                double proportion;

                if (!@event.Root.CurrentLocation.IsOutdoors)
                    switch (@event.Root.CurrentLocation.Name)
                    {
                        case "Coop":
                        case "Barn":
                        case "FarmCave":
                        case "FarmHouse":
                        case "Greenhouse":
                            timeCounter = (!TimeFlowConfig.TickIntervalFarmIndoors.Equals(TenMinuteTickInterval)) ? (TimeFlowConfig.TickIntervalFarmIndoors * fraction) : timeCounter;
                            TenMinuteTickInterval = TimeFlowConfig.TickIntervalFarmIndoors;
                            break;
                        case "UndergroundMine":
                            timeCounter = (!TimeFlowConfig.TickIntervalInMines.Equals(TenMinuteTickInterval)) ? (TimeFlowConfig.TickIntervalInMines * fraction) : timeCounter;
                            TenMinuteTickInterval = TimeFlowConfig.TickIntervalInMines;
                            break;
                        default:
                            timeCounter = (!TimeFlowConfig.TickIntervalIndoors.Equals(TenMinuteTickInterval)) ? (TimeFlowConfig.TickIntervalIndoors * fraction) : timeCounter;
                            TenMinuteTickInterval = TimeFlowConfig.TickIntervalIndoors;
                            break;
                    }
                else
                {
                    timeCounter = (!TimeFlowConfig.TickIntervalOutdoors.Equals(TenMinuteTickInterval)) ? (TimeFlowConfig.TickIntervalOutdoors * fraction) : timeCounter;
                    TenMinuteTickInterval = TimeFlowConfig.TickIntervalOutdoors;
                }

                proportion = (7 * timeCounter / TenMinuteTickInterval);
                @event.Root.GameTimeInterval = Convert.ToInt32(proportion);
                lastGameTimeInterval = @event.Root.GameTimeInterval;
#if DEBUG
                if (lastGameTimeInterval % 100 == 0)
                    Console.WriteLine("TimeFlow : " + @event.Root.CurrentLocation.Name + " : " + DateTime.Now.ToString("HH:mm:ss.ffff") +
                        "\n\ttimeCounter : " + Convert.ToInt32(timeCounter).ToString() + "/" + (TenMinuteTickInterval * 1000).ToString() +
                        "\n\tproportion : " + Convert.ToInt32(proportion).ToString() + "/7000" +
                        "\n\tTimeOfDay : " + @event.Root.TimeOfDay.ToString("G"));
#endif
            }
    }

        [Subscribe]
        public void KeyPressedCallback(KeyPressedEvent @event)
        {
            if (@event.Key.ToString().Equals(TimeFlowConfig.FreezeTimeToggleKey))
                FreezeTimeToggle = (FreezeTimeToggle) ? false : true;
#if DEBUG
            else if (@event.Key.ToString().Equals("Add") && !@event.Root.TimeOfDay.Equals(200))
            {
                @event.Root.TimeOfDay += (@event.Root.TimeOfDay % 50 == 0 && @event.Root.TimeOfDay % 100 != 0) ? 50 : 10;
                @event.Root.GameTimeInterval = 0;
                lastGameTimeInterval = 0;
                timeCounter = 0;
            }
#endif
        }
    }

    public class ModConfig : Config
    {
        public int TickIntervalOutdoors { get; set; }
        public int TickIntervalIndoors { get; set; }
        public int TickIntervalFarmIndoors { get; set; }
        public int TickIntervalInMines { get; set; }
        public string FreezeTimeToggleKey { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            TickIntervalOutdoors = 21;
            TickIntervalIndoors = 28;
            TickIntervalFarmIndoors = 35;
            TickIntervalInMines = 35;
            FreezeTimeToggleKey = "Pause";

            return this;
        }
    }
}
