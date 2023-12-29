namespace LineWars.Model
{
    public static class IntExtension
    {
        public static int GetValueInRind(this int value, int module)
        {
            var temp = value % module;
            return temp < 0 ? temp + module : temp;
        }
    }
}