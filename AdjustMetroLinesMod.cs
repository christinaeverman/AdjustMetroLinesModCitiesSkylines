using ICities;
using ColossalFramework;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
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
      private const int MAX_PASSENGERS_STATION = 100;
      private const int MIN_PASSENGERS_STATION = 50;
      private const int MIN_PASSENGERS_TRAIN = 100;
      private TransportManager _transportManager;
      private FastList<TransportLine> _metroLines;
      private VehicleManager _vehicleManager;
      private SimulationManager _simulationManager;
      private float _startTime = 0f;

      public override void OnCreated(IThreading threading)
      {
         _transportManager = Singleton<TransportManager>.instance;
         _simulationManager = Singleton<SimulationManager>.instance;
         _vehicleManager = Singleton<VehicleManager>.instance;
         _metroLines = new FastList<TransportLine>();
      }

      public override void OnUpdate(float realTimeData, float simulationTimeDelta)
      {
         try
         {
            if (_startTime > 20)
            {
               _startTime = 0;

               // find all metro lines
               for (int i = 0; i < _transportManager.m_lines.m_size; i++)
               {
                  if (_vehicleManager.m_vehicles.m_buffer[_transportManager.m_lines.m_buffer[i].GetVehicle(0)].Info
                         .m_vehicleType == VehicleInfo.VehicleType.Metro)
                  {
                     _metroLines.Add(_transportManager.m_lines.m_buffer[i]);
                  }
               }

               DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Current total number of metro lines is " + _metroLines.m_size.ToString());

               // iterate through all metro lines
               for (int i = 0; i < _metroLines.m_size; i++)
               {
                  DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Number of stops on line " + _metroLines.m_buffer[i].m_lineNumber.ToString() + " is " + _metroLines.m_buffer[i].CountStops(_metroLines.m_buffer[i].m_infoIndex));
                  
                  // iterate through all stops for any given metro line
                  for (int j = 0; j < _metroLines.m_buffer[i].CountStops(_metroLines.m_buffer[i].m_infoIndex); j++)
                  {
                     // check if another metro vehicle is needed for the provided metro line
                     if (_metroLines.m_buffer[i].CalculatePassengerCount(_metroLines.m_buffer[i].GetStop(j)) > MAX_PASSENGERS_STATION)
                     {
                        // add new metro vehicle to current metro line
                        DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Adding new metro vehicle to metro line " + _metroLines.m_buffer[i].m_lineNumber.ToString());
                        //_metroLines.m_buffer[i].m_vehicles++;
                        DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "m_infoIndex: " + _metroLines.m_buffer[i].m_infoIndex + ", Now there are " + _metroLines.m_buffer[i].CountVehicles(_metroLines.m_buffer[i].m_infoIndex) + " vehicles on this line"); // this is the number of current vehicles that appears on the line, not total number that should be there at the moment
                        break;
                     }
                  }
               }
               _metroLines.Clear();
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
