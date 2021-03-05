using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public class AfterSimulationGroup : ComponentSystemGroup { }