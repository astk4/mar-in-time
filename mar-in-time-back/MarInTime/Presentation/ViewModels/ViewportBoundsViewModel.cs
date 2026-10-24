using System.Text.Json.Serialization;

namespace MarInTime.Presentation.ViewModels
{
    /// <summary>
    /// Rectangular bounding box on the map
    /// </summary>
    public class ViewportBoundsViewModel
    {
        /// <summary>
        /// Westmost longitude of the bounding box
        /// </summary>
        public double West {  get; set; }

        /// <summary>
        /// Southernmost latitude of the bounding box
        /// </summary>
        public double South { get; set; }

        /// <summary>
        /// Easternmost longitude of the bounding box
        /// </summary>
        public double East { get; set; }

        /// <summary>
        /// Northmost latitude of the bounding box
        /// </summary>
        public double North { get; set; }
    }
}
