using System;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace TimeFlow
{
    public class TimeFlow : Mod
    {
        public static ModConfig TimeFlowConfig { get; private set; }
        public double timeCounter = 0;
        public double lastGameTimeInterval = 0;
        public int TenMinuteTickInterval = 7;
        public bool FreezeTimeToggle = false;

        public override void Entry(params object[] objects)
        {
            TimeFlowConfig = new ModConfig().InitializeConfig(BaseConfigPath);
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] TimeFlow : Loaded");
            ControlEvents.KeyPressed += Event_KeyPressed;
            TimeEvents.DayOfMonthChanged += Event_DayOfMonthChanged;
            TimeEvents.TimeOfDayChanged += Event_TimeOfDayChanged;
            GameEvents.UpdateTick += Event_UpdateTick;
        }

        public void Event_DayOfMonthChanged(object sender, EventArgs e)
        {
            Game1.gameTimeInterval = 0;
            timeCounter = 0;
            lastGameTimeInterval = 0;
        }

        public void Event_TimeOfDayChanged(object sender, EventArgs e)
        {
            Game1.gameTimeInterval = 0;
            timeCounter = 0;
            lastGameTimeInterval = 0;
        }

        public void Event_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation != null)
            {
                if (FreezeTimeToggle)
                {
                    Game1.gameTimeInterval = Convert.ToInt32(lastGameTimeInterval);
                }
                else
                {
                    timeCounter += Math.Abs((Game1.gameTimeInterval - lastGameTimeInterval));
                    double fraction = (timeCounter / TenMinuteTickInterval);
                    double proportion;

                    if (!Game1.currentLocation.IsOutdoors)
                        switch (Game1.currentLocation.Name)
                        {
                            case "Coop":
                            case "Barn":
                            case "FarmCave":
                            case "FarmHouse":
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
                    Game1.gameTimeInterval = Convert.ToInt32(proportion);
                    lastGameTimeInterval = Game1.gameTimeInterval;
#if DEBUG
                    if (lastGameTimeInterval % 100 == 0)
                        Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] TimeFlow : " + Game1.currentLocation.Name +
                            "\n\ttimeCounter : " + Convert.ToInt32(timeCounter).ToString() + "/" + (TenMinuteTickInterval * 1000).ToString() +
                            "\n\tproportion : " + Convert.ToInt32(proportion).ToString() + "/7000" +
                            "\n\tTimeOfDay : " + Game1.timeOfDay.ToString("G"));
#endif
                }
            }
        }

        void Event_KeyPressed(object sender, EventArgsKeyPressed e)
        {
#if DEBUG
            //Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] TimeFlow : KeyPressed : " + e.KeyPressed.ToString());
#endif
            if (e.KeyPressed.ToString().Equals(TimeFlowConfig.FreezeTimeToggleKey))
            {
                FreezeTimeToggle = (FreezeTimeToggle) ? false : true;
                string message = (FreezeTimeToggle) ? "Time flow frozen..." : "Time flow thawed...";
                Game1.showGlobalMessage(message);
            }
        }
    }

    public class ModConfig : Config
    {
        public int TickIntervalOutdoors { get; set; }
        public int TickIntervalIndoors { get; set; }
        public int TickIntervalFarmIndoors { get; set; }
        public int TickIntervalInMines { get; set; }
        public string FreezeTimeToggleKey { get; set; }

        public override T GenerateDefaultConfig<T>()
        {
            TickIntervalOutdoors = 21;
            TickIntervalIndoors = 28;
            TickIntervalFarmIndoors = 35;
            TickIntervalInMines = 35;
            FreezeTimeToggleKey = "Pause";

            return this as T;
        }
    }
}
