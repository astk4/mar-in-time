using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarInTime.Domain.Entities
{
    [Table("eez_v12")]
    public class ExclusiveEconomicZone : SpatialEntity
    {
        /// <summary>
        /// Internal primary key for the zone, essential for technical performance of the mapping.
        /// </summary>
        [Key]
        [Column("gid")]
        public override int GID { get; set; }

        /// <summary>
        /// Marine Regions Geographic Identifier of the polygon feature (= of the whole zone).
        /// </summary>
        [Column("mrgid")]
        public int MRGID { get; set; }

        /// <summary>
        /// Name of the polygon feature.
        /// </summary>
        [Column("geoname")]
        public required string Name { get; set; }

        /// <summary>
        /// Type of polygon area.
        /// </summary>
        [Column("pol_type")]
        public required string Type { get; set; }

        /// <summary>
        /// Name of the area which directly relates to the polygon.
        /// </summary>
        [Column("territory1")]
        public required string Territory { get; set; }

        /// <summary>
        /// MRGID for the directly related territory.
        /// </summary>
        [Column("mrgid_ter1")]
        public int? MRGID_Territory {  get; set; }

        /// <summary>
        /// ISO3 code for the directly related territory.
        /// </summary>
        [Column("iso_ter1")]
        [Length(3, 3)]
        public string? ISO3_Territory {  get; set; }

        /// <summary>
        /// UN code (M49 Country code) for the directly related territory.
        /// </summary>
        [Column("un_ter1")]
        public int? UN_M49_Territory { get; set; }

        /// <summary>
        /// State that has jurisdiction over the directly related territory.
        /// </summary>
        [Column("sovereign1")]
        public required string Sovereign {  get; set; }

        /// <summary>
        /// MRGID for the main territory sovereign.
        /// </summary>
        [Column("mrgid_sov1")]
        public int MRGID_Sovereign { get; set; }

        /// <summary>
        /// ISO3 code for the main territory sovereign.
        /// </summary>
        [Column("iso_sov1")]
        [Length(3, 3)]
        public required string ISO3_Sovereign { get; set; }

        /// <summary>
        /// UN code (M49 Country code) for the main territory sovereign.
        /// </summary>
        [Column("un_sov1")]
        public int UN_M49_Sovereign { get; set; }

        /// <summary>
        /// Area of polygon in square kilometers.
        /// </summary>
        [Column("area_km2")]
        public double AreaKm2 { get; set; }

        /// <summary>
        /// Centroid longitude of polygon.
        /// </summary>
        [Column("x_1")]
        public double CentroidLongitude { get; set; }

        /// <summary>
        /// Centroid latitude of polygon.
        /// </summary>
        [Column("y_1")]
        public double CentroidLatitude { get; set; }


        /// <summary>
        /// Name of another area which relates to the polygon, for joint regimes and overlapping claims.
        /// </summary>
        [Column("territory2")]
        public string? Territory2 { get; set; }

        /// <summary>
        /// MRGID for another related territory, if any.
        /// </summary>
        [Column("mrgid_ter2")]
        public int? MRGID_Territory2 { get; set; }

        /// <summary>
        /// ISO3 code for another related territory, if any.
        /// </summary>
        [Column("iso_ter2")]
        [Length(3, 3)]
        public string? ISO3_Territory2 { get; set; }

        /// <summary>
        /// UN code (M49 Country code) for another related territory, if any.
        /// </summary>
        [Column("un_ter2")]
        public int? UN_M49_Territory2 { get; set; }

        /// <summary>
        /// State that has jurisdiction over another related territory, if any.
        /// </summary>
        [Column("sovereign2")]
        public string? Sovereign2 { get; set; }

        /// <summary>
        /// MRGID for the extra territory sovereign, if any.
        /// </summary>
        [Column("mrgid_sov2")]
        public int? MRGID_Sovereign2 { get; set; }

        /// <summary>
        /// ISO3 code for the extra territory sovereign, if any.
        /// </summary>
        [Column("iso_sov2")]
        [Length(3, 3)]
        public string? ISO3_Sovereign2 { get; set; }

        /// <summary>
        /// UN code (M49 Country code) for the extra territory sovereign.
        /// </summary>
        [Column("un_sov2")]
        public int? UN_M49_Sovereign2 { get; set; }


        /// <summary>
        /// Name of another area which relates to the polygon, for joint regimes and overlapping claims.
        /// </summary>
        [Column("territory3")]
        public string? Territory3 { get; set; }

        /// <summary>
        /// MRGID for another related territory, if any.
        /// </summary>
        [Column("mrgid_ter3")]
        public int? MRGID_Territory3 { get; set; }

        /// <summary>
        /// ISO3 code for another related territory, if any.
        /// </summary>
        [Column("iso_ter3")]
        [Length(3, 3)]
        public string? ISO3_Territory3 { get; set; }

        /// <summary>
        /// UN code (M49 Country code) for another related territory, if any.
        /// </summary>
        [Column("un_ter3")]
        public int? UN_M49_Territory3 { get; set; }

        /// <summary>
        /// State that has jurisdiction over another related territory, if any.
        /// </summary>
        [Column("sovereign3")]
        public string? Sovereign3 { get; set; }

        /// <summary>
        /// MRGID for the extra territory sovereign, if any.
        /// </summary>
        [Column("mrgid_sov3")]
        public int? MRGID_Sovereign3 { get; set; }

        /// <summary>
        /// ISO3 code for the extra territory sovereign, if any.
        /// </summary>
        [Column("iso_sov3")]
        [Length(3, 3)]
        public string? ISO3_Sovereign3 { get; set; }

        /// <summary>
        /// UN code (M49 Country code) for the extra territory sovereign.
        /// </summary>
        [Column("un_sov3")]
        public int? UN_M49_Sovereign3 { get; set; }

    }
}
