/// <summary>
/// 
/// </summary>
namespace WestervilleFoodBot.Bots
{
    using Newtonsoft.Json.Linq;
    using System;

    public class Restaurant
    {
        readonly JToken name;
        readonly JToken stars;
        readonly JToken icon;

        public Restaurant (JToken _name, JToken _stars, JToken _icon)
        {
            name = _name;
            stars = _stars;
            icon = _icon;
        }

        public string GetName()
        {
            return name.ToString();
        }

        public string GetStars()
        {
            return stars.ToString();
        }

        public Uri GetIcon()
        {
            return new Uri(icon.ToString());
        }
    }
}
