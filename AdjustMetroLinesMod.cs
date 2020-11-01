using ICities;
using ColossalFramework;
using UnityEngine;
using System;
using ColossalFramework.IO;
using ColossalFramework.Math;
using ColossalFramework.Plugins;

namespace AdjustMetroLines
{
	public class AdjustMetroLinesMod : IUserMod
	{
      public string Name
      {
         get { return "Adjust Metro Lines"; }
      }

      public string Description
      {
         get
         {
            return
               "This mod automatically adjusts the number of metro trains depending on the number " +
               "of passengers currently riding the trains and waiting in the stations.";
         }
      }
   }

   public class AdjustMetroLinesThreading : ThreadingExtensionBase
   {
      private const int MAX_PASSENGERS_STATION_THRESHOLD = 100;
      private TransportManager _transportManager;
      private Array16<TransportLine> _metroLines;
      private VehicleManager _vehicleManager;
      private SimulationManager _simulationManager;
      private float _startTime = 0f;

      public override void OnCreated(IThreading threading)
      {
         _transportManager = Singleton<TransportManager>.instance;
         _simulationManager = Singleton<SimulationManager>.instance;
         _vehicleManager = Singleton<VehicleManager>.instance;
         _metroLines = _transportManager.m_lines;
      }

      public override void OnUpdate(float realTimeData, float simulationTimeDelta)
      {
         try
         {
            if (_startTime > 20)
            {
               _startTime = 0;
               DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Reset");

               // iterate through all metro lines
               for (int i = 0; i < _metroLines.m_size; i++)
               {
                  // iterate through all stops for any given metro line
                  for (int j = 0; j < _metroLines.m_buffer[i].m_stops; j++)
                  {
                     //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Line " + i + " stop " + j);
                     // check if another metro vehicle is needed for the provided metro line
                     if (_metroLines.m_buffer[i].CalculatePassengerCount(_metroLines.m_buffer[i].GetStop(j)) > MAX_PASSENGERS_STATION_THRESHOLD)
                     {
                        // add new metro vehicle to current metro line
                        //ushort newVehicleID = _vehicleManager.m_vehicles.NextFreeItem(ref _simulationManager.m_randomizer);
                        DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Adding new metro vehicle to metro line " + _metroLines.m_buffer[i].m_lineNumber.ToString());
                        _metroLines.m_buffer[i].m_vehicles++;
                        DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Now there are " + _metroLines.m_buffer[i].m_vehicles + " vehicles on this line");
                        //_metroLines.m_buffer[i].AddVehicle(_metroLines.m_buffer[i].GetVehicle(_metroLines.m_buffer[i].m_vehicles - 1), 
                        //   _vehicleManager.m_vehicles.m_buffer[_metroLines.m_buffer[i].GetVehicle(_metroLines.m_buffer[i].m_vehicles - 1)].Info.0);
                        //_metroLines.m_buffer[i].AddVehicle(newVehicleID, ref _vehicleManager.m_vehicles.m_buffer[newVehicleID], true);
                        break;
                     }
                  }
               }
            }

            _startTime += simulationTimeDelta;
         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "Problem with checking metro lines");
         }
      }
   }
}
