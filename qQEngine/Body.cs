namespace qQEngine;

public class Body(string identifier, SpatialInfo spatialInfo) 
    : KinematicObject(identifier, spatialInfo)
{
    public double Temperature { get; set; }
    public SDecimal Mass { get; set; }
    public SDecimal Charge { get; set; }
}