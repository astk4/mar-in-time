CREATE TABLE IF NOT EXISTS AIS_History
(
    SessionId UInt32 CODEC(Delta, LZ4),
    SessionStartTime DateTime64 CODEC(Delta(4), LZ4),
    SessionElapsedMillis UInt64 CODEC(Delta, LZ4),
    
    MMSI UInt32 CODEC(ZSTD),
    RepeatIndicator Bool,
    AisMessageType UInt8,
    
    Position_Lat Nullable(Double) CODEC(ZSTD(3)),
    Position_Long Nullable(Double) CODEC(ZSTD(3)),
    Position_Accuracy Nullable(Bool),
    Position_NavigationStatus UInt8 CODEC(ZSTD(1)),
    Position_RateOfTurn Int8 CODEC(ZSTD(1)),
    Position_CourseOverGround Nullable(Double) CODEC(ZSTD(3)),
    Position_SpeedOverGround Nullable(Double) CODEC(ZSTD(3)),
    Position_TrueHeading Nullable(UInt16) CODEC(T64, ZSTD(3)),
    Position_Timestamp UInt8 CODEC(ZSTD(1)),
    Position_SpecialManeuverIndicator UInt8 CODEC(ZSTD(1)),
    
    Ship_Name Nullable(String) CODEC(LZ4),
    Ship_TypeId UInt8 CODEC(ZSTD(1)),
    Ship_Dimension_A Nullable(UInt16) CODEC(T64, ZSTD(3)),
    Ship_Dimension_B Nullable(UInt16) CODEC(T64, ZSTD(3)),
    Ship_Dimension_C Nullable(UInt16) CODEC(T64, ZSTD(3)),
    Ship_Dimension_D Nullable(UInt16) CODEC(T64, ZSTD(3)),
    Ship_CallSign Nullable(String) CODEC(LZ4),
    
    Ship_IMO Nullable(UInt32) CODEC(ZSTD),
    Ship_ETA_Min UInt8 CODEC(ZSTD(1)),
    Ship_ETA_Hour UInt8 CODEC(ZSTD(1)),
    Ship_ETA_Day UInt8 CODEC(ZSTD(1)),
    Ship_ETA_Month UInt8 CODEC(ZSTD(1)),
    Ship_MaxStaticDraught Nullable(Double) CODEC(ZSTD(3)),
    Ship_DestinationName Nullable(String) CODEC(LZ4),
    
    Ship_VendorIdName Nullable(String) CODEC(LZ4),
    Ship_VendorIdSerial Nullable(UInt32) CODEC(ZSTD),
    Ship_VendorIdModel UInt8 CODEC(ZSTD(1)),
    
    Safety_Text Nullable(String) CODEC(LZ4),
    Safety_DestinationID Nullable(UInt32) CODEC(ZSTD),
    Safety_Retransmission Nullable(Bool)
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(SessionStartTime)
ORDER BY (SessionId, SessionElapsedMillis);